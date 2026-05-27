using TodoApp.Core.DTOs;
using TodoApp.Core.Entities;

namespace TodoApp.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}

public interface ITaskService
{
    Task<PagedResult<TaskDto>> GetTasksAsync(int userId, TaskQueryParams query);
    Task<TaskDto?> GetByIdAsync(int id, int userId);
    Task<TaskDto> CreateAsync(int userId, CreateTaskDto dto);
    Task<TaskDto?> UpdateAsync(int id, int userId, UpdateTaskDto dto);
    Task<bool> DeleteAsync(int id, int userId);
}

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync(int userId);
    Task<CategoryDto?> GetByIdAsync(int id, int userId);
    Task<CategoryDto> CreateAsync(int userId, CreateCategoryDto dto);
    Task<CategoryDto?> UpdateAsync(int id, int userId, UpdateCategoryDto dto);
    Task<bool> DeleteAsync(int id, int userId);
}

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
