using BuyMate.BLL.Contracts;
using BuyMate.BLL.Contracts.Repositories;
using BuyMate.DTO.Category;
using BuyMate.Model.Entities;

namespace BuyMate.BLL.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repo;

        public CategoryService(ICategoryRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<CategoryViewModel>> GetAllAsync()
        {
            // We await the repo to get the data from the database
            var query = await _repo.GetAllAsync();

            // Convert Entity (Database format) -> ViewModel (API format)
            return query.Select(x => new CategoryViewModel
            {
                Id = x.Id,
                Name = x.Name
            }).ToList();
        }

        public async Task<CategoryViewModel?> GetByIdAsync(Guid id)
        {
            // Your repo uses 'GetAsync' with a filter instead of 'GetById'
            var query = await _repo.GetAsync(x => x.Id == id);

            var entity = query.FirstOrDefault();

            if (entity == null) return null;

            return new CategoryViewModel
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }

        public async Task<CategoryViewModel> CreateAsync(CreateCategoryDto dto)
        {
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = dto.Name
            };

            // 1. Add to the tracker
            await _repo.CreateAsync(category);



            return new CategoryViewModel { Id = category.Id, Name = category.Name };
        }

        public async Task<bool> UpdateAsync(Guid id, CreateCategoryDto dto)
        {
            // 1. Find the item
            var query = await _repo.GetAsync(x => x.Id == id);
            var category = query.FirstOrDefault();

            if (category == null) return false;

            // 2. Update the property
            category.Name = dto.Name;

            // 3. Save changes
            await _repo.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            // Use the specific delete method from your CommonRepository
            return await _repo.DeletePhysicallyAsync(id);
        }
    }
}