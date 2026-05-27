using Microsoft.EntityFrameworkCore;
using TodoApp.Core.Entities;

namespace TodoApp.DataAccess;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var demoUser = new User
        {
            Email = "demo@todo.app",
            UserName = "demo",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("demo1234"),
            CreatedAt = DateTime.UtcNow
        };
        db.Users.Add(demoUser);
        await db.SaveChangesAsync();

        var work = new Category { Name = "Work",     Color = "#6366f1", UserId = demoUser.Id };
        var pers = new Category { Name = "Personal", Color = "#10b981", UserId = demoUser.Id };
        var shop = new Category { Name = "Shopping", Color = "#f59e0b", UserId = demoUser.Id };
        var heal = new Category { Name = "Health",   Color = "#ef4444", UserId = demoUser.Id };
        db.Categories.AddRange(work, pers, shop, heal);
        await db.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var tasks = new List<TaskItem>
        {
            new() { Title = "Finish project proposal", Description = "Send to client by Friday",
                    UserId = demoUser.Id, CategoryId = work.Id, Priority = TaskPriority.High,
                    DueDate = now.AddDays(3), CreatedAt = now.AddDays(-2) },
            new() { Title = "Code review for team PR", Description = "Review 3 pull requests",
                    UserId = demoUser.Id, CategoryId = work.Id, Priority = TaskPriority.Medium,
                    DueDate = now.AddDays(1), CreatedAt = now.AddDays(-1) },
            new() { Title = "Update sprint board",
                    UserId = demoUser.Id, CategoryId = work.Id, Priority = TaskPriority.Low,
                    IsCompleted = true, CreatedAt = now.AddDays(-5) },

            new() { Title = "Call mom", UserId = demoUser.Id, CategoryId = pers.Id,
                    Priority = TaskPriority.Medium, CreatedAt = now.AddHours(-3) },
            new() { Title = "Plan weekend trip", Description = "Book hotel and tickets",
                    UserId = demoUser.Id, CategoryId = pers.Id, Priority = TaskPriority.Low,
                    DueDate = now.AddDays(7), CreatedAt = now.AddDays(-3) },

            new() { Title = "Buy groceries", Description = "Milk, eggs, bread, cheese",
                    UserId = demoUser.Id, CategoryId = shop.Id, Priority = TaskPriority.Medium,
                    DueDate = now.AddDays(1), CreatedAt = now.AddHours(-12) },
            new() { Title = "Order new laptop charger",
                    UserId = demoUser.Id, CategoryId = shop.Id, Priority = TaskPriority.High,
                    CreatedAt = now.AddHours(-5) },

            new() { Title = "Gym workout", Description = "Leg day",
                    UserId = demoUser.Id, CategoryId = heal.Id, Priority = TaskPriority.Medium,
                    IsCompleted = true, CreatedAt = now.AddDays(-1) },
            new() { Title = "Schedule dentist appointment",
                    UserId = demoUser.Id, CategoryId = heal.Id, Priority = TaskPriority.Low,
                    CreatedAt = now.AddDays(-4) },

            new() { Title = "Read 'Clean Architecture'",
                    UserId = demoUser.Id, Priority = TaskPriority.Low,
                    CreatedAt = now.AddDays(-10) },
            new() { Title = "Backup important files",
                    UserId = demoUser.Id, Priority = TaskPriority.Medium,
                    DueDate = now.AddDays(2), CreatedAt = now.AddDays(-1) },
            new() { Title = "Review monthly budget",
                    UserId = demoUser.Id, CategoryId = pers.Id, Priority = TaskPriority.High,
                    CreatedAt = now.AddHours(-8) }
        };

        db.Tasks.AddRange(tasks);
        await db.SaveChangesAsync();
    }
}
