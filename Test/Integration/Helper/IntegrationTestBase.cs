namespace Test.Integration.Helper
{
    public abstract class IntegrationTestBase(DatabaseFixture fixture) : IAsyncLifetime
    {
        protected readonly DatabaseFixture Fixture = fixture;

        public Task InitializeAsync() => Fixture.ResetAsync();

        public Task DisposeAsync()
        {
            Fixture.Context.ChangeTracker.Clear();

            return Task.CompletedTask;
        }
    }
}
