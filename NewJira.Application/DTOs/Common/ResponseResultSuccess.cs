// Decompiled with JetBrains decompiler
// Type: NewJira.Application.DTOs.Common.ResponseResultSuccess`1
// Assembly: NewJira.Application, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BDC59BCB-45F6-4270-AF08-BFE427A81A03
// Assembly location: D:\Project\TaskManagement_Swagger\publish-check-somee\NewJira.Application.dll

#nullable enable
namespace NewJira.Application.DTOs.Common;

public class ResponseResultSuccess<T> : ResponseResult<T>
{
    public ResponseResultSuccess() => this.IsSuccess = true;

    public ResponseResultSuccess(string message)
    {
        this.IsSuccess = true;
        this.Message = message;
    }

    public ResponseResultSuccess(string message, T result)
    {
        this.IsSuccess = true;
        this.Message = message;
        this.ResultObject = result;
    }
}
