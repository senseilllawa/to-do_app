using System.ComponentModel.DataAnnotations;

namespace TodoApp.Core.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int TasksCount { get; set; }
}

public class CreateCategoryDto
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required, RegularExpression("^#[0-9A-Fa-f]{6}$")]
    public string Color { get; set; } = "#6366f1";
}

public class UpdateCategoryDto
{
    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required, RegularExpression("^#[0-9A-Fa-f]{6}$")]
    public string Color { get; set; } = string.Empty;
}
