using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace BasketballTournamentsApp.Extensions
{
    public static class ExceptionMiddleware
    {
        public static void UseCustomExceptionHandler(this WebApplication app)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.ContentType = "application/problem+json";

                    var feature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var error = feature?.Error;

                    int status;
                    string detail;

                    switch (error)
                    {
                        case ArgumentException argEx:
                            status = (int)HttpStatusCode.BadRequest;
                            detail = argEx.Message;
                            break;

                        case SqlException sqlEx:
                            status = (int)HttpStatusCode.BadRequest;
                            detail = sqlEx.Number switch
                            {
                                547 => "Invalid reference: related entity does not exist (Foreign key violation).", 
                                2627 or 2601 => "Duplicate entry not allowed (Unique constraint).",   
                                0 => "A network related error occured.",
                                _ => "A database error occurred."
                            };
                            break;

                        default:
                            status = (int)HttpStatusCode.InternalServerError;
                            detail = "An unexpected error occurred.";
                            break;
                    }

                    var problem = new ProblemDetails
                    {
                        Status = status,
                        Title = status == (int)HttpStatusCode.BadRequest
                                 ? "Bad Request"
                                 : "Internal Server Error",
                        Detail = detail
                    };

                    context.Response.StatusCode = status;
                    await context.Response.WriteAsJsonAsync(problem);
                });
            });
        }
    }
}
