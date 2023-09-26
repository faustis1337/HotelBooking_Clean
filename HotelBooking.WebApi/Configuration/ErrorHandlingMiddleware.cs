﻿using System.Net;
using System.Text.Json;
using HotelBooking.Core.Exceptions;

namespace HotelBooking.WebApi.Configuration; 

public class GlobalErrorHandlingMiddleware
{
    private readonly RequestDelegate next;

    public GlobalErrorHandlingMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode status;
        var stackTrace = string.Empty;
        string message;

        var exceptionType = exception.GetType();

        if (exceptionType == typeof(RestException)) {
            (RestException)exception.;
            message = exception.Message;
            status = (RestException)exception.s;
            stackTrace = exception.StackTrace;
        }
        else if (exceptionType == typeof(NotImplementedException))
        {
            status = HttpStatusCode.NotImplemented;
            message = exception.Message;
            stackTrace = exception.StackTrace;
        }
        else if (exceptionType == typeof(UnauthorizedAccessException))
        {
            status = HttpStatusCode.Unauthorized;
            message = exception.Message;
            stackTrace = exception.StackTrace;
        }
        else if (exceptionType == typeof(KeyNotFoundException))
        {
            status = HttpStatusCode.Unauthorized;
            message = exception.Message;
            stackTrace = exception.StackTrace;
        }
        else
        {
            status = HttpStatusCode.InternalServerError;
            message = exception.Message;
            stackTrace = exception.StackTrace;
        }

        var exceptionResult = JsonSerializer.Serialize(new { error = message, stackTrace });
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        return context.Response.WriteAsync(exceptionResult);
    }
}