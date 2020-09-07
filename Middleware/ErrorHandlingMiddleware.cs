using BackEnd.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BackEnd.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (RestException ex)
            {
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex, ILogger<ErrorHandlingMiddleware> logger)
        {
            switch (ex)
            {
                case RestException re:
                    logger.LogError(ex, "REST ERROR");
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)re.Code;
                    if (re.Errors != null)
                    {
                        var resultRe = JsonConvert.SerializeObject(new
                        {
                            re.Errors
                        });

                        await context.Response.WriteAsync(resultRe);
                    }
                    
                    break;
                case Exception e:
                    logger.LogError(ex, "SERVER ERROR");
                    context.Response.ContentType = "application/json";
                    var result = JsonConvert.SerializeObject(new
                    {
                        e.Message
                    });

                    await context.Response.WriteAsync(result);
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;

            }

            
        }
    }
}
