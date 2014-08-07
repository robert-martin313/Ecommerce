using MrCMS.Entities;

namespace MrCMS.Events
{
    /// <summary>
    /// Interface to define events that are called before a new item has been persisted to the db
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOnAdding<T> : IEvent<OnAddingArgs<T>>, ICoreEvent where T : SystemEntity
    {
    }
}