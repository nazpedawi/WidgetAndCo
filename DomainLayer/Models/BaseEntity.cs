using MediatR;

namespace DomainLayer.Models
{
    public abstract class BaseEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        private readonly List<INotification> _domainEvents = new();

        public IReadOnlyCollection<INotification> GetDomainEvents() => _domainEvents.AsReadOnly();
        public void AddDomainEvent(INotification domainEvent) => _domainEvents.Add(domainEvent);
        public void ClearDomainEvents() => _domainEvents.Clear();
    }
}