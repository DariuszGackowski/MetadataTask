using System.Collections.Concurrent;

namespace FivetranClient.Infrastructure;

public class TtlDictionary<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, AsyncLazy<(TValue Value, DateTime ExpirationTime)>> _dictionary = new();

    public async Task<TValue> GetOrAddAsync(TKey key, Func<Task<TValue>> valueFactory, TimeSpan ttl)
    {
        var lazyEntry = _dictionary.GetOrAdd(key, k => new AsyncLazy<(TValue, DateTime)>(async () =>
        {
            var value = await valueFactory();
            return (value, DateTime.UtcNow.Add(ttl));
        }));

        var entry = await lazyEntry.Value;

        if (DateTime.UtcNow < entry.ExpirationTime)
        {
            return entry.Value;
        }

        _dictionary.TryRemove(key, out _);
        return await GetOrAddAsync(key, valueFactory, ttl);
    }

    private class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<Task<T>> taskFactory) : base(taskFactory) { }
    }
}