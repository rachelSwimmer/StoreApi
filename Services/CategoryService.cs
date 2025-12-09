using StoreApi.DTOs;
using StoreApi.Interfaces;
using StoreApi.Models;

namespace StoreApi.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryService> _logger;
    
    public CategoryService(
        ICategoryRepository categoryRepository,
        ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }
    
    public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(MapToResponseDto);
    }
    
    public async Task<CategoryResponseDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category != null ? MapToResponseDto(category) : null;
    }
    
    public async Task<CategoryResponseDto> CreateCategoryAsync(CategoryCreateDto createDto)
    {
        var category = new Category
        {
            Name = createDto.Name,
            Description = createDto.Description
        };
        
        var createdCategory = await _categoryRepository.CreateAsync(category);
        _logger.LogInformation("Category created with ID: {CategoryId}", createdCategory.Id);
        
        return MapToResponseDto(createdCategory);
    }
    
    public async Task<CategoryResponseDto?> UpdateCategoryAsync(int id, CategoryUpdateDto updateDto)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(id);
        if (existingCategory == null) return null;
        
        if (updateDto.Name != null) existingCategory.Name = updateDto.Name;
        if (updateDto.Description != null) existingCategory.Description = updateDto.Description;
        
        var updatedCategory = await _categoryRepository.UpdateAsync(existingCategory);
        return updatedCategory != null ? MapToResponseDto(updatedCategory) : null;
    }
    
    public async Task<bool> DeleteCategoryAsync(int id)
    {
        return await _categoryRepository.DeleteAsync(id);
    }
    
    private static CategoryResponseDto MapToResponseDto(Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            CreatedAt = category.CreatedAt,
            ProductCount = category.Products?.Count ?? 0
        };
    }
}
