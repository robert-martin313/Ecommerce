using MrCMS.Entities;

namespace MrCMS.Events
{
    /// <summary>
    /// Interface to define events that are called before an item has been deleted
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOnDeleting<T> : IEvent<OnDeletingArgs<T>>, ICoreEvent where T : SystemEntity
    {
    }
}