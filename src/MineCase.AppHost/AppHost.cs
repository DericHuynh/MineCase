var builder = DistributedApplication.CreateBuilder(args);

var mongodb = builder.AddMongoDB("mongodb")
                     .WithImage("mongo", "4.0.27")
                     .WithContainerName("mongodb-container")
                     .WithDbGate();

var minecaseDb = mongodb.AddDatabase("minecase"); // Database name dont change

var silos = builder.AddProject<Projects.MineCase_Server>("minecase-server")
                   //.WithHttpEndpoint(name: "orleans-dashboard")
                   //.WithUrlForEndpoint("orleans-dashboard", (annotation) =>
                   //{
                   //    annotation.DisplayText = "Orleans Dashboard";
                   //    annotation.Url = "/dashboard";
                   //})
                   .WithHttpEndpoint()
                   .WithHttpHealthCheck("/health")
                   .WithEnvironment("persistenceOptions:connectionString", minecaseDb)
                   .WithReplicas(2);

silos.WaitFor(mongodb);

var gateway = builder.AddProject<Projects.MineCase_Gateway>("minecase-gateway")
                     .WithEnvironment("persistenceOptions:connectionString", minecaseDb);

gateway.WaitFor(silos);

var healthchecksUi = builder.AddHealthChecksUI("healthchecks-ui")
                     .WithReference(silos)
                     .WithReference(gateway)
                     .WithReference(mongodb)
                     .WithExternalHttpEndpoints();

builder.Build().Run();
