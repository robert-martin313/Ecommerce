﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using MrCMS.Entities.Multisite;
using MrCMS.Helpers;
using MrCMS.Indexing.Management;
using NHibernate;
using NHibernate.Criterion;
using Ninject;

namespace MrCMS.Services
{
    public interface IIndexService
    {
        void InitializeAllIndices();
        List<MrCMSIndex> GetIndexes();
        void Reindex(string typeName);
        void Optimise(string typeName);
        IIndexManagerBase GetIndexManagerBase(Type indexType);
    }

    public class IndexService : IIndexService
    {
        private readonly IKernel _kernel;
        private readonly ISession _session;
        private readonly Site _site;

        public IndexService(IKernel kernel, ISession session, Site site)
        {
            _kernel = kernel;
            _session = session;
            _site = site;
        }

        public void InitializeAllIndices()
        {
            List<MrCMSIndex> mrCMSIndices = GetIndexes();
            mrCMSIndices.ForEach(index => Reindex(index.TypeName));
            mrCMSIndices.ForEach(index => Optimise(index.TypeName));
        }

        public List<MrCMSIndex> GetIndexes()
        {
            var mrCMSIndices = new List<MrCMSIndex>();
            List<Type> indexDefinitionTypes = TypeHelper.GetAllConcreteTypesAssignableFrom(typeof (IndexDefinition<>));
            foreach (Type definitionType in indexDefinitionTypes)
            {
                IIndexManagerBase indexManagerBase = GetIndexManagerBase(definitionType);

                if (indexManagerBase != null)
                {
                    mrCMSIndices.Add(new MrCMSIndex
                                     {
                                         Name = indexManagerBase.IndexName,
                                         DoesIndexExist = indexManagerBase.IndexExists,
                                         LastModified = indexManagerBase.LastModified,
                                         NumberOfDocs = indexManagerBase.NumberOfDocs,
                                         TypeName = indexManagerBase.GetIndexDefinitionType().FullName
                                     });
                }
            }
            return mrCMSIndices;
        }

        public static Func<Type, IIndexManagerBase> GetIndexManagerOverride = null;

        public IIndexManagerBase GetIndexManagerBase(Type indexType)
        {
            IIndexManagerBase indexManagerBase =
                (GetIndexManagerOverride ?? DefaultGetIndexManager())(indexType);
            return indexManagerBase;
        }

        public void Reindex(string typeName)
        {
            Type definitionType = TypeHelper.GetTypeByName(typeName);
            IIndexManagerBase indexManagerBase = GetIndexManagerBase(definitionType);

            IList list =
                _session.CreateCriteria(indexManagerBase.GetEntityType())
                    .Add(Restrictions.Eq("Site.Id", _site.Id))
                    .List();

            object listInstance =
                Activator.CreateInstance(typeof (List<>).MakeGenericType(indexManagerBase.GetEntityType()));
            MethodInfo methodExt = listInstance.GetType().GetMethodExt("Add", indexManagerBase.GetEntityType());
            foreach (object entity in list)
            {
                methodExt.Invoke(listInstance, new[] {entity});
            }
            Type concreteManagerType = typeof (IIndexManager<,>).MakeGenericType(indexManagerBase.GetEntityType(),
                indexManagerBase.GetIndexDefinitionType());
            MethodInfo methodInfo = concreteManagerType.GetMethodExt("ReIndex",
                typeof (IEnumerable<>).MakeGenericType(
                    indexManagerBase.GetEntityType()));

            methodInfo.Invoke(indexManagerBase, new[] {listInstance});
        }

        public void Optimise(string typeName)
        {
            Type definitionType = TypeHelper.GetTypeByName(typeName);
            IIndexManagerBase indexManagerBase = GetIndexManagerBase(definitionType);

            indexManagerBase.Optimise();
        }

        private Func<Type, IIndexManagerBase> DefaultGetIndexManager()
        {
            return indexType => _kernel.Get(
                typeof (IIndexManager<,>).MakeGenericType(indexType.BaseType.GetGenericArguments()[0], indexType)) as
                IIndexManagerBase;
        }
    }

    public class MrCMSIndex
    {
        public string Name { get; set; }
        public bool DoesIndexExist { get; set; }
        public DateTime? LastModified { get; set; }
        public int? NumberOfDocs { get; set; }
        public string TypeName { get; set; }
    }
}