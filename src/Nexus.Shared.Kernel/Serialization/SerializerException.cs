namespace Nexus.Shared.Kernel.Serialization;

public class SerializerException : Exception
{
    public SerializerException(string input, Exception origin)
        : base("Problem to deserialize input: " + input, origin)
    {
    }
}