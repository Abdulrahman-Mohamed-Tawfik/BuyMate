using BuyMate.BLL.Contracts;
using BuyMate.BLL.Contracts.Repositories;
using BuyMate.BLL.GlobalHelpers;
using BuyMate.DTO.Category;
using BuyMate.DTO.Common;
using BuyMate.DTO.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace BuyMate.BLL.Features.CategoryFeatures
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<Response<List<CategoryViewModel>>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            var list = await categories
                .Select(x => new CategoryViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    ImageUrl = x.ImageUrl
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
                var originalFileName = Path.GetFileName(imageFile.FileName);
                var combinedFileName = $"{category.Id}_{originalFileName}"; // categoryId + imageName
                var physicalPath = AppHelper.GetImagePhysicalPath(ImageTypes.Categories, combinedFileName);

                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                category.ImageUrl = AppHelper.GetImageRelativePath(ImageTypes.Categories, combinedFileName);
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
                var originalFileName = Path.GetFileName(imageFile.FileName);
                var combinedFileName = $"{category.Id}_{originalFileName}"; // categoryId + imageName
                var physicalPath = AppHelper.GetImagePhysicalPath(ImageTypes.Categories, combinedFileName);

                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                category.ImageUrl = AppHelper.GetImageRelativePath(ImageTypes.Categories, combinedFileName);
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