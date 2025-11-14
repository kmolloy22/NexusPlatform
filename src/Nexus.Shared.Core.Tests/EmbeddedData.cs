using Newtonsoft.Json;
using System.Reflection;

namespace Nexus.Shared.Core.Tests;

public static class EmbeddedData
{
    public static string AsString(string name)
    {
        var stream = EmbeddedDataSource.GetStream(name);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static byte[] AsBytes(string name)
    {
        var stream = EmbeddedDataSource.GetStream(name);
        byte[] buffer = new byte[16 * 1024];
        using (MemoryStream ms = new MemoryStream())
        {
            int read;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            return ms.ToArray();
        }
    }

    public static T DeserializeObject<T>(string name)
    {
        var fileContent = AsString(name);

        return JsonConvert.DeserializeObject<T>(fileContent);

        // return Serializer.ToModel<T>(fileContent);
    }

    private static class EmbeddedDataSource
    {
        private static Dictionary<string, Assembly> _assemblies = null;

        public static Stream GetStream(string name)
        {
            var assemblies = GetAssemblies(name);

            var key = assemblies.Keys.First(x => x.EndsWith(name));
            var assemblyContainingResource = assemblies[key];
            return assemblyContainingResource.GetManifestResourceStream(key);
        }

        private static Dictionary<string, Assembly> GetAssemblies(string name)
        {
            if (_assemblies != null)
            {
                return _assemblies;
            }

            var whlsTestAssemblies = AssembliesHelper.GetWhlsTestAssemblies();
            var assemblyNames = string.Join(", ", whlsTestAssemblies.Select(x => x.GetName().Name));

            var result = new Dictionary<string, Assembly>();
            foreach (var whlsTestAssembly in whlsTestAssemblies)
            {
                var fullNameAssemblyDictionary = whlsTestAssembly
                    .GetManifestResourceNames()
                    .Select(resource => new KeyValuePair<string, Assembly>(resource, whlsTestAssembly));

                foreach (var (fullName, assembly) in fullNameAssemblyDictionary)
                {
                    if (result.ContainsKey(fullName))
                    {
                        throw new ArgumentException($"Multiple resource with name '{name}' found in assemblies '{assemblyNames}");
                    }

                    result.Add(fullName, assembly);
                }
            }

            if (!result.Keys.Any(s => s.EndsWith(name)))
            {
                throw new ArgumentException($"Embedded resource with name '{name}' cannot be found in assemblies '{assemblyNames}");
            }

            _assemblies = result;
            return _assemblies;
        }
    }
}