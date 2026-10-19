using MessageBus;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceMessageBus
{
    public class InMemoryMessageBus : IMessageBus
    {
        private readonly ConcurrentDictionary<string, List<Delegate>> _handlers = new();

        public Task PublishAsync<T>(string topic, T message)
        {
            if (_handlers.TryGetValue(topic, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    if (handler is Action<T> typedHandler)
                    {
                        // Fire and forget on a background thread for parallel processing
                        Task.Run(() => typedHandler(message));
                    }
                }
            }
            return Task.CompletedTask;
        }

        public void Subscribe<T>(string topic, Action<T> handler)
        {
            _handlers.AddOrUpdate(
                topic,
                _ => new List<Delegate> { handler },
                (_, list) => { list.Add(handler); return list; }
            );
        }
    }
}