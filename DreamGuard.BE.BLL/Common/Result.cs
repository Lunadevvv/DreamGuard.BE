using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamGuard.BE.BLL.Common
{
    public class Result
    {
        public bool Succeeded { get; init; }
        public string? Error { get; init; }
        public int StatusCode { get; init; }
        public string Message { get; init; } = string.Empty;

        public static Result Success(string message) =>
            new() { Succeeded = true, Message = message, StatusCode = 200 };

        public static Result Failure(string error, int statusCode) =>
            new() { Succeeded = false, Error = error, StatusCode = statusCode };
    }

    public class Result<T> : Result
    {
        public T? Data { get; init; }

        public static Result<T> Success(T? data) =>
            new() { Succeeded = true, Data = data, StatusCode = 200 };

        public static Result<T> Failure(string error, int statusCode) =>
            new() { Succeeded = false, Error = error, StatusCode = statusCode };
    }

}
