using Nexus.Shared.Kernel.Serialization;

namespace Nexus.Shared.Core.Tests.Httpresponses;

public static class HttpResponseMessageExtensions
{
    public static async Task<T> DeserializeBodyAsync<T>(this HttpResponseMessage message)
    {
        var responseBody = await message.Content.ReadAsStringAsync();
        return Serializer.ToModel<T>(responseBody);
    }
}