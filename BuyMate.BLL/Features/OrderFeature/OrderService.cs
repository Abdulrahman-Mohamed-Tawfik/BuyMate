using BuyMate.BLL.Contracts;
using BuyMate.BLL.Contracts.Repositories;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using BuyMate.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuyMate.BLL.Features.OrderFeature
{
    public class OrderService : IOrderService
    {
        public ICartService _cartService { get; }
        public IProductService _productService { get; }
        public IOrderRepository _orderRepository { get; }
        public IOrderItemRepository _orderItemRepository { get; }

        public OrderService(ICartService cartService,IProductService productService, IOrderRepository orderRepository, IOrderItemRepository orderItemRepository) 
        {
            _cartService = cartService;
            _productService = productService;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
        }


        public async Task<Response<bool>> CreateOrderAsync(CheckoutViewModel model, string userId)
        {

            var cartResponse = await _cartService.GetCartAsync(userId);

            //Check if cart is available and has items
            if (cartResponse is null || cartResponse.Status is false || cartResponse.Data.Items.Count == 0)
            {
                return Response<bool>.Fail("Cannot create order. Cart is empty or unavailable.");
            }


           //Validate Address and Checkout Data

            var addr = model.ShippingAddress;
            if (addr is null)
                return Response<bool>.Fail("Shipping address is required.");

            if (string.IsNullOrWhiteSpace(addr.Street) ||
                string.IsNullOrWhiteSpace(addr.City) ||
                string.IsNullOrWhiteSpace(addr.State) ||
                string.IsNullOrWhiteSpace(addr.ZipCode))
            {
                return Response<bool>.Fail("Shipping address is incomplete.");
            }

            // Validate payment and delivery selections
            if (string.IsNullOrWhiteSpace(model.PaymentMethod))
                return Response<bool>.Fail("Payment method is required.");

            if (string.IsNullOrWhiteSpace(model.DeliveryType))
                return Response<bool>.Fail("Delivery type is required.");

            // Validate cart totals (basic sanity)
            if (cartResponse.Data.Subtotal <= 0m)
                return Response<bool>.Fail("Cart subtotal is invalid.");

            string address = $"{addr.Street}, {addr.City}, {addr.State}, {addr.ZipCode}";



            //Validate Products
            var products = await _productService.GetProductsByIdsAsync(cartResponse.Data.Items.Select(i => i.ProductId).ToList());

            if (products is null || products.Status is false)
            {
                return Response<bool>.Fail("Cannot retrieve products information.");
            }

            if (products.Data.Count != cartResponse.Data.Items.Count)
            {
                return Response<bool>.Fail("Some products in the cart are unavailable.");
            }
            //validate stock quantity
            foreach (var item in cartResponse.Data.Items)
            {
                var p = products.Data.FirstOrDefault(p => item.ProductId == p.Id);
                if (item.Quantity > p.StockQuantity)
                    return Response<bool>.Fail($"Not enough stock for product: {item.ProductName}");

                p.StockQuantity -= item.Quantity;
            }

            //Update Stock Quantity
            await _productService.UpdateProductsStockAsync(products.Data);

            //Apply coupon if available
            //To Be implemented


            //calculate order totals
            var subtotal = cartResponse.Data.Items.Select(i => i.PriceAtAddition * i.Quantity).Sum();

            var discountAmount = 0m;  //To Be implemented

            var fees = cartResponse.Data.Tax; //To Be implemented (shipping fees etc.)
            var total = subtotal - discountAmount + fees;


            //

            var entity = new Order
            {
                UserId = Guid.Parse(userId),
                OrderDate = DateTime.UtcNow,
                ShippingAddress = address,
                OrderStatus = 0,
                PaymentStatus = 0,
                Total = cartResponse.Data.Total,
                Fees = cartResponse.Data.Tax,
                Subtotal = subtotal,
                DiscountAmount = discountAmount,
                TrackingNumber =  Guid.NewGuid().ToString("N").Substring(0, 9).ToUpper()
            };

            var orderItems = new List<OrderItem>();
            var order = await _orderRepository.CreateAsync(entity);

            foreach (var item in cartResponse.Data.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.PriceAtAddition,
                    TotalPrice = item.Quantity * item.PriceAtAddition
                };
                orderItems.Add(orderItem);
            }


            //SAve Entities and Order Items
            await _orderItemRepository.CreateRangeAsync(orderItems);


            //Clear Cart
            await _cartService.ClearCart(userId);

            return Response<bool>.Success(true);
        }

        public async Task<Response<OrderViewModel>> GetUserOrderByIDForAdminAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderWithItemsAsync(orderId);
            if(order is null)
            {
                return Response<OrderViewModel>.Fail("Order not found.");
            }

            var orderViewModel = new OrderViewModel
            {
                Id = order.Id,
                UserId = order.UserId,
                CouponId = order.CouponId,
                ShippingAddress = order.ShippingAddress,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                PaymentStatus = order.PaymentStatus,
                TrackingNumber = order.TrackingNumber,
                Subtotal = order.Subtotal,
                Fees = order.Fees,
                Total = order.Total,
                Items = order.Items.Select(item => new OrderItemViewModel
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    Brand = item.Product.Brand,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice,
                    ImageUrl = item.Product.Images.Where(p => p.IsMain == true).Select(p => p.ImageUrl).FirstOrDefault()
                }).ToList()
            };

            return Response<OrderViewModel>.Success(orderViewModel);

        }

        public async Task<Response<OrderViewModel>> GetUserOrderByIDForUserAsync(Guid orderId, string userId)
        {
            var result = await GetUserOrderByIDForAdminAsync(orderId);

            if(result.Status is false || result.Data.UserId.ToString() != userId)
            {
                return Response<OrderViewModel>.Fail("Order not found.");

            }

            return result;
        }

        public async Task<Response<List<OrderViewModel>>> GetUserOrdersAsync(string userId)
        {
            var result = await _orderRepository.GetOrdersByUserIdAsync(userId);

            if(result is null)
            {
                return Response<List<OrderViewModel>>.Fail("No orders found for the user.");
            }

            var ordersList = result.Select(order => new OrderViewModel
            {
                Id = order.Id,
                UserId = order.UserId,
                CouponId = order.CouponId,
                ShippingAddress = order.ShippingAddress,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                PaymentStatus = order.PaymentStatus,
                TrackingNumber = order.TrackingNumber,
                Subtotal = order.Subtotal,
                Fees = order.Fees,
                Total = order.Total,
            }).ToList();

            return Response<List<OrderViewModel>>.Success(ordersList);


        }

        public async Task<Response<bool>> CancelOrderAsync(Guid orderId, string userId)
        {
            
            var order = await _orderRepository.GetOrderAsync(orderId);
            if (order is null || order.UserId.ToString() != userId )
            {
                return Response<bool>.Fail("Order not found or you do not have permission to delete this order.");
            }

            if(order.OrderStatus != 0) 
            {
                return Response<bool>.Fail("Only pending orders can be deleted.");
            }

            var itemIds = await _orderItemRepository.DeleteOrderItemsByOrderIdAsync(orderId);


            await _orderRepository.DeletePhysicallyAsync(orderId);
            return Response<bool>.Success(true);
        }


        public async Task<Response<bool>> DeleteOrderByAdminAsync(Guid orderId)
        {

            var order = await _orderRepository.GetOrderAsync(orderId);
            if (order is null)
            {
                return Response<bool>.Fail("Order not found or you do not have permission to delete this order.");
            }

            if (order.OrderStatus != 0)
            {
                return Response<bool>.Fail("Only pending orders can be deleted.");
            }

            var itemIds = await _orderItemRepository.DeleteOrderItemsByOrderIdAsync(orderId);


            await _orderRepository.DeletePhysicallyAsync(orderId);
            return Response<bool>.Success(true);
        }

        public async Task<Response<List<OrderViewModel>>> GetAllOrdersAsync()
        {
            var result = await _orderRepository.GetAllAsync();

            if (result is null)
            {
                return Response<List<OrderViewModel>>.Fail("No Orders Exist");
            }

            var ordersList = result.Select(order => new OrderViewModel
            {
                Id = order.Id,
                UserId = order.UserId,
                CouponId = order.CouponId,
                ShippingAddress = order.ShippingAddress,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                PaymentStatus = order.PaymentStatus,
                TrackingNumber = order.TrackingNumber,
                Subtotal = order.Subtotal,
                Fees = order.Fees,
                Total = order.Total,
            }).ToList();

            return Response<List<OrderViewModel>>.Success(ordersList);
        }

        public async Task<Response<bool>> UpdateOrderStatusByAdminAsync(Guid orderId, int status)
        {
            var _statusMapping = new Dictionary<int, string>()
            {
                { 0, "Pending" },
                { 1, "Processing"},
                { 2, "Shipped"},
                { 3, "Delivered" },
                { 4, "Cancelled" },
                {5, "Returned" }
            };

            if (!_statusMapping.ContainsKey(status))
            {
                return Response<bool>.Fail("Invalid status value.");
            }

            
            var order = await _orderRepository.GetOrderAsync(orderId);
            if (order is null)
            {
                return Response<bool>.Fail("Order not found.");
            }

            order.OrderStatus = status;
            await _orderRepository.UpdateAsync(order);
            return Response<bool>.Success(true);
        }
    }
}
