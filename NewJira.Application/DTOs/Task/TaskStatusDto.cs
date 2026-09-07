// Decompiled with JetBrains decompiler
// Type: NewJira.Application.DTOs.Task.TaskStatusDto
// Assembly: NewJira.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BDC59BCB-45F6-4270-AF08-BFE427A81A03
// Assembly location: D:\Project\TaskManagement_Swagger\publish-check-somee\NewJira.Application.dll

using System.ComponentModel.DataAnnotations;

#nullable disable
namespace NewJira.Application.DTOs.Task;

public class TaskStatusDto
{
    [Required(ErrorMessage = "StatusId không được để trống")]
    public int StatusId { get; set; }
    [Required(ErrorMessage = "StatusName không được để trống")]
    public string StatusName { get; set; }
}
