﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using MrCMS.Paging;
using MrCMS.Services;
using MrCMS.Web.Apps.Ecommerce.Entities.ProductReviews;
using MrCMS.Web.Apps.Ecommerce.Entities.Products;
using MrCMS.Website;
using NHibernate;
using NHibernate.Criterion;

namespace MrCMS.Web.Apps.Ecommerce.Services.ProductReviews
{
    public class ProductReviewUIService : IProductReviewUIService
    {
        private readonly ISession _session;
        private readonly IGetCurrentUser _getCurrentUser;

        public ProductReviewUIService(ISession session,IGetCurrentUser getCurrentUser)
        {
            _session = session;
            _getCurrentUser = getCurrentUser;
        }

        public ProductReview GetById(int id)
        {
            return _session.QueryOver<ProductReview>().Where(x => x.Id == id).SingleOrDefault();
        }

        public IList<ProductReview> GetAll()
        {
            return _session.QueryOver<ProductReview>().OrderBy(x => x.CreatedOn).Desc.Cacheable().List();
        }

        public void Add(ProductReview productReview)
        {
            var user = _getCurrentUser.Get();
            if (user!= null)
                productReview.User = user;
            _session.Transact(session => session.Save(productReview));
        }

        public void Update(ProductReview productReview)
        {
            _session.Transact(session => session.Update(productReview));
        }

        public void Delete(ProductReview productReview)
        {
            _session.Transact(session => session.Delete(productReview));
        }

        public List<SelectListItem> GetRatingOptions()
        {
            return Enumerable.Range(1, 5)
                .BuildSelectItemList(i => i.ToString(), emptyItemText: "Please select");
        }

        public IPagedList<ProductReview> GetReviewsByProductVariantId(ProductVariant productVariant, int pageNum, int pageSize = 10)
        {
            ProductReview productReviewAlias = null;
            return
                GetBaseProductVariantReviewsQuery(_session.QueryOver(() => productReviewAlias), productVariant)
                    .OrderBy(
                        Projections.SubQuery(
                            QueryOver.Of<HelpfulnessVote>()
                                .Where(vote => vote.ProductReview.Id == productReviewAlias.Id && vote.IsHelpful)
                                .Select(Projections.Count<HelpfulnessVote>(x => x.Id)))).Desc
                    .Paged(pageNum, pageSize);
        }

        //Ask gary for averaging
        public decimal GetAverageRatingsByProductVariant(ProductVariant productVariant)
        {
            if (!GetBaseProductVariantReviewsQuery(_session.QueryOver<ProductReview>(), productVariant).Cacheable().Any()) 
                return decimal.Zero;
            return GetBaseProductVariantReviewsQuery(_session.QueryOver<ProductReview>(),productVariant)
                .Select(x => x.Rating).Cacheable()
                .List<int>().Select(Convert.ToDecimal).Average();
        }

        private IQueryOver<ProductReview, ProductReview> GetBaseProductVariantReviewsQuery(IQueryOver<ProductReview,ProductReview> query, ProductVariant productVariant)
        {
            return query.Where(review => review.ProductVariant.Id == productVariant.Id && review.Approved == true);
        }

        public IPagedList<ProductReview> GetReviewsByUser(User user, int pageNum, int pageSize = 10)
        {
            int id = user.Id;
            return _session.QueryOver<ProductReview>()
                .Where(x => x.User.Id == id)
                .OrderBy(x => x.CreatedOn)
                .Desc.Paged(pageNum, pageSize);
        }

        public IPagedList<ProductReview> GetPaged(int pageNum, string search, int pageSize = 10)
        {
            return BaseQuery(search).Paged(pageNum, pageSize);
        }

        private IQueryOver<ProductReview, ProductReview> BaseQuery(string search)
        {
            ProductVariant productVariantAlias = null;
            ProductReview productReviewAlias = null;

            if (String.IsNullOrWhiteSpace(search))
                return
                _session.QueryOver<ProductReview>()
                        .OrderBy(entry => entry.CreatedOn).Desc;
            return _session.QueryOver(() => productReviewAlias)
                .JoinAlias(() => productReviewAlias.ProductVariant, () => productVariantAlias)
                .Where(() => productVariantAlias.Name.IsInsensitiveLike(search, MatchMode.Anywhere))
                .OrderBy(entry => entry.CreatedOn).Desc;
        }
    }
}