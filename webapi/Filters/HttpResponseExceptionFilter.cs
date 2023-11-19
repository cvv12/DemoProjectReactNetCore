using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;

namespace webapi.Filters
{
    public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
    {
        private readonly ILogger<HttpResponseExceptionFilter> _logger;
        private readonly IConfiguration _configuration;

        public HttpResponseExceptionFilter(ILogger<HttpResponseExceptionFilter> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public int Order => int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception is not null)
            {
                _logger.LogError(context.Exception, "An unhandled exception has occurred.");

                // Log exception to the database
                LogExceptionToDatabase(context.Exception);

                context.Result = new ObjectResult(new { error = context.Exception.Message })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
                context.ExceptionHandled = true;
            }
        }

        private void LogExceptionToDatabase(Exception exception)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var query = @"
                INSERT INTO ExceptionLog (ExceptionMessage, StackTrace, Source, ExceptionType)
                VALUES (@ExceptionMessage, @StackTrace, @Source, @ExceptionType)";

            using (var connection = new SqlConnection(connectionString))
            {
                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ExceptionMessage", exception.Message);
                command.Parameters.AddWithValue("@StackTrace", exception.StackTrace);
                command.Parameters.AddWithValue("@Source", exception.Source);
                command.Parameters.AddWithValue("@ExceptionType", exception.GetType().ToString());

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
