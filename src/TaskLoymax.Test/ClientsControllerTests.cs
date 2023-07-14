using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskLoymax.WebApi.Controllers;
using TaskLoymax.WebApi.Infrastructure;
using TaskLoymax.WebApi.Models;

namespace TaskLoymax.Test
{
    public class ClientsControllerTests : IDisposable
    {
        private DbContextOptions<ClientContext> _options;

        public ClientsControllerTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json")
                .Build();

            _options = new DbContextOptionsBuilder<ClientContext>()
                .UseMySQL(configuration.GetConnectionString("TestConnection"))
                .Options;

            using var context = new ClientContext(_options);
            context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            using var context = new ClientContext(_options);
            context.Database.EnsureDeleted();
        }

        [Fact]
        public async Task TestMultithreadedBalanceAdjustment()
        {
            // Arrange: Prepare data: register 50 clients.
            await using (var context = new ClientContext(_options))
            {
                for (var i = 0; i < 50; i++)
                {
                    context.Clients.Add(new Client
                    {
                        ClientId = Guid.NewGuid(),
                        FirstName = "",
                        LastName = "",
                        MiddleName = "",
                        Balance = 1000
                    });
                }

                await context.SaveChangesAsync();
            }

            // Act: Run the test: in 10 threads deposit and withdraw for these clients.
            var tasks = new List<Task>();
            for (var i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    await using var threadContext = new ClientContext(_options);
                    var controller = new ClientsController(threadContext);

                    foreach (var client in threadContext.Clients.ToList())
                    {
                        await controller.Deposit(client.ClientId, 100);
                        await controller.Withdraw(client.ClientId, 50);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            // Assert: Check the results.
            await using (var context = new ClientContext(_options))
            {
                foreach (var client in context.Clients.ToList())
                {
                    Assert.Equal(1050, client.Balance);  // expecting each client to have 1050 as their balance
                }
            }
        }
    }

}