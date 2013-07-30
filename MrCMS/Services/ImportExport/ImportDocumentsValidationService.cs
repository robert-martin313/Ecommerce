﻿using System;
using System.Collections.Generic;
using System.Linq;
using MrCMS.Helpers;
using MrCMS.Services.ImportExport.DTOs;
using MrCMS.Services.ImportExport.Rules;
using MrCMS.Website;
using OfficeOpenXml;

namespace MrCMS.Services.ImportExport
{
    public class ImportDocumentsValidationService : IImportDocumentsValidationService
    {
        private readonly IDocumentService _documentService;

        public ImportDocumentsValidationService(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        /// <summary>
        /// Validate Business Logic
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public Dictionary<string,List<string>> ValidateBusinessLogic(IEnumerable<DocumentImportDataTransferObject> items)
        {
            var errors = new Dictionary<string, List<string>>();
            var itemRules = MrCMSApplication.GetAll<IDocumentImportValidationRule>();

            var documentImportDataTransferObjects = items as IList<DocumentImportDataTransferObject> ?? items.ToList();
            foreach (var item in documentImportDataTransferObjects)
            {
                var validationErrors = itemRules.SelectMany(rule => rule.GetErrors(item, documentImportDataTransferObjects)).ToList();
                if (validationErrors.Any())
                    errors.Add(item.UrlSegment, validationErrors);
            }

            return errors;
        }

        /// <summary>
        /// Parse and Import to DTOs
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <param name="parseErrors"></param>
        /// <returns></returns>
        public List<DocumentImportDataTransferObject> ValidateAndImportDocuments(ExcelPackage spreadsheet, ref Dictionary<string, List<string>> parseErrors)
        {
            var items = new List<DocumentImportDataTransferObject>();

            if (!parseErrors.Any())
            {
                if (spreadsheet != null)
                {
                    if (spreadsheet.Workbook != null)
                    {
                        var worksheet = spreadsheet.Workbook.Worksheets.SingleOrDefault(x => x.Name == "Items");
                        if (worksheet != null)
                        {
                            var totalRows = worksheet.Dimension.End.Row;
                            for (var rowId = 2; rowId <= totalRows; rowId++)
                            {
                                var item = new DocumentImportDataTransferObject();

                                //Prepare handle name for storing and grouping errors
                                string urlSegment = worksheet.GetValue<string>(rowId, 1), name = worksheet.GetValue<string>(rowId, 2);
                                var handle = urlSegment.HasValue() ? urlSegment : name;

                                if (items.Any(x => x.Name == name || x.UrlSegment == urlSegment)) continue;
                                if (string.IsNullOrWhiteSpace(handle)) continue;

                                if (!parseErrors.Any(x => x.Key == handle))
                                    parseErrors.Add(handle, new List<string>());
                                item.UrlSegment = worksheet.GetValue<string>(rowId, 1).HasValue()
                                                      ? worksheet.GetValue<string>(rowId, 1)
                                                      : _documentService.GetDocumentUrl(name, null);
                                item.ParentUrl = worksheet.GetValue<string>(rowId, 2);
                                if (worksheet.GetValue<string>(rowId, 3).HasValue())
                                    item.DocumentType = worksheet.GetValue<string>(rowId, 3);
                                else
                                    parseErrors[handle].Add("Document Type is required.");
                                if (worksheet.GetValue<string>(rowId, 4).HasValue())
                                    item.Name = worksheet.GetValue<string>(rowId, 4);
                                else
                                    parseErrors[handle].Add("Document Name is required.");
                                item.BodyContent = worksheet.GetValue<string>(rowId, 5);
                                item.MetaTitle = worksheet.GetValue<string>(rowId, 6);
                                item.MetaDescription = worksheet.GetValue<string>(rowId, 7);
                                item.MetaKeywords = worksheet.GetValue<string>(rowId, 8);
                                //Tags
                                try
                                {
                                    var value = worksheet.GetValue<string>(rowId, 9);
                                    if (!String.IsNullOrWhiteSpace(value))
                                    {
                                        var tags = value.Split(',');
                                        foreach (var tag in tags.Where(tag => !String.IsNullOrWhiteSpace(tag)))
                                        {
                                            item.Tags.Add(tag);
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    parseErrors[handle].Add(
                                        "Url History field value contains illegal characters / not in correct format.");
                                }
                                if (worksheet.GetValue<string>(rowId, 10).HasValue())
                                {
                                    if (!worksheet.GetValue<string>(rowId, 10).IsValidInput<bool>())
                                        parseErrors[handle].Add("Reveal in Navigation is not a valid boolean value.");
                                    else
                                        item.RevealInNavigation = worksheet.GetValue<bool>(rowId, 10);
                                }
                                else
                                    item.RevealInNavigation = false;

                                if (worksheet.GetValue<string>(rowId, 11).HasValue())
                                {
                                    if (!worksheet.GetValue<string>(rowId, 11).IsValidInput<int>())
                                        parseErrors[handle].Add("Display Order is not a valid number.");
                                    else
                                        item.DisplayOrder = worksheet.GetValue<int>(rowId, 11);
                                }
                                else
                                    item.DisplayOrder = 0;

                                if (worksheet.GetValue<string>(rowId, 12).HasValue())
                                {
                                    if (!worksheet.GetValue<string>(rowId, 12).IsValidInput<bool>())
                                        parseErrors[handle].Add("Require SSL is not a valid boolean value.");
                                    else
                                        item.RequireSSL = worksheet.GetValue<bool>(rowId, 12);
                                }
                                else
                                    item.RequireSSL = false;


                                if (worksheet.GetValue<string>(rowId, 13).HasValue())
                                {
                                    if (!worksheet.GetValue<string>(rowId, 13).IsValidInputDateTime())
                                        parseErrors[handle].Add("Publish Date is not a valid date.");
                                    else
                                        item.PublishDate = worksheet.GetValue<DateTime>(rowId, 13);
                                }

                                //Url History
                                try
                                {
                                    var value = worksheet.GetValue<string>(rowId, 14);
                                    if (!String.IsNullOrWhiteSpace(value))
                                    {
                                        var urls = value.Split(',');
                                        foreach (var url in urls.Where(url => !String.IsNullOrWhiteSpace(url)))
                                        {
                                            item.UrlHistory.Add(url);
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                    parseErrors[handle].Add(
                                        "Url History field value contains illegal characters / not in correct format.");
                                }

                                items.Add(item);
                            }

                            //Remove duplicate errors
                            parseErrors = parseErrors.GroupBy(x=>x.Value).Select(x=>x.First()).ToDictionary(pair => pair.Key, pair => pair.Value);
                        }
                    }
                }

                //Remove handles with no errors
                parseErrors = parseErrors.Where(x => x.Value.Any()).ToDictionary(pair => pair.Key, pair => pair.Value);
            }

            return items;
        }

        /// <summary>
        /// Validate Import File
        /// </summary>
        /// <param name="spreadsheet"></param>
        /// <returns></returns>
        public Dictionary<string, List<string>> ValidateImportFile(ExcelPackage spreadsheet)
        {
            var parseErrors = new Dictionary<string, List<string>> { { "file", new List<string>() } };

            if (spreadsheet == null)
                parseErrors["file"].Add("No import file");
            else
            {
                if (spreadsheet.Workbook == null)
                    parseErrors["file"].Add("Error reading Workbook from import file.");
                else
                {
                    if (spreadsheet.Workbook.Worksheets.Count == 0)
                        parseErrors["file"].Add("No worksheets in import file.");
                    else
                    {
                        if (spreadsheet.Workbook.Worksheets.Count < 2 ||
                            !spreadsheet.Workbook.Worksheets.Any(x => x.Name == "Info") ||
                             !spreadsheet.Workbook.Worksheets.Any(x => x.Name == "Items"))
                            parseErrors["file"].Add(
                                "One or both of the required worksheets (Info and Items) are not present in import file.");
                    }
                }
            }

            return parseErrors.Where(x => x.Value.Any()).ToDictionary(pair => pair.Key, pair => pair.Value);
        }
    }
}