using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace MineCase.IntegrationTests.Tests
{
    public class IntegrationTest1
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

        // Instructions:
        // 1. Add a project reference to the target AppHost project, e.g.:
        //
        //    <ItemGroup>
        //        <ProjectReference Include="../MyAspireApp.AppHost/MyAspireApp.AppHost.csproj" />
        //    </ItemGroup>
        //
        // 2. Uncomment the following example test and update 'Projects.MyAspireApp_AppHost' to match your AppHost project:
        //
        [Fact]
        public async Task GetWebResourceRootReturnsOkStatusCode()
        {
            // Arrange
            var cancellationToken = TestContext.Current.CancellationToken;
            var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.MineCase_AppHost>(cancellationToken);

            appHost.Services.AddLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Debug);
                // Override the logging filters from the app's configuration
                logging.AddFilter(appHost.Environment.ApplicationName, LogLevel.Debug);
                logging.AddFilter("Aspire.", LogLevel.Debug);
                // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging
            });

            await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);
            await app.StartAsync(cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

            // Act
            var tcpClient = new TcpClient(new IPEndPoint(IPAddress.Parse("0.0.0.0"), 25565));
            await app.ResourceNotifications.WaitForResourceHealthyAsync("webfrontend", cancellationToken).WaitAsync(DefaultTimeout, cancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
