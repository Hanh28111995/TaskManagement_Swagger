// Decompiled with JetBrains decompiler
// Type: NewJira.Application.DTOs.Task.TaskEstimateDto
// Assembly: NewJira.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BDC59BCB-45F6-4270-AF08-BFE427A81A03
// Assembly location: D:\Project\TaskManagement_Swagger\publish-check-somee\NewJira.Application.dll

using System;
using System.ComponentModel.DataAnnotations;

#nullable disable
namespace NewJira.Application.DTOs.Task;

public class TaskEstimateDto
{
	[Range(0, 1000, ErrorMessage = "Số giờ ước lượng phải lớn hơn hoặc bằng 0")]
	public Decimal EstimateHours { get; set; }
}
