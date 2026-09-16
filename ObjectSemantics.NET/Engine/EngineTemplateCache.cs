using ObjectSemantics.NET.Engine.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace ObjectSemantics.NET.Engine
{
    internal static class EngineTemplateCache
    {
        private static CacheState _state = new CacheState(2048, 16 * 1024 * 1024);

        public static void Configure(int capacity, long maximumSourceCharacters)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (maximumSourceCharacters < 1) throw new ArgumentOutOfRangeException(nameof(maximumSourceCharacters));
            Volatile.Write(ref _state, new CacheState(capacity, maximumSourceCharacters));
        }

        public static EngineRunnerTemplate GetOrAdd(string templateContent, Func<string, EngineRunnerTemplate> factory)
        {
            return Volatile.Read(ref _state).GetOrAdd(templateContent ?? string.Empty, factory);
        }

        // FIFO eviction keeps cache hits lock-free. The character budget bounds retained source,
        // not total managed memory; parsed nodes and compiled accessors have additional overhead.
        internal class CacheState
        {
            private readonly int _capacity;
            private readonly long _maximumSourceCharacters;
            private readonly object _gate = new object();
            private readonly ConcurrentDictionary<string, Lazy<EngineRunnerTemplate>> _entries = new ConcurrentDictionary<string, Lazy<EngineRunnerTemplate>>(StringComparer.Ordinal);
            private readonly Queue<string> _order = new Queue<string>();
            private long _sourceCharacters;

            public CacheState(int capacity, long maximumSourceCharacters)
            {
                _capacity = capacity;
                _maximumSourceCharacters = maximumSourceCharacters;
            }

            public EngineRunnerTemplate GetOrAdd(string key, Func<string, EngineRunnerTemplate> factory)
            {
                if (key.Length > _maximumSourceCharacters)
                    return factory(key);
                if (!_entries.TryGetValue(key, out Lazy<EngineRunnerTemplate> entry))
                {
                    lock (_gate)
                    {
                        if (!_entries.TryGetValue(key, out entry))
                        {
                            while (_order.Count >= _capacity || _sourceCharacters + key.Length > _maximumSourceCharacters)
                            {
                                string oldest = _order.Dequeue();
                                _entries.TryRemove(oldest, out _);
                                _sourceCharacters -= oldest.Length;
                            }
                            entry = new Lazy<EngineRunnerTemplate>(() => factory(key), LazyThreadSafetyMode.ExecutionAndPublication);
                            _entries.TryAdd(key, entry);
                            _order.Enqueue(key);
                            _sourceCharacters += key.Length;
                        }
                    }
                }
                return entry.Value;
            }
        }
    }
}
