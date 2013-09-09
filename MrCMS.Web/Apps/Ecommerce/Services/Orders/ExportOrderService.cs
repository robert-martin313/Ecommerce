﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using MrCMS.Web.Apps.Ecommerce.Entities.Orders;
using MrCMS.Web.Apps.Ecommerce.Helpers;
using MrCMS.Website;
using PdfSharp.Pdf;

namespace MrCMS.Web.Apps.Ecommerce.Services.Orders
{
    public class ExportOrderService : IExportOrdersService
    {
        public byte[] ExportOrderToPdf(Order order)
        {
            var pdf = SetDocumentInfo(order);

            SetDocumentStyles(ref pdf);

            SetDocument(ref pdf, order);

            return GetDocumentToByteArray(ref pdf);
        }

        private static Document SetDocumentInfo(Order order)
        {
            return new Document
            {
                Info =
                {
                    Title = CurrentRequestData.CurrentSite.Name + " Order: " + order.Guid,
                    Subject = CurrentRequestData.CurrentSite.Name + " Order: " + order.Guid,
                    Keywords = "MrCMS, Order",
                    Author = CurrentRequestData.CurrentUser.Name
                }
            };
        }

        private void SetDocumentStyles(ref Document document)
        {
            var style = document.Styles["Normal"];
            style.Font.Name = "Tahoma";
            style.Font.Size = 10;

            style = document.Styles.AddStyle("Table", "Normal");
            style.Font.Name = "Tahoma";
            style.Font.Size = 9;
        }

        private void SetDocument(ref Document document, Order order)
        {
            var tableColor = new Color(0, 0, 0, 0);
            var section = document.AddSection();

            //HEADER
            SetHeader(ref section);

            //FOOTER
            SetFooter(ref section);

            //INFO
            SetInfo(order, ref section);

            //TABLE STYLE
            var table = SetTableStyle(ref section, tableColor);

            //HEADERS
            SetTableHeader(ref table, tableColor);

            //ITEMS
            SetTableData(order, ref table);

            //SUMMARY
            SetTableSummary(order, ref table);
        }

        private void SetHeader(ref Section section)
        {
            var frame1 = section.Headers.Primary.AddTextFrame();
            frame1.RelativeVertical = RelativeVertical.Page;
            frame1.Left = ShapePosition.Left;
            frame1.MarginTop = new Unit(1, UnitType.Centimeter);
            frame1.Width = new Unit(10, UnitType.Centimeter);

            var frame2 = section.Headers.Primary.AddTextFrame();
            frame2.RelativeVertical = RelativeVertical.Page;
            frame2.Left = ShapePosition.Right;
            frame2.MarginTop = new Unit(1, UnitType.Centimeter);
            frame2.Width = new Unit(2, UnitType.Centimeter);

            var p = frame1.AddParagraph();
            p.AddFormattedText(CurrentRequestData.CurrentSite.Name, TextFormat.Bold);
            p = frame2.AddParagraph();
            p.AddDateField("dd/MM/yyyy");
        }

        private void SetFooter(ref Section section)
        {
            var p = section.Footers.Primary.AddParagraph();
            p.Format.Alignment = ParagraphAlignment.Left;
            p.Format.Font.Size = 8;
            p.AddText(CurrentRequestData.CurrentSite.BaseUrl);
        }

        private void SetInfo(Order order, ref Section section)
        {
            var frame1 = section.AddTextFrame();
            frame1.RelativeVertical = RelativeVertical.Page;
            frame1.Left = ShapePosition.Left;
            frame1.Top = new Unit(1.85, UnitType.Centimeter);
            frame1.Width = new Unit(10, UnitType.Centimeter);
            var p = frame1.AddParagraph();
            p.Format.Font.Size = 16;
            p.AddFormattedText("Order #" + order.Id, TextFormat.Bold);

            //LEFT
            frame1 = section.AddTextFrame();
            frame1.RelativeVertical = RelativeVertical.Page;
            frame1.Left = ShapePosition.Left;
            frame1.Top = new Unit(3, UnitType.Centimeter);
            frame1.Width = new Unit(10, UnitType.Centimeter);

            //RIGHT
            var frame2 = section.AddTextFrame();
            frame2.RelativeVertical = RelativeVertical.Page;
            frame2.Left = ShapePosition.Right;
            frame2.Top = new Unit(3, UnitType.Centimeter);
            frame2.Width = new Unit(8, UnitType.Centimeter);

            //BILLING AND SHIPPING
            p = frame1.AddParagraph();
            p.AddFormattedText("Bill to:", TextFormat.Bold);
            p = frame2.AddParagraph();
            p.AddFormattedText("Ship to:", TextFormat.Bold);
            p = frame1.AddParagraph();
            p.AddText(order.BillingAddress.Name);
            p = frame2.AddParagraph();
            p.AddText(order.ShippingAddress.Name);
            p = frame1.AddParagraph();
            p.AddText(order.BillingAddress.PhoneNumber);
            p = frame2.AddParagraph();
            p.AddText(order.ShippingAddress.PhoneNumber);
            p = frame1.AddParagraph();
            p.AddText(order.BillingAddress.Address1);
            p = frame2.AddParagraph();
            p.AddText(order.ShippingAddress.Address1);
            if (!String.IsNullOrWhiteSpace(order.BillingAddress.Address2))
            {
                p = frame1.AddParagraph();
                p.AddText(order.BillingAddress.Address2);
            }
            if (!String.IsNullOrWhiteSpace(order.ShippingAddress.Address2))
            {
                p = frame2.AddParagraph();
                p.AddText(order.ShippingAddress.Address2);
            }
            p = frame1.AddParagraph();
            p.AddText(order.BillingAddress.City);
            p = frame2.AddParagraph();
            p.AddText(order.ShippingAddress.City);
            if (!String.IsNullOrWhiteSpace(order.BillingAddress.StateProvince))
            {
                p = frame1.AddParagraph();
                p.AddText(order.BillingAddress.StateProvince);
            }
            if (!String.IsNullOrWhiteSpace(order.ShippingAddress.StateProvince))
            {
                p = frame2.AddParagraph();
                p.AddText(order.ShippingAddress.StateProvince);
            }
            p = frame1.AddParagraph();
            p.AddText(order.BillingAddress.Country.Name);
            p = frame2.AddParagraph();
            p.AddText(order.ShippingAddress.Country.Name);
            p = frame1.AddParagraph();
            p.AddText(order.BillingAddress.PostalCode);
            p = frame2.AddParagraph();
            p.AddText(order.ShippingAddress.PostalCode);

            frame1.AddParagraph("").AddLineBreak();
            frame2.AddParagraph("").AddLineBreak();

            //PAYMENT AND SHIPPING METHODS
            p = frame1.AddParagraph();
            p.AddText("Payment method: " + order.PaymentMethod);
            p = frame2.AddParagraph();
            p.AddText("Shipping method: " + order.ShippingMethod.Name);
        }

        private Table SetTableStyle(ref Section section, Color tableColor)
        {
            var frame = section.AddTextFrame();
            frame.MarginTop = new Unit(6, UnitType.Centimeter);
            frame.Width = new Unit(16, UnitType.Centimeter);

            //TABLE LABEL
            var p = frame.AddParagraph();
            p.AddFormattedText("Purchased goods:", TextFormat.Bold);

            frame.AddParagraph("").AddLineBreak();

            //TABLE
            var table = frame.AddTable();
            table.Style = "Table";
            table.Borders.Color = tableColor;
            table.Borders.Width = 0.25;
            table.Borders.Left.Width = 0.5;
            table.Borders.Right.Width = 0.5;
            table.Rows.LeftIndent = 0;
            return table;
        }

        private void SetTableHeader(ref Table table, Color tableColor)
        {
            var columns = new Dictionary<string, Dictionary<string, ParagraphAlignment>>()
                {
                    {
                        "#", new Dictionary<string, ParagraphAlignment>()
                            {
                                {"1cm", ParagraphAlignment.Center}
                            }
                    },
                    {
                        "Title", new Dictionary<string, ParagraphAlignment>()
                            {
                                {"6cm", ParagraphAlignment.Left}
                            }
                    },
                    {
                        "Unit Price", new Dictionary<string, ParagraphAlignment>()
                            {
                                {"3cm", ParagraphAlignment.Right}
                            }
                    },
                    {
                        "Qty", new Dictionary<string, ParagraphAlignment>()
                            {
                                {"3cm", ParagraphAlignment.Center}
                            }
                    },
                    {
                        "Total", new Dictionary<string, ParagraphAlignment>()
                            {
                                {"3cm", ParagraphAlignment.Right}
                            }
                    },
                };

            foreach (var item in columns)
            {
                var column = table.AddColumn(item.Value.First().Key);
                column.Format.Alignment = item.Value.First().Value;
            }

            var row = table.AddRow();
            row.HeadingFormat = true;
            row.Format.Alignment = ParagraphAlignment.Center;
            row.Format.Font.Bold = true;
            row.Shading.Color = tableColor;
            row.TopPadding = 2;
            row.BottomPadding = 2;
            var rowId = 0;
            foreach (var item in columns)
            {
                row.Cells[rowId].AddParagraph(item.Key);
                row.Cells[rowId].Format.Alignment = ParagraphAlignment.Center;
                rowId++;
            }

            table.SetEdge(0, 0, 5, 1, Edge.Box, BorderStyle.Single, 0.75, Color.Empty);
        }

        private void SetTableData(Order order, ref Table table)
        {
            for (var i = 0; i < order.OrderLines.Count; i++)
            {
                var orderLine = order.OrderLines[i];
                var row = table.AddRow();
                row.TopPadding = 2;
                row.BottomPadding = 2;

                row.Cells[0].AddParagraph((i + 1).ToString());
                row.Cells[1].AddParagraph(orderLine.ProductVariant.DisplayName);
                row.Cells[2].AddParagraph(orderLine.UnitPrice.ToCurrencyFormat());
                row.Cells[3].AddParagraph(orderLine.Quantity.ToString());
                row.Cells[4].AddParagraph(orderLine.Price.ToCurrencyFormat());

                table.SetEdge(0, table.Rows.Count - 2, 5, 2, Edge.Box, BorderStyle.Single, 0.75);
            }
        }

        private void SetTableSummary(Order order, ref Table table)
        {
            var summaryData = new Dictionary<string, string>()
                {
                    {"Sub-total", order.Subtotal.ToCurrencyFormat()},
                    {"Shipping", order.ShippingTotal.ToCurrencyFormat()},
                    {"Tax", order.Tax.ToCurrencyFormat()},
                    {"Discount", order.DiscountAmount.ToCurrencyFormat()},
                    {"Total", order.Total.ToCurrencyFormat()},
                };

            foreach (var item in summaryData)
            {
                var row = table.AddRow();
                row.TopPadding = 2;
                row.BottomPadding = 2;
                row.Cells[0].Borders.Visible = false;
                row.Cells[0].AddParagraph(item.Key + ":");
                row.Cells[0].Format.Alignment = ParagraphAlignment.Right;
                row.Cells[0].MergeRight = 3;
                if (item.Key == "Total")
                    row.Cells[4].Format.Font.Bold = true;
                row.Cells[4].AddParagraph(item.Value);
            }

            table.SetEdge(4, table.Rows.Count - 3, 1, 3, Edge.Box, BorderStyle.Single, 0.75);
        }

        private byte[] GetDocumentToByteArray(ref Document pdf)
        {
            var renderer = new PdfDocumentRenderer(true, PdfFontEmbedding.Automatic) { Document = pdf };
            renderer.RenderDocument();
            var stream = new MemoryStream();
            renderer.PdfDocument.Save(stream);
            return stream.ToArray();
        }
    }
}