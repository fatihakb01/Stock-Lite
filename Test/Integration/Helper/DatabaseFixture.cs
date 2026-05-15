using Microsoft.Extensions.Logging.Abstractions;
using Infrastructure.Context;
using Infrastructure.Context.DatabaseInitializer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Respawn;
using Testcontainers.MsSql;

namespace Test.Integration.Helper
{
    public class DatabaseFixture : IAsyncLifetime
    {
        private readonly string _password;
        private readonly MsSqlContainer _container = null!;
        private Respawner _respawner = null!;

        public DataContext Context { get; private set; } = null!;
        public string ConnectionString { get; private set; } = null!;

        public DatabaseFixture()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: false)
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection")!;

            _password = connectionString
                .Split(';')
                .Select(part => part.Trim())
                .Where(part => part.StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
                .Select(part => part["Password=".Length..])
                .FirstOrDefault() ?? throw new InvalidOperationException(
                    "Password not found in connection string.");

            string image = config["Image"]!;

            _container = new MsSqlBuilder(image)
                .WithPassword(_password)
                .Build();
        }


        public async Task InitializeAsync()
        {
            await _container.StartAsync();

            ConnectionString = _container.GetConnectionString();

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlServer(ConnectionString)
                .Options;

            Context = new DataContext(options);

            var logger = NullLogger.Instance;

            // Apply all migrations to spin up the schema
            await Context.Database.MigrateAsync();

            // Create DB objects
            await DbInitializer.InitializeAsync(Context, logger);

            // Set up Respawn to reset data between tests
            var connection = Context.Database.GetDbConnection();
            await connection.OpenAsync();

            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                // Exclude migration history table from reset
                TablesToIgnore = ["__EFMigrationsHistory"]
            });

            await connection.CloseAsync();
        }

        public async Task ResetAsync()
        {
            var connection = Context.Database.GetDbConnection();

            if (connection.State != System.Data.ConnectionState.Open)
                await connection.OpenAsync();

            await _respawner.ResetAsync(connection);
        }

        public async Task DisposeAsync()
        {
            await Context.DisposeAsync();
            await _container.DisposeAsync();
        }
    }
}