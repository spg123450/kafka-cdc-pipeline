using System;
using System.Threading.Tasks;

namespace MessageBus
{
    public interface IMessageBus
    {
        Task PublishAsync<T>(string topic, T message);
        void Subscribe<T>(string topic, Action<T> handler);
    }
}