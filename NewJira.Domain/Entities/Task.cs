// Decompiled with JetBrains decompiler
// Type: NewJira.Domain.Entities.TaskItem
// Assembly: NewJira.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A3FEE43D-8244-4F6D-AA68-BB475C2465E5
// Assembly location: D:\Project\TaskManagement_Swagger\publish-check-somee\NewJira.Domain.dll

using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;
#nullable enable
namespace NewJira.Domain.Entities;

public class TaskItem
{
    [Key]
    public int Id { get; set; }

    public string TaskName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Decimal EstimateHours { get; set; }

    public Decimal TimeTrackingSpentHours { get; set; }

    public Decimal TimeTrackingRemainingHours { get; set; }

    public int ProjectId { get; set; }

    [ForeignKey("ProjectId")]
    public Project? Project { get; set; }

    public int StatusId { get; set; }

    public Status? Status { get; set; }

    public int PriorityId { get; set; }

    public Priority? Priority { get; set; }

    public int TaskTypeId { get; set; }

    public TaskType? TaskType { get; set; }

    public int? AssigneeId { get; set; }

    public User? Assignee { get; set; }

    public ICollection<Comment> Comments { get; set; } = (ICollection<Comment>)new List<Comment>();
}
