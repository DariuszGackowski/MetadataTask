using System.Collections.Concurrent;

namespace FivetranClient.Infrastructure;

public class TtlDictionary<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, (TValue Value, DateTimeOffset ExpirationTime)> _dictionary = new();

    public TValue GetOrAdd(TKey key, Func<TValue> valueFactory, TimeSpan ttl)
    {
        var currentDateTime = DateTimeOffset.UtcNow;
        var (Value, ExpirationTime) = _dictionary.GetOrAdd(key, keyToAdd => (valueFactory(), currentDateTime.Add(ttl)));

        if (currentDateTime < ExpirationTime)
        {
            return Value;
        }

        _dictionary.TryRemove(key, out _);
        var newValue = valueFactory();
        var newEntry = (newValue, currentDateTime.Add(ttl));
        _dictionary[key] = newEntry;

        return newValue;
    }

    public bool TryGetValue(TKey key, out TValue? value)
    {
        if (_dictionary.TryGetValue(key, out var entry) && DateTimeOffset.UtcNow < entry.ExpirationTime)
        {
            value = entry.Value;
            return true;
        }

        value = default;
        return false;
    }
}