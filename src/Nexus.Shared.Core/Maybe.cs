namespace Nexus.Shared.Core;

/// Used to encapsulate the possibility of a null-reference and communicate intent
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct Maybe<T>
{
    private readonly IEnumerable<T> _values;

    /// <summary>
    /// If value is null then empty otherwise with the value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Maybe<T> If(T value)
    {
        return value != null
            ? With(value)
            : Empty;
    }

    /// <summary>
    /// Returns Maybe with a value
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static Maybe<T> With(T value)
    {
        if (value == null)
            throw new ArgumentException($"Parameter {nameof(value)} should not be null");

        return new Maybe<T>(new[] { value });
    }

    /// <summary>
    /// Returns Maybe without a value
    /// </summary>
    public static Maybe<T> Empty => new Maybe<T>(Array.Empty<T>());

    private Maybe(IEnumerable<T> values)
    {
        _values = values;
    }

    public readonly bool HasValue
        => _values != null && _values.Any();

    public readonly bool IsEmpty
        => !HasValue;

    public T Value
    {
        get
        {
            if (!HasValue)
            {
                throw new InvalidOperationException("Maybe does not have a value");
            }

            return _values.Single();
        }
    }

    public readonly T ValueOrDefault
        => _values.SingleOrDefault();

    /// <summary>
    /// If maybe has value, then value is returned. Otherwise the provided alternative is returned.
    /// </summary>
    /// <param name="alternative"></param>
    /// <returns></returns>
    public T ValueOr(T alternative) => HasValue ? Value : alternative;
}

/// <summary>
/// Creates a <see cref="Maybe{T}"/> and infers the T type
/// </summary>
public struct Maybe
{
    /// <summary>
    /// If value is null then empty otherwise with the value
    /// </summary>
    public static Maybe<T> If<T>(T value) => Maybe<T>.If(value);
    /// <summary>
    /// Returns Maybe with a value
    /// </summary>
    public static Maybe<T> With<T>(T value) => Maybe<T>.With(value);
}
