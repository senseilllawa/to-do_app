using Microsoft.EntityFrameworkCore;
using TodoApp.Core.DTOs;
using TodoApp.Core.Entities;
using TodoApp.Core.Interfaces;

namespace TodoApp.DataAccess.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _db;
    public TaskRepository(AppDbContext db) => _db = db;

    public async Task<(IEnumerable<TaskItem> Items, int TotalCount)> GetPagedAsync(int userId, TaskQueryParams q)
    {
        var query = _db.Tasks
            .Include(t => t.Category)
            .Where(t => t.UserId == userId);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim().ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(s) ||
                (t.Description != null && t.Description.ToLower().Contains(s)));
        }

        if (q.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == q.CategoryId.Value);

        if (q.IsCompleted.HasValue)
            query = query.Where(t => t.IsCompleted == q.IsCompleted.Value);

        query = (q.SortBy?.ToLower(), q.SortDir?.ToLower()) switch
        {
            ("title", "asc")    => query.OrderBy(t => t.Title),
            ("title", _)        => query.OrderByDescending(t => t.Title),
            ("duedate", "asc")  => query.OrderBy(t => t.DueDate ?? DateTime.MaxValue),
            ("duedate", _)      => query.OrderByDescending(t => t.DueDate ?? DateTime.MinValue),
            ("priority", "asc") => query.OrderBy(t => t.Priority),
            ("priority", _)     => query.OrderByDescending(t => t.Priority),
            (_, "asc")          => query.OrderBy(t => t.CreatedAt),
            _                   => query.OrderByDescending(t => t.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<TaskItem?> GetByIdAsync(int id, int userId) =>
        await _db.Tasks
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);

    public async Task<TaskItem> AddAsync(TaskItem task)
    {
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();
        await _db.Entry(task).Reference(t => t.Category).LoadAsync();
        return task;
    }

    public async Task UpdateAsync(TaskItem task)
    {
        _db.Tasks.Update(task);
        await _db.SaveChangesAsync();
        await _db.Entry(task).Reference(t => t.Category).LoadAsync();
    }

    public async Task DeleteAsync(TaskItem task)
    {
        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
    }
}
