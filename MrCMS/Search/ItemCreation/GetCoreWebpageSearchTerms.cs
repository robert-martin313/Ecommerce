using System.Collections.Generic;
using System.Linq;
using MrCMS.Entities.Documents;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Transform;

namespace MrCMS.Search.ItemCreation
{
    public class GetCoreWebpageSearchTerms : IGetWebpageSearchTerms
    {
        private readonly IStatelessSession _statelessSession;

        public GetCoreWebpageSearchTerms(IStatelessSession statelessSession)
        {
            _statelessSession = statelessSession;
        }

        public IEnumerable<string> GetPrimary(Webpage webpage)
        {
            return GetPrimaryTerms(webpage);
        }

        public Dictionary<Webpage, HashSet<string>> GetPrimary(HashSet<Webpage> webpages)
        {
            return webpages.ToDictionary(webpage => webpage,
                webpage => GetPrimaryTerms(webpage).ToHashSet());
        }

        public IEnumerable<string> GetSecondary(Webpage webpage)
        {
            HashSet<string> documentTags =
                _statelessSession.Query<Tag>()
                    .Where(x => x.Documents.Contains(webpage))
                    .Select(tag => tag.Name)
                    .ToHashSet();
            return GetSecondaryTerms(webpage, documentTags);
        }

        public Dictionary<Webpage, HashSet<string>> GetSecondary(HashSet<Webpage> webpages)
        {
            if (!webpages.Any())
                return new Dictionary<Webpage, HashSet<string>>();
            Tag tagAlias = null;
            TagInfo tagInfo = null;
            string typeName = webpages.First().DocumentType;
            Dictionary<int, IEnumerable<string>> tagInfoDictionary = _statelessSession.QueryOver<Webpage>()
                .Where(webpage => webpage.DocumentType == typeName)
                .JoinAlias(webpage => webpage.Tags, () => tagAlias)
                .SelectList(builder =>
                    builder.Select(webpage => webpage.Id).WithAlias(() => tagInfo.WebpageId)
                        .Select(() => tagAlias.Name).WithAlias(() => tagInfo.TagName))
                .TransformUsing(Transformers.AliasToBean<TagInfo>()).List<TagInfo>()
                .GroupBy(info => info.WebpageId)
                .ToDictionary(infos => infos.Key, infos => infos.Select(x => x.TagName));
            return webpages.ToDictionary(webpage => webpage,
                webpage =>
                    GetSecondaryTerms(webpage,
                        tagInfoDictionary.ContainsKey(webpage.Id)
                            ? tagInfoDictionary[webpage.Id]
                            : Enumerable.Empty<string>()).ToHashSet());
        }

        private IEnumerable<string> GetPrimaryTerms(Webpage webpage)
        {
            yield return webpage.Name;
        }

        private IEnumerable<string> GetSecondaryTerms(Webpage webpage, IEnumerable<string> tags)
        {
            yield return webpage.BodyContent;
            yield return webpage.UrlSegment;
            foreach (string tag in tags)
            {
                yield return tag;
            }
        }

        public class TagInfo
        {
            public int WebpageId { get; set; }
            public string TagName { get; set; }
        }
    }
}