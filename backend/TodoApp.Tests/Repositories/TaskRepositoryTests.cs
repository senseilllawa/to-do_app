using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using TodoApp.Core.DTOs;
using TodoApp.Core.Entities;
using TodoApp.DataAccess;
using TodoApp.DataAccess.Repositories;

namespace TodoApp.Tests.Repositories;

public class TaskRepositoryTests
{
    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<(AppDbContext db, int userId, int otherId, int catId)> SeedAsync()
    {
        var db = CreateDb();
        var u1 = new User { Email = "a@a.com", UserName = "a", PasswordHash = "x" };
        var u2 = new User { Email = "b@b.com", UserName = "b", PasswordHash = "x" };
        db.Users.AddRange(u1, u2);
        await db.SaveChangesAsync();

        var cat = new Category { Name = "Робота", Color = "#000", UserId = u1.Id };
        db.Categories.Add(cat);
        await db.SaveChangesAsync();

        var tasks = new[]
        {
            new TaskItem { Title = "Купити молоко", Description = "2л", UserId = u1.Id, CategoryId = cat.Id,
                           Priority = TaskPriority.High, IsCompleted = false, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new TaskItem { Title = "Прочитати книгу", UserId = u1.Id,
                           Priority = TaskPriority.Low, IsCompleted = true, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new TaskItem { Title = "Перевірити код", Description = "Переглянути молоко PR", UserId = u1.Id,
                           Priority = TaskPriority.Medium, IsCompleted = false, CreatedAt = DateTime.UtcNow },
            new TaskItem { Title = "Завдання іншого користувача", UserId = u2.Id, Priority = TaskPriority.Low }
        };
        db.Tasks.AddRange(tasks);
        await db.SaveChangesAsync();

        return (db, u1.Id, u2.Id, cat.Id);
    }

    [Fact]
    public async Task ОтриматиСторінку_ПовертаєТількиЗавданняКористувача()
    {
        var (db, userId, _, _) = await SeedAsync();
        var repo = new TaskRepository(db);

        var (items, total) = await repo.GetPagedAsync(userId, new TaskQueryParams { PageSize = 10 });

        total.Should().Be(3);
        items.Should().OnlyContain(t => t.UserId == userId);
    }

    [Fact]
    public async Task ОтриматиСторінку_ФільтрПошуку_ЗнаходитьЗаНазвоюТаОписом()
    {
        var (db, userId, _, _) = await SeedAsync();
        var repo = new TaskRepository(db);

        var (items, total) = await repo.GetPagedAsync(userId, new TaskQueryParams { Search = "молоко" });

        total.Should().Be(2); // "Купити молоко" + "Переглянути молоко PR"
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task ОтриматиСторінку_ФільтрКатегорії_ПовертаєВідповідні()
    {
        var (db, userId, _, catId) = await SeedAsync();
        var repo = new TaskRepository(db);

        var (items, total) = await repo.GetPagedAsync(userId, new TaskQueryParams { CategoryId = catId });

        total.Should().Be(1);
        items.Single().Title.Should().Be("Купити молоко");
    }

    [Fact]
    public async Task ОтриматиСторінку_ФільтрВиконаних_ПовертаєВиконані()
    {
        var (db, userId, _, _) = await SeedAsync();
        var repo = new TaskRepository(db);

        var (items, total) = await repo.GetPagedAsync(userId, new TaskQueryParams { IsCompleted = true });

        total.Should().Be(1);
        items.Single().Title.Should().Be("Прочитати книгу");
    }

    [Fact]
    public async Task ОтриматиСторінку_Пагінація_РозбиваєНаСторінки()
    {
        var (db, userId, _, _) = await SeedAsync();
        var repo = new TaskRepository(db);

        var (page1, total) = await repo.GetPagedAsync(userId, new TaskQueryParams { Page = 1, PageSize = 2 });
        var (page2, _) = await repo.GetPagedAsync(userId, new TaskQueryParams { Page = 2, PageSize = 2 });

        total.Should().Be(3);
        page1.Should().HaveCount(2);
        page2.Should().HaveCount(1);
    }

    [Fact]
    public async Task ОтриматиЗаId_ЗавданняІншогоКористувача_ПовертаєNull()
    {
        var (db, userId, otherId, _) = await SeedAsync();
        var repo = new TaskRepository(db);
        var theirTask = await db.Tasks.FirstAsync(t => t.UserId == otherId);

        var result = await repo.GetByIdAsync(theirTask.Id, userId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Додати_ЗберігаєЗавдання()
    {
        var (db, userId, _, _) = await SeedAsync();
        var repo = new TaskRepository(db);

        var task = new TaskItem { Title = "Нове", UserId = userId, Priority = TaskPriority.Low };
        await repo.AddAsync(task);

        task.Id.Should().BeGreaterThan(0);
        (await db.Tasks.CountAsync(t => t.UserId == userId)).Should().Be(4);
    }
}
