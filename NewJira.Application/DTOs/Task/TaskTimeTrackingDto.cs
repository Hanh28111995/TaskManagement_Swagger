// Decompiled with JetBrains decompiler
// Type: NewJira.Application.DTOs.Task.TaskTimeTrackingDto
// Assembly: NewJira.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BDC59BCB-45F6-4270-AF08-BFE427A81A03
// Assembly location: D:\Project\TaskManagement_Swagger\publish-check-somee\NewJira.Application.dll

using System;
using System.ComponentModel.DataAnnotations;

#nullable disable
namespace NewJira.Application.DTOs.Task;

public class TaskTimeTrackingDto
{
    [Range(0, 1000, ErrorMessage = "Số giờ đã làm không hợp lệ")]
    public Decimal TimeTrackingSpentHours { get; set; }

    [Range(0, 1000, ErrorMessage = "Số giờ còn lại không hợp lệ")]
    public Decimal TimeTrackingRemainingHours { get; set; }
}
