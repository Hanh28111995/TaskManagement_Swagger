// Decompiled with JetBrains decompiler
// Type: NewJira.Application.DTOs.Task.TaskResponseDto
// Assembly: NewJira.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BDC59BCB-45F6-4270-AF08-BFE427A81A03
// Assembly location: D:\Project\TaskManagement_Swagger\publish-check-somee\NewJira.Application.dll

using NewJira.Application.DTOs.Auth;
using NewJira.Application.DTOs.Project;
using System;

#nullable enable
namespace NewJira.Application.DTOs.Task;

public class TaskResponseDto
{
    public int Id { get; set; }

    public string TaskName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Decimal EstimateHours { get; set; }

    public Decimal TimeTrackingSpentHours { get; set; }

    public Decimal TimeTrackingRemainingHours { get; set; }    

    public int StatusId { get; set; }

    public string? StatusName { get; set; }

    public int PriorityId { get; set; }

    public string? PriorityName { get; set; }

    public int TaskTypeId { get; set; }

    public string? TaskTypeName { get; set; }

    public UserMemberListResponseDto? Assignee { get; set; }

    public ProjectListResponseDto? Project { get; set; }


    
}


public class TaskListItemDto
{
    public int Id { get; set; }
    public string TaskName { get; set; } = string.Empty;

    public int ProjectId { get; set; }

    public int StatusId { get; set; }
    public string? StatusName { get; set; }

    public int PriorityId { get; set; }
    public string? PriorityName { get; set; }

    public int TaskTypeId { get; set; }
    public string? TaskTypeName { get; set; }

    public UserMemberListResponseDto? Assignee { get; set; }
}