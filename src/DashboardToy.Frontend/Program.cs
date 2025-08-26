using DashboardToy.Frontend.Data;
using Microsoft.AspNetCore.Mvc;
using Orleans.Configuration;
using Orleans;

var builder = WebApplication.CreateBuilder(args);
#pragma warning disable ORLEANSEXP001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
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
#pragma warning restore ORLEANSEXP001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

// Add services to the container.
builder.Services.AddSingleton<ClusterDiagnosticsService>();

var app = builder.Build();

var clusterDiagnosticsService = app.Services.GetRequiredService<ClusterDiagnosticsService>();
app.MapGet("/data.json", ([FromServices] ClusterDiagnosticsService clusterDiagnosticsService) => clusterDiagnosticsService.GetGrainCallFrequencies());

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

await app.StartAsync();

await app.WaitForShutdownAsync();
