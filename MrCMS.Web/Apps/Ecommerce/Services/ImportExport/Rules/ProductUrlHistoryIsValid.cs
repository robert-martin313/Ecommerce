﻿using System.Collections.Generic;
using MrCMS.Web.Apps.Ecommerce.Pages;
using MrCMS.Web.Apps.Ecommerce.Services.ImportExport.DTOs;
using MrCMS.Services;

namespace MrCMS.Web.Apps.Ecommerce.Services.ImportExport.Rules
{
    public class ProductUrlHistoryIsValid : IProductImportValidationRule
    {
        private readonly IDocumentService _documentService;

        public ProductUrlHistoryIsValid(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        public IEnumerable<string> GetErrors(ProductImportDataTransferObject product)
        {
            var errors = new List<string>();
            foreach (var item in product.UrlHistory)
            {
                if (!_documentService.UrlIsValidForWebpageUrlHistory(item))
                        errors.Add(string.Format("Url:{0} is not valid url (possibly already exists).",item));
            }
            return errors;
        }
    }
}