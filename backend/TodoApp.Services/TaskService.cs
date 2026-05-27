using TodoApp.Core.DTOs;
using TodoApp.Core.Entities;
using TodoApp.Core.Interfaces;

namespace TodoApp.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _tasks;
    private readonly ICategoryRepository _categories;

    public TaskService(ITaskRepository tasks, ICategoryRepository categories)
    {
        _tasks = tasks;
        _categories = categories;
    }

    public async Task<PagedResult<TaskDto>> GetTasksAsync(int userId, TaskQueryParams query)
    {
        if (query.Page < 1) query.Page = 1;
        if (query.PageSize < 1) query.PageSize = 10;
        if (query.PageSize > 100) query.PageSize = 100;

        var (items, total) = await _tasks.GetPagedAsync(userId, query);

        return new PagedResult<TaskDto>
        {
            Items = items.Select(ToDto),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalItems = total
        };
    }

    public async Task<TaskDto?> GetByIdAsync(int id, int userId)
    {
        var t = await _tasks.GetByIdAsync(id, userId);
        return t == null ? null : ToDto(t);
    }

    public async Task<TaskDto> CreateAsync(int userId, CreateTaskDto dto)
    {
        await ValidateCategoryAsync(dto.CategoryId, userId);

        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            DueDate = dto.DueDate?.ToUniversalTime(),
            Priority = dto.Priority,
            CategoryId = dto.CategoryId,
            UserId = userId
        };

        await _tasks.AddAsync(task);
        return ToDto(task);
    }

    public async Task<TaskDto?> UpdateAsync(int id, int userId, UpdateTaskDto dto)
    {
        var task = await _tasks.GetByIdAsync(id, userId);
        if (task == null) return null;

        await ValidateCategoryAsync(dto.CategoryId, userId);

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.IsCompleted = dto.IsCompleted;
        task.DueDate = dto.DueDate?.ToUniversalTime();
        task.Priority = dto.Priority;
        task.CategoryId = dto.CategoryId;

        await _tasks.UpdateAsync(task);
        return ToDto(task);
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var task = await _tasks.GetByIdAsync(id, userId);
        if (task == null) return false;
        await _tasks.DeleteAsync(task);
        return true;
    }

    private async Task ValidateCategoryAsync(int? categoryId, int userId)
    {
        if (categoryId.HasValue && !await _categories.ExistsForUserAsync(categoryId.Value, userId))
            throw new InvalidOperationException("Category not found or doesn't belong to user.");
    }

    internal static TaskDto ToDto(TaskItem t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Description = t.Description,
        IsCompleted = t.IsCompleted,
        DueDate = t.DueDate,
        Priority = t.Priority,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt,
        CategoryId = t.CategoryId,
        CategoryName = t.Category?.Name,
        CategoryColor = t.Category?.Color
    };
}
