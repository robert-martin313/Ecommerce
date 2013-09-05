﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MrCMS.Entities;
using MrCMS.Web.Apps.Ecommerce.Entities.Geographic;
using System.ComponentModel;
using NHibernate;

namespace MrCMS.Web.Apps.Ecommerce.Entities.Users
{
    public class Address : SiteEntity
    {
        [DisplayName("First Name")]
        [Required]
        public virtual string FirstName { get; set; }
        [DisplayName("Last Name")]
        [Required]
        public virtual string LastName { get; set; }

        public virtual string Name
        {
            get { return string.Format("{0} {1} {2}", Title, FirstName, LastName); }
        }
        public virtual string Title { get; set; }
        public virtual string Company { get; set; }
        [DisplayName("Address 1")]
        [Required]
        public virtual string Address1 { get; set; }
        [DisplayName("Address 2")]
        public virtual string Address2 { get; set; }
        [Required]
        public virtual string City { get; set; }
        [DisplayName("State/Province")]
        public virtual string StateProvince { get; set; }
        public virtual Country Country { get; set; }
        [DisplayName("Postal Code")]
        [Required]
        public virtual string PostalCode { get; set; }
        [DisplayName("Phone Number")]
        [Required]
        public virtual string PhoneNumber { get; set; }

        public virtual Guid UserGuid { get; set; }

        public virtual Address Clone(ISession session)
        {
            return new Address
                       {
                           Address1 = Address1,
                           Address2 = Address2,
                           City = City,
                           Company = Company,
                           Country = Country == null ? null : session.Get<Country>(Country.Id),
                           FirstName =FirstName,
                           LastName = LastName,
                           PhoneNumber = PhoneNumber,
                           PostalCode = PostalCode,
                           StateProvince = StateProvince,
                           Title = Title,
                           UserGuid = UserGuid
                       };
        }

        public virtual string Description
        {
            get { return string.Join(", ", AddressParts); }
        }

        private IEnumerable<string> AddressParts
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Name))
                    yield return Name;
                if (!string.IsNullOrWhiteSpace(Company))
                    yield return Company;
                if (!string.IsNullOrWhiteSpace(Address1))
                    yield return Address1;
                if (!string.IsNullOrWhiteSpace(Address2))
                    yield return Address2;
                if (!string.IsNullOrWhiteSpace(City))
                    yield return City;
                if (!string.IsNullOrWhiteSpace(StateProvince))
                    yield return StateProvince;
                if (Country != null)
                    yield return Country.Name;
                if (!string.IsNullOrWhiteSpace(PostalCode))
                    yield return PostalCode;
            }
        }
    }
}