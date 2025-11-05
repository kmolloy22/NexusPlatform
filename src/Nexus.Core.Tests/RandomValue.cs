namespace Nexus.Core.Tests;

public class RandomValue
{
    public static Guid Guid => Guid.NewGuid();
    public static string String => Guid.ToString();
}