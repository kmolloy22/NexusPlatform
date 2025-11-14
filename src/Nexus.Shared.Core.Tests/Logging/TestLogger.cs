using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Xunit.Abstractions;

namespace Nexus.Shared.Core.Tests.Logging;

public interface ITestLogger : ILogger<TestLogger>
{
    bool HasLog(Func<LogStatement, bool> predicate);
}

public class TestLogger : ITestLogger
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly List<LogStatement> _statements = new List<LogStatement>();

    public TestLogger(ITestOutputHelper testOutputHelper = null)
    {
        _testOutputHelper = testOutputHelper;
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
        Exception exception, Func<TState, Exception, string> formatter)
    {
        var statement = new LogStatement
        {
            Exception = exception,
            Message = state.ToString(),
            LogLevel = logLevel
        };

        _statements.Add(statement);

        var output = statement.ToString();

        Console.WriteLine(output);
        Trace.WriteLine(output);
        _testOutputHelper?.WriteLine(output);
    }

    public bool HasLog(Func<LogStatement, bool> predicate)
    {
        return _statements.Any(predicate);
    }

    public LogStatement[] GetLogs() => _statements.ToArray();
}