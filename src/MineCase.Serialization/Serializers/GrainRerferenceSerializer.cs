using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Orleans;
using Orleans.Runtime;

namespace MineCase.Serialization.Serializers
{
    [Orleans.GenerateSerializer]
    public class GrainRerferenceSerializer<TInterface> : SealedClassSerializerBase<TInterface>
        where TInterface : class, IAddressable
    {
        [Id(0)]
        private IGrainFactory _grainFactory;

        public GrainRerferenceSerializer(IServiceProvider serviceProvider)
        {
            _grainFactory = serviceProvider.GetRequiredService<IGrainFactory>();
        }

        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, TInterface value)
        {
            var key = value.GetPrimaryKeyString();
            context.Writer.WriteString(key);
        }

        protected override TInterface DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            string key = context.Reader.ReadString();
            GrainId id = GrainId.Parse(key);
            TInterface grain = _grainFactory.GetGrain<TInterface>(id);
            return grain;
        }
    }

    [Orleans.GenerateSerializer]
    public class GrainRerferenceSerializerProvider : IBsonSerializationProvider
    {
        [Id(0)]
        private readonly ConcurrentDictionary<Type, IBsonSerializer> _bsonSerializers = new ConcurrentDictionary<Type, IBsonSerializer>();
        [Id(1)]
        private readonly Type _referType = typeof(IAddressable);
        [Id(2)]
        private readonly IServiceProvider _serviceProvider;
        [Id(3)]
        private readonly Type _serializerTypeGen = typeof(GrainRerferenceSerializer<>);

        public GrainRerferenceSerializerProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IBsonSerializer GetSerializer(Type type)
        {
            if (_referType.IsAssignableFrom(type))
                return _bsonSerializers.GetOrAdd(type, t => (IBsonSerializer)Activator.CreateInstance(_serializerTypeGen.MakeGenericType(t), _serviceProvider));
            return null;
        }
    }
}
