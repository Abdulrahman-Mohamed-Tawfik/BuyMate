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
    private readonly IProductRepository _productRepository;
    public CartService(ICartRepository cartRepository, ICartItemRepository cartItemRepository, IProductRepository productRepository)
    {
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _productRepository = productRepository;
    }

    public async Task<Response<CartViewModel>> GetCartAsync(string userId)
    {
        var cart = await _cartRepository.GetCartWithItemsAsync(userId);

        if (cart is null)
            return Response<CartViewModel>.Fail("Cart not found.");

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
                ImageUrl = item.Product.Images.Where(p => p.IsMain == true).Select(p => p.ImageUrl).FirstOrDefault()
            }).ToList()
        };

        return Response<CartViewModel>.Success(cartViewModel, cart.Items.Count == 0 ? "Cart is empty." : "Cart retrieved successfully.");

    }

    public async Task<Response<bool>> AddToCartAsync(string userId, Guid productId, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null)
            return Response<bool>.Fail("Product not found.");

        // Get or create the user's cart
        var cart = await _cartRepository.GetCartWithItemsAsync(userId);
        if (cart is null)
        {
            cart = new Model.Entities.Cart
            {
                UserId = Guid.Parse(userId)
            };
            await _cartRepository.CreateAsync(cart);
        }

        // Check if the item already exists in the cart
        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem is not null)
        {
            var newQuantity = existingItem.Quantity + quantity;
            if (product.StockQuantity < newQuantity)
                //return Response<bool>.Fail($"Only {product.StockQuantity} units of {product.Name} are currently in stock.");
                return Response<bool>.Fail($"Cannot add {quantity} more units of {product.Name} to cart. Only {product.StockQuantity - existingItem.Quantity} additional units are available in stock.");

            existingItem.Quantity = newQuantity;
            await _cartRepository.SaveChangesAsync();
        }
        else
        {
            var newQuantity = quantity;
            if (product.StockQuantity < newQuantity)
                //return Response<bool>.Fail($"Only {product.StockQuantity} units of {product.Name} are currently in stock.");
                return Response<bool>.Fail($"Cannot add {quantity} more units of {product.Name} to cart. Only {product.StockQuantity} additional units are available in stock.");

            var newItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = newQuantity,
                PriceAtAddition = product.Price
            };
            await _cartItemRepository.CreateAsync(newItem);
        }

        return Response<bool>.Success(true, "Product added to cart successfully.");
    }

    public async Task<Response<bool>> UpdateItemQuantityAsync(string userId, Guid itemId, int quantity)
    {
        if (quantity <= 0)
            return Response<bool>.Fail("Quantity must be greater than zero.");

        var itemToUpdate = await _cartItemRepository.GetCartItemWithProductAsync(itemId);
        if (itemToUpdate is null)
            return Response<bool>.Fail("Item not found in cart.");

        if (itemToUpdate.Product!.StockQuantity < quantity)
            return Response<bool>.Fail($"Only {itemToUpdate.Product.StockQuantity} units of {itemToUpdate.Product.Name} are currently in stock.");

        itemToUpdate.Quantity = quantity;
        await _cartRepository.SaveChangesAsync();

        return Response<bool>.Success(true, "Item quantity updated successfully.");
    }

    public async Task<Response<bool>> RemoveFromCartAsync(Guid itemId)
    {
        var itemToDelete = await _cartItemRepository.GetCartItemWithProductAsync(itemId);
        if (itemToDelete is null)
            return Response<bool>.Fail("Item not found in cart.");

        var isDeleted = await _cartItemRepository.DeletePhysicallyAsync(itemToDelete.Id);
        if (!isDeleted)
            return Response<bool>.Fail("Failed to remove item from cart.");

        return Response<bool>.Success(true, "Item removed from cart successfully.");
    }
}

