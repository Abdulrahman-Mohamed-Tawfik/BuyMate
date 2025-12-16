using BuyMate.BLL.Contracts;
using BuyMate.BLL.Features.Cart;
using BuyMate.DTO.ViewModels.Order;
using BuyMate.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        public IUserProfileService _userProfileService { get; }
        public ICheckoutService _checkoutService { get; }
        public IOrderService _orderservice { get; }

        public OrderController(IUserProfileService userProfileService, ICheckoutService checkoutService, IOrderService orderService)
        {
            _userProfileService = userProfileService;
            _checkoutService = checkoutService;
            _orderservice = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var profile = await _userProfileService.GetProfileAsync(User);
            if (profile.Status is false || profile.Data is null)
            {
                return RedirectToAction("Login", "User");
            }

            var model = await _orderservice.GetUserOrdersAsync(profile.Data.Id);
            var orderData = model.Data ?? new List<OrderViewModel>();
            return View(orderData);
        }

        public async Task<IActionResult> Get(Guid orderid)
        {
            var profile = await _userProfileService.GetProfileAsync(User);
            if (profile.Status is false || profile.Data is null)
            {
                return RedirectToAction("Login");
            }

            var result = await _orderservice.GetUserOrderByIDForUserAsync(orderid, profile.Data.Id);

            if (result.Status is false)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index");
            }

            return View("OrderDetail", result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CheckoutViewModel model)
        {

            var profile = await _userProfileService.GetProfileAsync(User);
            if (profile.Status is false || profile.Data is null)
            {
                return RedirectToAction("Login");
            }


            if (!ModelState.IsValid)
            {
                var checkoutVmResult = await _checkoutService.GetCheckoutViewModelAsync(profile.Data!.Id);
                if (checkoutVmResult.Status is false)
                {
                    TempData["Error"] = checkoutVmResult.Message;
                    return RedirectToAction("Index");
                }
                model.CartVm = checkoutVmResult.Data.CartVm;
                return View("Checkout", model);
            }



            var result = await _orderservice.CreateOrderAsync(model, profile.Data.Id);

            if (result.Status is false)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index", "Cart");
            }

            TempData["Success"] = "Order placed successfully!";

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var profile = await _userProfileService.GetProfileAsync(User);
            if (profile.Status is false || profile.Data is null)
            {
                return RedirectToAction("Login");
            }

            var result = await _orderservice.CancelOrderAsync(id, profile.Data.Id.ToString());

            if (result.Status is false)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = "Order cancelled successfully.";
            }
            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var profile = await _userProfileService.GetProfileAsync(User);
            if (profile.Status is false)
            {
                return RedirectToAction("Login", "User");
            }
            var checkoutVmResult = await _checkoutService.GetCheckoutViewModelAsync(profile.Data!.Id);
            if (checkoutVmResult.Status is false)
            {
                TempData["Error"] = checkoutVmResult.Message;
                return RedirectToAction("Index");
            }
            return View(checkoutVmResult.Data);
        }


        #region Admin Actions
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderservice.GetAllOrdersAsync();

            var orderData = orders.Data ?? new List<OrderViewModel>();

            return View("AllOrders", orderData);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetById(Guid orderid)
        {
            var result = await _orderservice.GetUserOrderByIDForAdminAsync(orderid);
            if (result.Status is false)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("GetAll");
            }
            return View("AdminOrderDetail", result.Data);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var result = await _orderservice.GetUserOrderByIDForAdminAsync(id);
            if (result.Status is false)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("GetAll");
            }
            return View("EditOrder", result.Data);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateStatus(Guid id, int orderstatus)
        {
            var result = await _orderservice.UpdateOrderStatusByAdminAsync(id, orderstatus);
            if (result.Status is false)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = "Order status updated successfully.";
            }
            return RedirectToAction("GetAll");
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _orderservice.DeleteOrderByAdminAsync(id);
            if (result.Status is false)
            {
                TempData["Error"] = result.Message;
            }
            else
            {
                TempData["Success"] = "Order deleted successfully.";
            }
            return RedirectToAction("GetAll");
        }

        #endregion
    }
}
