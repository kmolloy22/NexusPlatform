namespace Nexus.Infrastructure.StorageAccount.Tests.Units;

public abstract class ClientTestsBase : IAsyncLifetime
{
    private readonly List<Func<Task>> _cleanupActions = new List<Func<Task>>();

    protected void AddCleanupAction(Func<Task> action)
    {
        _cleanupActions.Add(action);
    }

    public virtual Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _cleanupActions.Reverse();
        var exceptions = new List<Exception>();

        foreach (var action in _cleanupActions)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                Console.WriteLine("Cleanup action failed: " + ex);
            }
        }

        _cleanupActions.Clear();

        if (exceptions.Count == 0)
            return;

        if (exceptions.Count == 1)
            throw exceptions.Single();

        throw new AggregateException("Multiple exceptions occured in Cleanup. See test log for more details", exceptions);
    }
}