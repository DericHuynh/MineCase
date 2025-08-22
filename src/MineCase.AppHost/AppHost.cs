var builder = DistributedApplication.CreateBuilder(args);

var mongodb = builder.AddMongoDB("mongodb")
                     .WithImage("mongo", "4.0.27")
                     .WithContainerName("mongodb-container")
                     .WithDbGate();

var minecaseDb = mongodb.AddDatabase("mongodb-minecase");

var silos = builder.AddProject<Projects.MineCase_Server>("minecase-server")
                   .WithHttpEndpoint(targetPort: 8080, name:"orleans-dashboard")
                   .WithUrlForEndpoint("orleans-dashboard", (annotation) =>
                   {
                       annotation.DisplayText = "Orleans Dashboard";
                   })
                   .WithEnvironment("persistenceOptions:connectionString", minecaseDb)
                   .WaitFor(mongodb);

var gateway = builder.AddProject<Projects.MineCase_Gateway>("minecase-gateway")
                     .WithEnvironment("persistenceOptions:connectionString", minecaseDb)
                     .WaitFor(silos);

builder.Build().Run();
