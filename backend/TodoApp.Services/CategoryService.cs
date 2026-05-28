using TodoApp.Core.DTOs;
using TodoApp.Core.Entities;
using TodoApp.Core.Interfaces;

namespace TodoApp.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categories;

    public CategoryService(ICategoryRepository categories)
    {
        _categories = categories;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync(int userId)
    {
        var items = await _categories.GetAllAsync(userId);
        return items.Select(ToDto);
    }

    public async Task<CategoryDto?> GetByIdAsync(int id, int userId)
    {
        var c = await _categories.GetByIdAsync(id, userId);
        return c == null ? null : ToDto(c);
    }

    public async Task<CategoryDto> CreateAsync(int userId, CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Color = dto.Color,
            UserId = userId
        };
        await _categories.AddAsync(category);
        return ToDto(category);
    }

    public async Task<CategoryDto?> UpdateAsync(int id, int userId, UpdateCategoryDto dto)
    {
        var category = await _categories.GetByIdAsync(id, userId);
        if (category == null) return null;

        category.Name = dto.Name;
        category.Color = dto.Color;
        await _categories.UpdateAsync(category);
        return ToDto(category);
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var category = await _categories.GetByIdAsync(id, userId);
        if (category == null) return false;
        await _categories.DeleteAsync(category);
        return true;
    }

    private static CategoryDto ToDto(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Color = c.Color,
        TasksCount = c.Tasks?.Count ?? 0
    };
}
