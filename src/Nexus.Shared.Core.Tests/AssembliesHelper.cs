using System.Reflection;

namespace Nexus.Shared.Core.Tests;

internal static class AssembliesHelper
{
    public static IList<Assembly> GetWhlsTestAssemblies()
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .Where(x => x.FullName != null && x.FullName.StartsWith("Nexus") && x.FullName.Contains("Tests"))
            .ToList();
    }
}