using Orleans.Configuration;
using Orleans.Hosting;
using System.Reflection.PortableExecutable;

var builder = WebApplication.CreateBuilder(args);

builder.UseOrleansClient(options =>
{
    options.Configure<ClusterOptions>(configure =>
    {
        configure.ClusterId = "dev";
        configure.ServiceId = "MineCaseService";
    });
    options.UseMongoDBClient(builder.Configuration.GetSection("persistenceOptions")["connectionString"]);
    options.UseMongoDBClustering(options =>
    {
        options.DatabaseName = builder.Configuration.GetSection("persistenceOptions")["databaseName"];
    });
});
builder.Services.AddControllers();

var app = builder.Build();

var options = new DefaultFilesOptions();
options.DefaultFileNames.Clear();
options.DefaultFileNames.Add("index.html");

app.UseDefaultFiles(options);
app.UseStaticFiles();
app.MapControllers();
app.Run();
