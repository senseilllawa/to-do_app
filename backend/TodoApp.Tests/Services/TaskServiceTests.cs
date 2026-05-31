using FluentAssertions;
using Moq;
using Xunit;
using TodoApp.Core.DTOs;
using TodoApp.Core.Entities;
using TodoApp.Core.Interfaces;
using TodoApp.Services;

namespace TodoApp.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepo = new();
    private readonly Mock<ICategoryRepository> _catRepo = new();
    private readonly TaskService _sut;

    private const int UserId = 42;

    public TaskServiceTests()
    {
        _sut = new TaskService(_taskRepo.Object, _catRepo.Object);
    }

    [Fact(DisplayName = "Отримати завдання: нормалізує сторінку та розмір при некоректних значеннях")]
    public async Task ОтриматиЗавдання_НормалізуєСторінкуТаРозмір()
    {
        _taskRepo.Setup(r => r.GetPagedAsync(UserId, It.IsAny<TaskQueryParams>()))
                 .ReturnsAsync((Enumerable.Empty<TaskItem>(), 0));

        var result = await _sut.GetTasksAsync(UserId, new TaskQueryParams { Page = -1, PageSize = 9999 });

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(100);
    }

    [Fact(DisplayName = "Отримати завдання: правильно мапує елементи разом з категорією")]
    public async Task ОтриматиЗавдання_МапуєЕлементи()
    {
        var items = new[]
        {
            new TaskItem { Id = 1, Title = "А", UserId = UserId },
            new TaskItem { Id = 2, Title = "Б", UserId = UserId,
                           Category = new Category { Id = 9, Name = "Кат", Color = "#fff" } }
        };
        _taskRepo.Setup(r => r.GetPagedAsync(UserId, It.IsAny<TaskQueryParams>()))
                 .ReturnsAsync((items, 2));

        var result = await _sut.GetTasksAsync(UserId, new TaskQueryParams());

        result.TotalItems.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Last().CategoryName.Should().Be("Кат");
        result.Items.Last().CategoryColor.Should().Be("#fff");
    }

    [Fact(DisplayName = "Створити: без категорії → успішно створює завдання")]
    public async Task Створити_БезКатегорії_Успішно()
    {
        var dto = new CreateTaskDto { Title = "Нове завдання", Priority = TaskPriority.High };
        _taskRepo.Setup(r => r.AddAsync(It.IsAny<TaskItem>()))
                 .ReturnsAsync((TaskItem t) => { t.Id = 10; return t; });

        var result = await _sut.CreateAsync(UserId, dto);

        result.Id.Should().Be(10);
        result.Title.Should().Be("Нове завдання");
        result.Priority.Should().Be(TaskPriority.High);
        _catRepo.Verify(r => r.ExistsForUserAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact(DisplayName = "Створити: категорія належить користувачу → успішно")]
    public async Task Створити_КатегоріяНалежитьКористувачу_Успішно()
    {
        var dto = new CreateTaskDto { Title = "Завдання", CategoryId = 5 };
        _catRepo.Setup(r => r.ExistsForUserAsync(5, UserId)).ReturnsAsync(true);
        _taskRepo.Setup(r => r.AddAsync(It.IsAny<TaskItem>()))
                 .ReturnsAsync((TaskItem t) => { t.Id = 1; return t; });

        var result = await _sut.CreateAsync(UserId, dto);

        result.Should().NotBeNull();
        _catRepo.Verify(r => r.ExistsForUserAsync(5, UserId), Times.Once);
    }

    [Fact(DisplayName = "Створити: чужа категорія → кидає виняток")]
    public async Task Створити_КатегоріяНеНалежитьКористувачу_КидаєВиняток()
    {
        var dto = new CreateTaskDto { Title = "Завдання", CategoryId = 99 };
        _catRepo.Setup(r => r.ExistsForUserAsync(99, UserId)).ReturnsAsync(false);

        var act = () => _sut.CreateAsync(UserId, dto);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _taskRepo.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact(DisplayName = "Оновити: завдання не знайдено → повертає null")]
    public async Task Оновити_ЗавданняНеЗнайдено_ПовертаєNull()
    {
        _taskRepo.Setup(r => r.GetByIdAsync(1, UserId)).ReturnsAsync((TaskItem?)null);

        var result = await _sut.UpdateAsync(1, UserId, new UpdateTaskDto { Title = "X" });

        result.Should().BeNull();
    }

    [Fact(DisplayName = "Оновити: завдання існує → оновлює поля")]
    public async Task Оновити_ЗавданняІснує_ОновлюєПоля()
    {
        var existing = new TaskItem { Id = 1, UserId = UserId, Title = "Старе" };
        _taskRepo.Setup(r => r.GetByIdAsync(1, UserId)).ReturnsAsync(existing);

        var dto = new UpdateTaskDto
        {
            Title = "Нове",
            Description = "Опис",
            IsCompleted = true,
            Priority = TaskPriority.High
        };

        var result = await _sut.UpdateAsync(1, UserId, dto);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Нове");
        result.IsCompleted.Should().BeTrue();
        _taskRepo.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact(DisplayName = "Видалити: завдання існує → повертає true")]
    public async Task Видалити_ЗавданняІснує_ПовертаєTrue()
    {
        var task = new TaskItem { Id = 1, UserId = UserId };
        _taskRepo.Setup(r => r.GetByIdAsync(1, UserId)).ReturnsAsync(task);

        var result = await _sut.DeleteAsync(1, UserId);

        result.Should().BeTrue();
        _taskRepo.Verify(r => r.DeleteAsync(task), Times.Once);
    }

    [Fact(DisplayName = "Видалити: завдання відсутнє → повертає false")]
    public async Task Видалити_ЗавданняВідсутнє_ПовертаєFalse()
    {
        _taskRepo.Setup(r => r.GetByIdAsync(1, UserId)).ReturnsAsync((TaskItem?)null);

        var result = await _sut.DeleteAsync(1, UserId);

        result.Should().BeFalse();
        _taskRepo.Verify(r => r.DeleteAsync(It.IsAny<TaskItem>()), Times.Never);
    }
}
