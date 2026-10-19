using CDCTarget.Modules;
using CDCTarget.Services.IServices;

namespace CDCTarget.Services
{
    public class EventBindingService : IEventBindingService
    {
        private readonly EventManager _eventManager;

        public EventBindingService(EventManager eventManager)
        {
            _eventManager = eventManager;
        }

        public void InitializeBindings()
        {
            _eventManager.StartListening();
        }
    }
}