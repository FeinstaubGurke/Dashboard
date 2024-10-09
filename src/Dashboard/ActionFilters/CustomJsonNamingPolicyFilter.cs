using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Text.Json;

namespace Dashboard.ActionFilters
{
    public class CustomJsonNamingPolicyFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result is JsonResult jsonResult)
            {
                jsonResult.SerializerSettings = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                };
            }
        }
    }
}
