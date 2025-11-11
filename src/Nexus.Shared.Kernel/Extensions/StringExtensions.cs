using System.Diagnostics;
using static System.String;

namespace Nexus.Shared.Kernel.Extensions;

public static class StringExtensions
{
    [DebuggerStepThrough]
    public static bool IsMissing(this string value)
    {
        return IsNullOrWhiteSpace(value);
    }

    [DebuggerStepThrough]
    public static bool IsPresent(this string value)
    {
        return !IsNullOrWhiteSpace(value);
    }
}