using BuyMate.BLL.Contracts;
using BuyMate.BLL.Features.Cart;
using BuyMate.DTO.ViewModels;
using BuyMate.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.Controllers
{
    public class OrderController : Controller
    {
        public IUserProfileService _userProfileService { get; }
        public ICheckoutService _checkoutService { get; }
        public IOrderService _orderservice { get; }

        public OrderController(IUserProfileService userProfileService, ICheckoutService checkoutService,IOrderService orderService)
        {
            _userProfileService = userProfileService;
            _checkoutService = checkoutService;
            _orderservice = orderService;
        }
        // for User return all orders
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


        //return order For Specific user make sure it validate user
        //Admin Action
        public async Task<IActionResult> Get(Guid orderid)
        {
            var profile = await _userProfileService.GetProfileAsync(User);
            if (profile.Status is false || profile.Data is null)
            {
                return RedirectToAction("Login");
            }

            var result = await _orderservice.GetUserOrderByIDForAdminAsync(orderid);

            if(result.Status is false)
            {
                TempData["Error"] = result.Message;
                return RedirectToAction("Index");
            }

            return View("OrderDetail",result.Data);
        }


        //For Admin return all orders
        public IActionResult GetAll()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        //create order for a specific user
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
                ModelState.AddModelError(string.Empty, result.Message);
                return RedirectToAction("Checkout", "Cart");
            }

            TempData["Success"] = "Order placed successfully!";

            return RedirectToAction("Index","Home");
        }

        //For Admin to update order status
        public IActionResult Update(Guid id)
        {
            return View();
        }

        //Cancel order if it is not processing
        public IActionResult Cancel(Guid id)
        {
            return View(null);
        }

        public IActionResult Delete(Guid id)
        {
            return View(null);
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

    }
}
