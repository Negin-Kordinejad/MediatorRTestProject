using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace WebApplication.Core.Common.Middlewares
{
    public class LoggingMiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleWare> _logger;

        public LoggingMiddleWare(RequestDelegate next, ILogger<LoggingMiddleWare> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var requestDetail = $"Http request:{httpContext.Request.Method} : {httpContext.Request.Path}";
            try
            {
                _logger.LogInformation($"Starting { requestDetail}");

                await _next(httpContext);

                _logger.LogInformation($"Finished {requestDetail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"failed {requestDetail} ");

                throw;
            }
        }
    }
    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class LoggingMiddleWareExtensions
    {
        public static IApplicationBuilder UseLoggingMiddleWare(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleWare>();
        }
    }
}
