using System.Collections.Generic;
using MrCMS.Models;
using MrCMS.Web.Apps.Ecommerce.ACL;
using MrCMS.Website;

namespace MrCMS.Web.Apps.Ecommerce.Models
{
    public class EcommerceToolsMenuModel : IAdminMenuItem
    {
        private SubMenu _children;
        public string Text { get { return "Tools & Reports"; } }
        public string IconClass { get { return "fa fa-wrench"; } }
        public string Url { get; private set; }
        public bool CanShow { get { return CurrentRequestData.CurrentUser.CanAccess<ToolsAndReportsACL>(ToolsAndReportsACL.Show); } }
        public SubMenu Children
        {
            get
            {
                return _children ??
                       (_children = new SubMenu
                       {
                           new ChildMenuItem("Import/Export Products",
                                       "/Admin/Apps/Ecommerce/ImportExport/Products", ACLOption.Create(new ImportExportACL(), ImportExportACL.View)),
                                   new ChildMenuItem("Google Base Integration",
                                       "/Admin/Apps/Ecommerce/GoogleBase/Dashboard", ACLOption.Create(new GoogleBaseACL(), GoogleBaseACL.View)),
                                   new ChildMenuItem("Low Stock Report",
                                       "/Admin/Apps/Ecommerce/Stock/LowStockReport", ACLOption.Create(new LowStockReportACL(), LowStockReportACL.View)),
                                   new ChildMenuItem("Bulk Stock Update",
                                       "/Admin/Apps/Ecommerce/Stock/BulkStockUpdate", ACLOption.Create(new BulkStockUpdateACL(), BulkStockUpdateACL.BulkStockUpdate)),
                                   new ChildMenuItem("Bulk Shipping Update",
                                       "/Admin/Apps/Ecommerce/BulkShipping/Update", ACLOption.Create(new BulkShippingUpdateACL(), BulkShippingUpdateACL.Update)),
                       });
            }
        }
        public int DisplayOrder { get { return 51; } }
    }

    public class EcommerceReportsMenuModel : IAdminMenuItem
    {
        private SubMenu _children;
        public string Text { get { return "Ecommerce Reports"; } }
        public string IconClass { get { return "fa fa-line-chart"; } }
        public string Url { get; private set; }
        public bool CanShow { get { return CurrentRequestData.CurrentUser.CanAccess<ToolsAndReportsACL>(ToolsAndReportsACL.Show); } }
        public SubMenu Children
        {
            get
            {
                return _children ??
                       (_children = new SubMenu
                       {
                          new ChildMenuItem("Sales by day",
                                       "/Admin/Apps/Ecommerce/Report/SalesByDay", ACLOption.Create(new SalesByDayACL(), SalesByDayACL.View)),
                                   new ChildMenuItem("Sales by payment type",
                                       "/Admin/Apps/Ecommerce/Report/SalesByPaymentType", ACLOption.Create(new SalesByPaymentTypeACL(), SalesByPaymentTypeACL.View)),
                                   new ChildMenuItem("Sales by shipping type",
                                       "/Admin/Apps/Ecommerce/Report/SalesByShippingType", ACLOption.Create(new SalesByShippingTypeACL(), SalesByShippingTypeACL.View)),
                                    new ChildMenuItem("Orders by shipping type",
                                       "/Admin/Apps/Ecommerce/Report/OrdersByShippingType", ACLOption.Create(new OrdersByShippingTypeACL(), OrdersByShippingTypeACL.View)),

                       });
            }
        }
        public int DisplayOrder { get { return 52; } }
    }
}

