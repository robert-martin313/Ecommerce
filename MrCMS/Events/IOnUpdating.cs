using MrCMS.Entities;

namespace MrCMS.Events
{
    /// <summary>
    /// Interface to define events that are called before an item has been updated in the db
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IOnUpdating<T> : IEvent<OnUpdatingArgs<T>>, ICoreEvent where T : SystemEntity
    {
    }
}