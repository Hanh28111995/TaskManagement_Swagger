// Decompiled with JetBrains decompiler
// Type: NewJira.Domain.Entities.Priority
// Assembly: NewJira.Domain, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: A3FEE43D-8244-4F6D-AA68-BB475C2465E5
// Assembly location: D:\Project\TaskManagement_Swagger\publish-check-somee\NewJira.Domain.dll

#nullable enable
namespace NewJira.Domain.Entities;

public class Priority
{
    public int Id { get; set; }

    public int PriorityRank { get; set; }

    public string PriorityName { get; set; } = string.Empty;
}
