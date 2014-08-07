using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MrCMS.DbConfiguration.Helpers;
using MrCMS.Entities;
using MrCMS.Events;
using MrCMS.Helpers;
using MrCMS.Services;
using NHibernate;
using NHibernate.Transaction;

namespace MrCMS.DbConfiguration
{
    public class MrCMSTransaction : ITransaction
    {
        private readonly MrCMSSession _session;
        private readonly ITransaction _transaction;

        public MrCMSTransaction(ITransaction transaction, MrCMSSession session)
        {
            _transaction = transaction;
            _session = session;
        }

        public void Dispose()
        {
            _transaction.Dispose();
        }

        public void Begin()
        {
            _transaction.Begin();
        }

        public void Begin(IsolationLevel isolationLevel)
        {
            _transaction.Begin(isolationLevel);
        }

        public void Commit()
        {
            _transaction.Commit();
            HandlePostTransaction(_session);
        }

        public void Rollback()
        {
            _transaction.Rollback();
        }

        public void Enlist(IDbCommand command)
        {
            _transaction.Enlist(command);
        }

        public void RegisterSynchronization(ISynchronization synchronization)
        {
            _transaction.RegisterSynchronization(synchronization);
        }

        public bool IsActive
        {
            get { return _transaction.IsActive; }
        }

        public bool WasRolledBack
        {
            get { return _transaction.WasRolledBack; }
        }

        public bool WasCommitted
        {
            get { return _transaction.WasCommitted; }
        }

        private static void HandlePostTransaction(MrCMSSession session)
        {
            HashSet<EventInfo> eventInfos = session.Added.ToHashSet();
            eventInfos.ForEach(obj =>
            {
                if (obj.PostTransactionHandled)
                    return;
                obj.PostTransactionHandled = true;
                obj.Publish(session, typeof (IOnAdded<>), (info, ses, t) => info.GetTypedInfo(t).ToAddedArgs(ses, t));
            });
            HashSet<UpdatedEventInfo> updatedEventInfos = session.Updated.ToHashSet();
            updatedEventInfos.ForEach(obj =>
            {
                if (obj.PostTransactionHandled)
                    return;
                obj.PostTransactionHandled = true;
                obj.Publish(session, typeof (IOnUpdated<>), (info, ses, t) => info.GetTypedInfo(t).ToUpdatedArgs(ses, t));
            });
            HashSet<EventInfo> hashSet = session.Deleted.ToHashSet();
            hashSet.ForEach(obj =>
            {
                if (obj.PostTransactionHandled)
                    return;
                obj.PostTransactionHandled = true;
                obj.Publish(session, typeof (IOnDeleted<>), (info, ses, t) => info.GetTypedInfo(t).ToDeletedArgs(ses, t));
            });
        }
    }

    public static class CoreEventPublisher
    {
        public static void Publish<T>(this T onUpdatedArgs, ISession session, Type eventType,
            Func<T, ISession, Type, object> getArgs)
        {
            Type type = onUpdatedArgs.GetType().GenericTypeArguments[0];

            List<Type> types = GetEntityTypes(type).Reverse().ToList();

            types.ForEach(
                t => EventContext.Instance.Publish(eventType.MakeGenericType(t), getArgs(onUpdatedArgs, session, t)));
        }


        private static IEnumerable<Type> GetEntityTypes(Type type)
        {
            Type thisType = type;
            while (thisType != null && thisType != typeof (SystemEntity))
            {
                yield return thisType;
                thisType = thisType.BaseType;
            }
            yield return typeof (SystemEntity);
        }
    }
}