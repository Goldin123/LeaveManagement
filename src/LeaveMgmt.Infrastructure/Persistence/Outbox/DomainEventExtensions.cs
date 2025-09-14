using System.Reflection;
using LeaveMgmt.Domain.Common;

namespace LeaveMgmt.Infrastructure.Persistence.Outbox;

internal static class DomainEventExtensions
{
    /// <summary>
    /// Returns and clears domain events from an aggregate root.
    /// Works with common patterns:
    ///  - List&lt;object&gt; _domainEvents field
    ///  - IReadOnlyCollection&lt;object&gt; DomainEvents + ClearDomainEvents() method
    /// </summary>
    public static IReadOnlyList<object> DequeueDomainEvents(this Entity entity)
    {
        // 1) Private field: List<object> _domainEvents
        var field = entity.GetType().GetField("_domainEvents",
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? typeof(Entity).GetField("_domainEvents", BindingFlags.Instance | BindingFlags.NonPublic);

        if (field?.GetValue(entity) is IList<object> list)
        {
            var copy = list.ToList();
            list.Clear();
            return copy;
        }

        // 2) Property + clearer method
        var prop = entity.GetType().GetProperty("DomainEvents",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            ?? typeof(Entity).GetProperty("DomainEvents",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (prop?.GetValue(entity) is IEnumerable<object> enumerable)
        {
            var events = enumerable.ToList();

            var clear = entity.GetType().GetMethod("ClearDomainEvents",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?? typeof(Entity).GetMethod("ClearDomainEvents",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            clear?.Invoke(entity, null);
            return events;
        }

        return Array.Empty<object>();
    }
}
