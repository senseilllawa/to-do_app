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

    [Fact]
    public async Task GetTasksAsync_NormalizesPageAndPageSize()
    {
        _taskRepo.Setup(r => r.GetPagedAsync(UserId, It.IsAny<TaskQueryParams>()))
                 .ReturnsAsync((Enumerable.Empty<TaskItem>(), 0));

        var result = await _sut.GetTasksAsync(UserId, new TaskQueryParams { Page = -1, PageSize = 9999 });

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(100);
    }

    [Fact]
    public async Task GetTasksAsync_MapsItems()
    {
        var items = new[]
        {
            new TaskItem { Id = 1, Title = "A", UserId = UserId },
            new TaskItem { Id = 2, Title = "B", UserId = UserId,
                           Category = new Category { Id = 9, Name = "Cat", Color = "#fff" } }
        };
        _taskRepo.Setup(r => r.GetPagedAsync(UserId, It.IsAny<TaskQueryParams>()))
                 .ReturnsAsync((items, 2));

        var result = await _sut.GetTasksAsync(UserId, new TaskQueryParams());

        result.TotalItems.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Last().CategoryName.Should().Be("Cat");
        result.Items.Last().CategoryColor.Should().Be("#fff");
    }

    [Fact]
    public async Task CreateAsync_NoCategory_Succeeds()
    {
        var dto = new CreateTaskDto { Title = "New", Priority = TaskPriority.High };
        _taskRepo.Setup(r => r.AddAsync(It.IsAny<TaskItem>()))
                 .ReturnsAsync((TaskItem t) => { t.Id = 10; return t; });

        var result = await _sut.CreateAsync(UserId, dto);

        result.Id.Should().Be(10);
        result.Title.Should().Be("New");
        result.Priority.Should().Be(TaskPriority.High);
        _catRepo.Verify(r => r.ExistsForUserAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_CategoryBelongsToUser_Succeeds()
    {
        var dto = new CreateTaskDto { Title = "T", CategoryId = 5 };
        _catRepo.Setup(r => r.ExistsForUserAsync(5, UserId)).ReturnsAsync(true);
        _taskRepo.Setup(r => r.AddAsync(It.IsAny<TaskItem>()))
                 .ReturnsAsync((TaskItem t) => { t.Id = 1; return t; });

        var result = await _sut.CreateAsync(UserId, dto);

        result.Should().NotBeNull();
        _catRepo.Verify(r => r.ExistsForUserAsync(5, UserId), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CategoryDoesNotBelongToUser_Throws()
    {
        var dto = new CreateTaskDto { Title = "T", CategoryId = 99 };
        _catRepo.Setup(r => r.ExistsForUserAsync(99, UserId)).ReturnsAsync(false);

        var act = () => _sut.CreateAsync(UserId, dto);

        await act.Should().ThrowAsync<InvalidOperationException>();
        _taskRepo.Verify(r => r.AddAsync(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_TaskNotFound_ReturnsNull()
    {
        _taskRepo.Setup(r => r.GetByIdAsync(1, UserId)).ReturnsAsync((TaskItem?)null);

        var result = await _sut.UpdateAsync(1, UserId, new UpdateTaskDto { Title = "X" });

        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_TaskExists_UpdatesFields()
    {
        var existing = new TaskItem { Id = 1, UserId = UserId, Title = "Old" };
        _taskRepo.Setup(r => r.GetByIdAsync(1, UserId)).ReturnsAsync(existing);

        var dto = new UpdateTaskDto
        {
            Title = "New",
            Description = "Desc",
            IsCompleted = true,
            Priority = TaskPriority.High
        };

        var result = await _sut.UpdateAsync(1, UserId, dto);

        result.Should().NotBeNull();
        result!.Title.Should().Be("New");
        result.IsCompleted.Should().BeTrue();
        _taskRepo.Verify(r => r.UpdateAsync(It.IsAny<TaskItem>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_TaskExists_ReturnsTrue()
    {
        var task = new TaskItem { Id = 1, UserId = UserId };
        _taskRepo.Setup(r => r.GetByIdAsync(1, UserId)).ReturnsAsync(task);

        var result = await _sut.DeleteAsync(1, UserId);

        result.Should().BeTrue();
        _taskRepo.Verify(r => r.DeleteAsync(task), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_TaskMissing_ReturnsFalse()
    {
        _taskRepo.Setup(r => r.GetByIdAsync(1, UserId)).ReturnsAsync((TaskItem?)null);

        var result = await _sut.DeleteAsync(1, UserId);

        result.Should().BeFalse();
        _taskRepo.Verify(r => r.DeleteAsync(It.IsAny<TaskItem>()), Times.Never);
    }
}
