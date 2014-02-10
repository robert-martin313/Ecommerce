using System.Web.Mvc;
using MrCMS.Services;
using MrCMS.Web.Apps.Ecommerce.Models;
using MrCMS.Web.Apps.Ecommerce.Pages;
using MrCMS.Web.Apps.Ecommerce.Services.Orders;
using MrCMS.Web.Apps.Ecommerce.Services.Paypoint;
using MrCMS.Website.Controllers;

namespace MrCMS.Web.Apps.Ecommerce.Controllers
{
    public class PaypointController : MrCMSAppUIController<EcommerceApp>
    {
        private readonly IPaypointPaymentService _paypointPaymentService;
        private readonly IDocumentService _documentService;
        private readonly CartModel _cartModel;
        private readonly IOrderService _orderService;
        private readonly IPaypoint3DSecureHelper _paypoint3DSecureHelper;

        public PaypointController(IPaypointPaymentService paypointPaymentService, IDocumentService documentService, CartModel cartModel, IOrderService orderService, IPaypoint3DSecureHelper paypoint3DSecureHelper)
        {
            _paypointPaymentService = paypointPaymentService;
            _documentService = documentService;
            _cartModel = cartModel;
            _orderService = orderService;
            _paypoint3DSecureHelper = paypoint3DSecureHelper;
        }

        public PartialViewResult PaymentDetails(PaypointPaymentDetailsModel model)
        {
            return PartialView(model);
        }

        public ActionResult Response3DSecure(FormCollection formCollection)
        {
            if (!_cartModel.CanPlaceOrder)
            {
                TempData["error-details"] = new FailureDetails
                {
                    Message =
                        "We were unable to process your order with the specified cart. Please check your details and try again"
                };
                return _documentService.RedirectTo<PaymentDetails>();
            }
            if (_cartModel.CartGuid != _paypoint3DSecureHelper.GetCartGuid() ||
                _cartModel.Total != _paypoint3DSecureHelper.GetOrderAmount())
            {
                TempData["error-details"] = new FailureDetails
                {
                    Message = "Your order was changed after going off to PayPoint for 3D secure validation. No payment has been taken, but you will need to re-submit your details."
                };
                return _documentService.RedirectTo<PaymentDetails>();
            }

            var response = _paypointPaymentService.Handle3DSecureResponse(formCollection);

            if (response.Requires3DSecure)
            {
                TempData["redirect-details"] = response.RedirectDetails;
                return RedirectToAction("Redirect3DSecure");
            }

            if (response.PaymentSucceeded)
            {
                var order = _orderService.PlaceOrder(_cartModel, o =>
                {
                    o.PaymentStatus = PaymentStatus.Paid;
                    o.ShippingStatus = ShippingStatus.Unshipped;
                    o.AuthorisationToken = response.PaypointPaymentDetails.AuthCode;
                });
                return _documentService.RedirectTo<OrderPlaced>(new { id = order.Guid });
            }

            TempData["error-details"] = response.FailureDetails;
            return _documentService.RedirectTo<PaymentDetails>();
        }

        public ActionResult Redirect3DSecure()
        {
            var redirectDetails = TempData["redirect-details"] as RedirectDetails;
            if (redirectDetails != null)
                return View(redirectDetails);
            return _documentService.RedirectTo<PaymentDetails>();
        }
    }
}