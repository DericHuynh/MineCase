var builder = DistributedApplication.CreateBuilder(args);

var mongodb = builder.AddMongoDB("mongodb")
                     .WithContainerName("mongodb-container")
                     .WithDbGate();
var minecase = mongodb.AddDatabase("minecase");

var silos = builder.AddProject<Projects.MineCase_Server>("minecase-server")
        .WithEndpoint(port: 30000, targetPort: 30000, isProxied: false)
        .WithHttpEndpoint(targetPort: 8080, name: "orleans-dashboard")
        .WithUrlForEndpoint("orleans-dashboard", (annotation) =>
        {
            annotation.DisplayText = "Orleans Dashboard";
        })
        .WithReference(minecase)
        .WaitFor(mongodb);

//builder.AddProject<Projects.MineCase_Gateway>("minecase-gateway")
//        .WithReference(mongodb)
//        .WithReference(silos)
//        .WaitFor(mongodb)
//        .WaitFor(minecase)
//        .WaitFor(silos);

builder.Build().Run();
