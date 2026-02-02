namespace Nexus.Shared.Kernel.Extensions;

public static class NullableExtensions
{
    public static bool IsNull<T>(this T? value) where T : struct
        => !value.HasValue;
}