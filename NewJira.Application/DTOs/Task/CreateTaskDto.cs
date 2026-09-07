// Decompiled with JetBrains decompiler
// Type: NewJira.Application.DTOs.Task.CreateTaskDto
// Assembly: NewJira.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BDC59BCB-45F6-4270-AF08-BFE427A81A03
// Assembly location: D:\Project\TaskManagement_Swagger\publish-check-somee\NewJira.Application.dll

using System;
using System.ComponentModel.DataAnnotations;

#nullable enable
namespace NewJira.Application.DTOs.Task;

public class CreateTaskDto
{
	[Required(ErrorMessage = "Tên công việc không được để trống")]
	[StringLength(150, ErrorMessage = "Tên công việc không được vượt quá 150 ký tự")]
	public string TaskName { get; set; } = string.Empty;

	public string? Description { get; set; }

	public Decimal EstimateHours { get; set; }

	public Decimal TimeTrackingSpentHours { get; set; }

	public Decimal TimeTrackingRemainingHours { get; set; }

	[Required(ErrorMessage = "Phải chỉ định ProjectId cho công việc")]
	public int ProjectId { get; set; }

	[Required(ErrorMessage = "Phải có trạng thái cho công việc")]
	public int StatusId { get; set; }

	[Required(ErrorMessage = "Phải có mức độ ưu tiên")]
	public int PriorityId { get; set; }

	[Required(ErrorMessage = "Phải chọn loại công việc")]
	public int TaskTypeId { get; set; }

	public int? AssigneeId { get; set; }
}
