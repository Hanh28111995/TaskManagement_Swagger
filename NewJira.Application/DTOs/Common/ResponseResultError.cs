// Decompiled with JetBrains decompiler
// Type: NewJira.Application.DTOs.Common.ResponseResultError`1
// Assembly: NewJira.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BDC59BCB-45F6-4270-AF08-BFE427A81A03
// Assembly location: D:\Project\TaskManagement_Swagger\publish-check-somee\NewJira.Application.dll

using System;

#nullable enable
namespace NewJira.Application.DTOs.Common;

public class ResponseResultError<T> : ResponseResult<T>
{
    public string[] ValidationErrors { get; set; } = Array.Empty<string>();

    public ResponseResultError(string message)
    {
        this.IsSuccess = false;
        this.Message = message;
    }

    public ResponseResultError(string[] validationErrors)
    {
        this.IsSuccess = false;
        this.ValidationErrors = validationErrors;
    }
}
