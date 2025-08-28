using System.Runtime.InteropServices;
using Orleans.Core.Internal;
using Orleans.Runtime; // Add this for IManagementGrain

namespace DashboardToy.Frontend.Data;

public class ClusterDiagnosticsService(IGrainFactory grainFactory)
{
    private readonly Dictionary<SiloAddress, int> _hostKeys = [];
    private readonly Dictionary<SiloAddress, HostDetails> _hostDetails = [];
    private readonly Dictionary<(GrainId GrainId, SiloAddress Silo), int> _activationNodeKeys = [];
    private readonly List<(GrainId GrainId, int HostKey)> _nodeInfo = [];
    private readonly Dictionary<Key, ulong> _edges = [];
    private IManagementGrain _managementGrain;
    private const int CrossSiloWeightMultiplier = 10;

    // Renamed for clarity, no functional change
    private readonly record struct HostDetails(int HostKey, int ActivationCount);

    public async ValueTask<CallGraph> GetGrainCallFrequencies()
    {
        if (_managementGrain is null)
            _managementGrain = grainFactory.GetGrain<IManagementGrain>(0);

        // Reset data for this run
        Reset();

        var maxEdgeValue = 0;
        var maxActivationCount = 0;

        var silos = (await _managementGrain.GetHosts(onlyActive: true)).Keys.OrderBy(s => s.ToString()).ToList();

        // Pass 1: Discover all silos and initialize their host details
        foreach (var silo in silos)
        {
            var hostKey = GetHostVertex(silo);
            _hostDetails[silo] = new(hostKey, 0); // Initialize activation count to 0
        }

        // Pass 2: Discover all activations from detailed statistics
        // This is important to find grains that might not be involved in recent calls
        var allStats = await _managementGrain.GetDetailedGrainStatistics();
        foreach (var activation in allStats)
        {
            // The old filter for key == 0 is no longer needed, as we now model stateless workers correctly.
            if (activation.GrainId.IsSystemTarget()) continue;

            // Ensure the activation's silo is one we are tracking
            if (!_hostKeys.TryGetValue(activation.SiloAddress, out var hostKey)) continue;

            // Get or create a unique node for this specific activation
            GetActivationNodeKey(activation.GrainId, activation.SiloAddress);

            // Increment the activation count for the host
            var details = _hostDetails[activation.SiloAddress];
            _hostDetails[activation.SiloAddress] = details with { ActivationCount = details.ActivationCount + 1 };
        }

        // Update maxActivationCount after counting
        if (_hostDetails.Count > 0)
        {
            maxActivationCount = _hostDetails.Values.Max(h => h.ActivationCount);
        }

        // Pass 3: Process all call edges
        foreach (var edge in await _managementGrain.GetGrainCallFrequencies())
        {
            if (edge.TargetGrain.IsSystemTarget() || edge.SourceGrain.IsSystemTarget()) continue;

            // For a StatelessWorker, edge.SourceHost tells us WHICH activation made the call.
            // Our new GetActivationNodeKey function uses this information to find the correct source node.
            var sourceNodeKey = GetActivationNodeKey(edge.SourceGrain, edge.SourceHost);
            var targetNodeKey = GetActivationNodeKey(edge.TargetGrain, edge.TargetHost);

            maxEdgeValue = Math.Max(maxEdgeValue, (int)edge.CallCount);
            UpdateEdge(new(sourceNodeKey, targetNodeKey), edge.CallCount);
        }



        var nodeRawWeights = new ulong[_nodeInfo.Count];
        foreach (var edge in _edges)
        {
            var sourceInfo = _nodeInfo[edge.Key.Source];
            var targetInfo = _nodeInfo[edge.Key.Target];

            // Start with the base call count
            ulong weightContribution = edge.Value;

            // If the call is cross-silo, boost its contribution to the weight.
            if (sourceInfo.HostKey != targetInfo.HostKey)
            {
                weightContribution *= CrossSiloWeightMultiplier;
            }

            nodeRawWeights[edge.Key.Source] += weightContribution;
            nodeRawWeights[edge.Key.Target] += weightContribution;
        }

        var nodeLogWeights = new double[_nodeInfo.Count];
        double maxLogWeight = 0;
        for (int i = 0; i < nodeRawWeights.Length; i++)
        {
            // The +1 prevents Math.Log(0) which is -Infinity
            nodeLogWeights[i] = Math.Log(nodeRawWeights[i] + 1);
            if (nodeLogWeights[i] > maxLogWeight)
            {
                maxLogWeight = nodeLogWeights[i];
            }
        }

        var grainNodes = new List<GraphNode>(_nodeInfo.Count);
        CollectionsMarshal.SetCount(grainNodes, _nodeInfo.Count);
        for (int i = 0; i < _nodeInfo.Count; i++)
        {
            var (grainId, hostKey) = _nodeInfo[i];
            var nodeName = $"{grainId}@{hostKey}";

            // 3. Normalize the log-scaled weight into our final 1.0 to 10.0 range.
            double finalWeight = 1.0;
            if (maxLogWeight > 0)
            {
                // A node with 0 calls has a weight of 1.0.
                // The busiest node has a weight of 10.0.
                finalWeight = 1.0 + 9.0 * (nodeLogWeights[i] / maxLogWeight);
            }

            grainNodes[i] = new(nodeName, grainId.Key.ToString()!, hostKey, finalWeight);
        }

        var hostNodes = new List<HostNode>(_hostKeys.Count);
        CollectionsMarshal.SetCount(hostNodes, _hostKeys.Count);
        foreach ((var hostId, var key) in _hostKeys)
        {
            var details = _hostDetails[hostId];
            hostNodes[key] = new(hostId.ToString(), details.ActivationCount);
        }

        var graphEdges = new List<GraphEdge>();
        foreach (var edge in _edges)
        {
            graphEdges.Add(new(edge.Key.Source, edge.Key.Target, edge.Value));
        }

        return new(grainNodes, hostNodes, graphEdges, maxEdgeValue, maxActivationCount);
    }

    internal void Reset()
    {
        // Keep host keys across runs for consistent coloring, but clear everything else.
        // If silos can join/leave, you might want to clear _hostKeys as well.
        _hostDetails.Clear();
        _activationNodeKeys.Clear();
        _nodeInfo.Clear();
        _edges.Clear();
    }

    // *** MODIFICATION START ***
    /// <summary>
    /// Gets a unique integer key for a specific grain activation on a specific silo.
    /// If the activation hasn't been seen before, it creates a new key and records its details.
    /// </summary>
    private int GetActivationNodeKey(GrainId grainId, SiloAddress silo)
    {
        // The key for our dictionary is the unique combination of grain and silo
        var activationKey = (grainId, silo);

        ref var nodeKey = ref CollectionsMarshal.GetValueRefOrAddDefault(_activationNodeKeys, activationKey, out var exists);
        if (!exists)
        {
            // This is a new activation we haven't seen before.
            // Its unique key will be its index in the _nodeInfo list.
            nodeKey = _nodeInfo.Count;
            var hostKey = GetHostVertex(silo);
            _nodeInfo.Add((grainId, hostKey));
        }

        return nodeKey;
    }
    // *** MODIFICATION END ***

    private int GetHostVertex(SiloAddress silo)
    {
        ref var key = ref CollectionsMarshal.GetValueRefOrAddDefault(_hostKeys, silo, out var exists);
        if (!exists)
        {
            key = _hostKeys.Count - 1;
        }

        return key;
    }

    private void UpdateEdge(Key key, ulong increment)
    {
        ref var count = ref CollectionsMarshal.GetValueRefOrAddDefault(_edges, key, out var exists);
        count += increment;
    }
}

// Data record definitions remain the same
public record class CallGraph(List<GraphNode> GrainIds, List<HostNode> HostIds, List<GraphEdge> Edges, int MaxEdgeValue, int MaxActivationCount);

public record struct HostNode(string Name, int ActivationCount);
public record struct GraphNode(string Name, string Key, int Host, double Weight);
public record struct Key(int Source, int Target);
public record struct GraphEdge(int Source, int Target, double Weight);