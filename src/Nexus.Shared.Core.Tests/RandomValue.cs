namespace Nexus.Shared.Core.Tests;

public static class RandomValue
{
    private static readonly Random Rng = new();

    // -----------------------------
    //  Simple Values
    // -----------------------------
    public static string String => Guid.NewGuid().ToString();
    public static string[] StringArray => new[] { String, String, String };

    public static Guid Guid => Guid.NewGuid();
    public static byte Byte => (byte)RangeInt(0, 255);

    public static int Int => Rng.Next();
    public static int PositiveInt => RangeInt(1, int.MaxValue);
    public static int PositiveIntWithMax(int max) => RangeInt(1, max);

    public static long Long => Rng.NextInt64();

    public static bool Bool => (Int % 2) == 0;

    // -----------------------------
    //  Decimal / Double
    // -----------------------------
    public static decimal Decimal => RangeDecimal(-1_000_000, 1_000_000);
    public static double Double => RangeDouble(-100, 100);

    public static double RangeDouble(int min, int max, int precision = 2)
        => Math.Round(Rng.NextDouble() * (max - min) + min, precision);

    public static decimal RangeDecimal(int min, int max, int precision = 2)
        => (decimal)RangeDouble(min, max, precision);

    // -----------------------------
    //  Dates
    // -----------------------------
    public static DateTime DateTime => RangeDate(DateTime.MinValue, DateTime.MaxValue);
    public static DateTime DateTimeThisYear =>
        RangeDate(new DateTime(DateTime.UtcNow.Year, 1, 1), DateTime.UtcNow);

    public static DateTime DateTimeBefore(DateTime value) =>
        RangeDate(DateTime.MinValue, value.AddMilliseconds(-1));

    public static DateTime DateTimeAfter(DateTime value) =>
        RangeDate(value.AddMilliseconds(1), DateTime.MaxValue);

    public static DateTime RangeDate(DateTime from, DateTime to)
        => new(Rng.NextInt64(from.Ticks, to.Ticks), DateTimeKind.Utc);

    public static DateTimeOffset DateTimeOffset => new(DateTime);

    // -----------------------------
    //  Collections
    // -----------------------------
    public static HashSet<string> HashSet => new();

    // -----------------------------
    //  Specialized values
    // -----------------------------
    private const string Alphabet = "abcdefghijklmnopqrstuvwyxzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static string Chars(int length)
    {
        var buffer = new char[length];

        for (var i = 0; i < length; i++)
            buffer[i] = Alphabet[Rng.Next(Alphabet.Length)];

        return new(buffer);
    }

    public static string LegalEntityId =>
        $"{Chars(2)}{RangeInt(0, 99):D2}".ToUpperInvariant();

    // -----------------------------
    //  Internal helpers
    // -----------------------------
    public static int RangeInt(int min, int max) => Rng.Next(min, max);
}