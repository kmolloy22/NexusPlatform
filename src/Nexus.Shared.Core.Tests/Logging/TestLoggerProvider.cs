using Microsoft.Extensions.Logging;

namespace Nexus.Shared.Core.Tests.Logging;

public class TestLoggerProvider : ILoggerProvider
{
    private readonly ITestLogger _logger;

    public TestLoggerProvider(ITestLogger logger)
    {
        _logger = logger;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _logger;
    }

    public void Dispose()
    {
    }
}