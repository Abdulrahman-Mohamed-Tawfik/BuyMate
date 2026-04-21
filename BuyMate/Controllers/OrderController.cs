using BuyMate.BLL.Contracts;
using BuyMate.BLL.Features.Cart;
using BuyMate.DTO.ViewModels.Order;
using BuyMate.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    [Authorize]
    public class OrderController : BaseController
    {
        public ICheckoutService _checkoutService { get; }
        public IOrderService _orderservice { get; }

        public OrderController(ICheckoutService checkoutService, IOrderService orderService)
        {
            _checkoutService = checkoutService;
            _orderservice = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _orderservice.GetUserOrdersAsync(UserId);
            var orderData = model.Data ?? new List<OrderViewModel>();
            return View(orderData);
        }

        public async Task<IActionResult> Get(Guid orderid)
        {
            var result = await _orderservice.GetUserOrderByIDForUserAsync(orderid, UserId);

            if (result.Status is false)
            {
                SetErrorMessage(result.Message);
                return RedirectToAction("Index");
            }

            return View("OrderDetail", result.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CheckoutViewModel model)
        {

            if (!ModelState.IsValid)
            {
                var checkoutVmResult = await _checkoutService.GetCheckoutViewModelAsync(UserId);
                if (checkoutVmResult.Status is false)
                {
                    SetErrorMessage(checkoutVmResult.Message);
                    return RedirectToAction("Index");
                }
                model.CartVm = checkoutVmResult.Data.CartVm;
                return View("Checkout", model);
            }



            var result = await _orderservice.CreateOrderAsync(model, UserId);

            if (result.Status is false)
            {
                SetErrorMessage(result.Message);
                return RedirectToAction("Index", "Cart");
            }

            SetSuccessMessage("Order placed successfully!");

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var result = await _orderservice.CancelOrderAsync(id, UserId);

            if (result.Status is false)
            {
                SetErrorMessage(result.Message);
            }
            else
            {
                SetSuccessMessage("Order cancelled successfully.");
            }
            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var checkoutVmResult = await _checkoutService.GetCheckoutViewModelAsync(UserId);
            if (checkoutVmResult.Status is false)
            {
                SetErrorMessage(checkoutVmResult.Message);
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
                SetErrorMessage(result.Message);
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
                SetErrorMessage(result.Message);
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
                SetErrorMessage(result.Message);
            }
            else
            {
                SetSuccessMessage("Order status updated successfully.");
            }
            return RedirectToAction("GetAll");
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _orderservice.DeleteOrderByAdminAsync(id);
            if (result.Status is false)
            {
                SetErrorMessage(result.Message);
            }
            else
            {
                SetSuccessMessage("Order deleted successfully.");
            }
            return RedirectToAction("GetAll");
        }

        #endregion
    }
}
