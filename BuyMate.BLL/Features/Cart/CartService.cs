using BuyMate.BLL.Contracts;
using BuyMate.BLL.Contracts.Repositories;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using BuyMate.Model.Entities;

namespace BuyMate.BLL.Features.Cart;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;
    public CartService(ICartRepository cartRepository, ICartItemRepository cartItemRepository)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
    }
    public async Task<Response<CartViewModel>> GetCartAsync(string userId)
    {
        var cart = await _cartRepository.GetCartWithItemsAsync(userId);

        if (cart is null)
        {
            return new Response<CartViewModel>
            {
                Data = new CartViewModel
                {
                    CartId = Guid.Empty,
                    Items = new List<CartItemViewModel>()
                },
                Message = "Cart is empty",
                Status = true
            };
        }

        var cartViewModel = new CartViewModel
        {
            CartId = cart.Id,
            Items = cart.Items.Select(item => new CartItemViewModel
            {
                ItemId = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                Quantity = item.Quantity,
                PriceAtAddition = item.PriceAtAddition,
                ImageUrl = item.Product.ProductImages.Where(p => p.IsMain == true).Select(p => p.ImageUrl).FirstOrDefault()
                //TODO: Add ImageUrl mapping when Product images are implemented
            }).ToList()
        };

        return new Response<CartViewModel>
        {
            Data = cartViewModel,

            Message = cart.Items.Count == 0 ? "Cart is empty." : "Cart retrieved successfully.",
            Status = true
        };

    }
    public async Task<Response<bool>> AddToCartAsync(string userId, Guid productId, int quantity)
    {
        var cart = await _cartRepository.GetCartWithItemsAsync(userId);
        if (cart is null)
        {
            cart = new Model.Entities.Cart
            {
                UserId = Guid.Parse(userId)
            };
            await _cartRepository.CreateAsync(cart);
        }
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
            await _cartRepository.SaveChangesAsync();
        }
        else
        {
            var newItem = new Model.Entities.CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = quantity,
                PriceAtAddition = 0 // TODO: Fetch current price from Product service
            };
            await _cartItemRepository.CreateAsync(newItem);
        }
        return new Response<bool>
        {
            Data = true,
            Message = "Item added to cart successfully.",
            Status = true
        };
    }
    public async Task<Response<bool>> UpdateItemQuantityAsync(string userId, Guid itemId, int quantity)
    {
        if (quantity <= 0)
        {
            return new Response<bool>
            {
                Data = false,
                Message = "Quantity must be greater than zero.",
                Status = false
            };
        }

        var itemToUpdate = await _cartItemRepository.GetCartItemWithProductAsync(itemId);
        if (itemToUpdate is null)
        {
            return new Response<bool>
            {
                Data = false,
                Message = "Item not found in cart.",
                Status = false
            };
        }

        if (itemToUpdate.Product!.StockQuantity < quantity)
        {
            return new Response<bool>
            {
                Data = false,
                Message = $"Only {itemToUpdate.Product.StockQuantity} units of {itemToUpdate.Product.Name} are currently in stock.",
                Status = false
            };
        }
        itemToUpdate.Quantity = quantity;
        await _cartRepository.SaveChangesAsync();

        return new Response<bool>
        {
            Data = true,
            Message = "Item quantity updated successfully.",
            Status = true
        };
    }
    public async Task<Response<bool>> RemoveFromCartAsync(Guid itemId)
    {
        var itemToDelete = await _cartItemRepository.GetCartItemWithProductAsync(itemId);
        if (itemToDelete is null)
        {
            return new Response<bool>
            {
                Data = false,
                Message = "Item not found in cart.",
                Status = false
            };
        }

        await _cartRepository.DeletePhysicallyAsync(itemToDelete.Id);

        return new Response<bool>
        {
            Status = true,
            Data = true,
            Message = "Item removed from cart successfully."
        };
    }
}

