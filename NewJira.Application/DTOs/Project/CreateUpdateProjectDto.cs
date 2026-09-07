// Decompiled with JetBrains decompiler
// Type: NewJira.Application.DTOs.Project.CreateProjectDto
// Assembly: NewJira.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BDC59BCB-45F6-4270-AF08-BFE427A81A03
// Assembly location: D:\Project\TaskManagement_Swagger\publish-check-somee\NewJira.Application.dll

using System.ComponentModel.DataAnnotations;

#nullable enable
namespace NewJira.Application.DTOs.Project;

public class CreateUpdateProjectDto
{
    [Required(ErrorMessage = "Tên dự án không được để trống")]
    [StringLength(100, ErrorMessage = "Tên dự án không được vượt quá 100 ký tự")]
    public string ProjectName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Phải có CategoryId cho dự án")]
    public int CategoryId { get; set; }

    public int CreatorId { get; set; } 
}
