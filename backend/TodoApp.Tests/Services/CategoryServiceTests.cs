using FluentAssertions;
using Moq;
using Xunit;
using TodoApp.Core.DTOs;
using TodoApp.Core.Entities;
using TodoApp.Core.Interfaces;
using TodoApp.Services;

namespace TodoApp.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repo = new();
    private readonly CategoryService _sut;

    private const int UserId = 7;

    public CategoryServiceTests()
    {
        _sut = new CategoryService(_repo.Object);
    }

    [Fact(DisplayName = "Отримати всі: повертає категорії з кількістю завдань")]
    public async Task ОтриматиВсі_ПовертаєКатегоріїЗКількістюЗавдань()
    {
        var cats = new[]
        {
            new Category { Id = 1, Name = "Робота", Color = "#000", UserId = UserId,
                           Tasks = new List<TaskItem> { new(), new(), new() } },
            new Category { Id = 2, Name = "Дім", Color = "#fff", UserId = UserId, Tasks = new List<TaskItem>() }
        };
        _repo.Setup(r => r.GetAllAsync(UserId)).ReturnsAsync(cats);

        var result = (await _sut.GetAllAsync(UserId)).ToList();

        result.Should().HaveCount(2);
        result[0].TasksCount.Should().Be(3);
        result[1].TasksCount.Should().Be(0);
    }

    [Fact(DisplayName = "Створити: зберігає та повертає DTO з правильними полями")]
    public async Task Створити_ЗберігаєТаПовертаєDto()
    {
        var dto = new CreateCategoryDto { Name = "Книги", Color = "#abcdef" };
        _repo.Setup(r => r.AddAsync(It.IsAny<Category>()))
             .ReturnsAsync((Category c) => { c.Id = 1; return c; });

        var result = await _sut.CreateAsync(UserId, dto);

        result.Id.Should().Be(1);
        result.Name.Should().Be("Книги");
        result.Color.Should().Be("#abcdef");
    }

    [Fact(DisplayName = "Оновити: категорію не знайдено → повертає null")]
    public async Task Оновити_НеЗнайдено_ПовертаєNull()
    {
        _repo.Setup(r => r.GetByIdAsync(1, UserId)).ReturnsAsync((Category?)null);

        var result = await _sut.UpdateAsync(1, UserId, new UpdateCategoryDto { Name = "X", Color = "#000000" });

        result.Should().BeNull();
    }

    [Fact(DisplayName = "Оновити: категорію знайдено → оновлює назву та колір")]
    public async Task Оновити_Знайдено_ОновлюєПоля()
    {
        var existing = new Category { Id = 1, UserId = UserId, Name = "Старе", Color = "#111111" };
        _repo.Setup(r => r.GetByIdAsync(1, UserId)).ReturnsAsync(existing);

        var result = await _sut.UpdateAsync(1, UserId, new UpdateCategoryDto { Name = "Нове", Color = "#222222" });

        result!.Name.Should().Be("Нове");
        result.Color.Should().Be("#222222");
        _repo.Verify(r => r.UpdateAsync(existing), Times.Once);
    }

    [Fact(DisplayName = "Видалити: категорію знайдено → повертає true")]
    public async Task Видалити_Знайдено_ПовертаєTrue()
    {
        var existing = new Category { Id = 1, UserId = UserId };
        _repo.Setup(r => r.GetByIdAsync(1, UserId)).ReturnsAsync(existing);

        var result = await _sut.DeleteAsync(1, UserId);

        result.Should().BeTrue();
        _repo.Verify(r => r.DeleteAsync(existing), Times.Once);
    }
}
