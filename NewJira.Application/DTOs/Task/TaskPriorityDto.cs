// Decompiled with JetBrains decompiler
// Type: NewJira.Application.DTOs.Task.TaskPriorityDto
// Assembly: NewJira.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BDC59BCB-45F6-4270-AF08-BFE427A81A03
// Assembly location: D:\Project\TaskManagement_Swagger\publish-check-somee\NewJira.Application.dll

using System.ComponentModel.DataAnnotations;

#nullable disable
namespace NewJira.Application.DTOs.Task;

public class TaskPriorityDto
{
    [Required(ErrorMessage = "PriorityId không được để trống")]
    public int PriorityId { get; set; }
    [Required(ErrorMessage = "PriorityName không được để trống")]
    public string PriorityName { get; set; }
}
