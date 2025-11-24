using BuyMate.BLL.Contracts;
using BuyMate.BLL.Contracts.Repositories;
using BuyMate.DTO.Category;
using BuyMate.DTO.Common;
using BuyMate.DTO.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using BuyMate.Infrastructure.Contracts;
using BuyMate.DTO.ViewModels;

namespace BuyMate.BLL.Features.CategoryFeatures
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFileService _fileService;

        private const string CategoriesFolderName = "Categories";
        private static readonly string[] AllowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private const long MaxFileSizeBytes = 4 * 1024 * 1024; //4 MB
        public CategoryService(ICategoryRepository categoryRepository, IFileService fileService)
        {
            _categoryRepository = categoryRepository;
            _fileService = fileService;
        }

        public async Task<Response<List<CategoryViewModel>>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            var list = await categories
                .Select(x => new CategoryViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    ImageUrl = x.ImageUrl,
                    ProductCount = x.ProductCategories.Count(pc => !pc.Product.IsDeleted)
                }).ToListAsync();

            return Response<List<CategoryViewModel>>.Success(list);
        }

        public async Task<Response<CategoryViewModel>> GetByIdAsync(Guid id)
        {
            var category = (await _categoryRepository.GetAsync(x => x.Id == id)).FirstOrDefault();
            if (category == null)
                return Response<CategoryViewModel>.Fail("Category not found.");

            var vm = new CategoryViewModel
            {
                Id = category.Id,
                Name = category.Name,
                ImageUrl = category.ImageUrl
            };

            return Response<CategoryViewModel>.Success(vm);
        }

        public async Task<Response<CategoryViewModel>> CreateAsync(CreateCategoryDto dto, IFormFile? imageFile = null)
        {
            var existing = (await _categoryRepository.GetAsync(x => x.Name == dto.Name)).FirstOrDefault();
            if (existing != null)
                return Response<CategoryViewModel>.Fail("A category with the same name already exists.");

            var category = new Model.Entities.Category
            {
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            if (imageFile != null && imageFile.Length > 0)
            {
              
                var result  = await _fileService.SaveImageAsync(imageFile,MaxFileSizeBytes, AllowedExtensions,CategoriesFolderName, category.Id.ToString());

                category.ImageUrl = "images/"+result.Data;
            }
            else
            {
                category.ImageUrl = dto.ImageUrl; // fallback if any
            }

            await _categoryRepository.CreateAsync(category);

            var vm = new CategoryViewModel { Id = category.Id, Name = category.Name, ImageUrl = category.ImageUrl };
            return Response<CategoryViewModel>.Success(vm, "Category created successfully.");
        }

        public async Task<Response<bool>> UpdateAsync(Guid id, CreateCategoryDto dto, IFormFile? imageFile = null)
        {
            var category = (await _categoryRepository.GetAsync(x => x.Id == id)).FirstOrDefault();
            if (category == null)
                return Response<bool>.Fail("Category not found.");

            var duplicate = (await _categoryRepository.GetAsync(x => x.Name == dto.Name && x.Id != id)).FirstOrDefault();
            if (duplicate != null)
                return Response<bool>.Fail("Another category with that name already exists.");

            category.Name = dto.Name;

            if (imageFile != null && imageFile.Length > 0)
            {
                 _fileService.DeleteImage(category.ImageUrl.Replace("images/","")); // Delete old image if exists

                var result = await _fileService.SaveImageAsync(imageFile, MaxFileSizeBytes, AllowedExtensions, CategoriesFolderName, category.Id.ToString());

                category.ImageUrl = "images/" + result.Data;
            }

            await _categoryRepository.SaveChangesAsync();

            return Response<bool>.Success(true, "Category updated successfully.");
        }

        public async Task<Response<bool>> DeleteAsync(Guid id)
        {
            var deleted = await _categoryRepository.DeletePhysicallyAsync(id);
            if (!deleted)
                return Response<bool>.Fail("Category not found or could not be deleted.");

            return Response<bool>.Success(true, "Category deleted successfully.");
        }
    }
}