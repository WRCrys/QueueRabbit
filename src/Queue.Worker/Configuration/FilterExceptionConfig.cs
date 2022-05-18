using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Queue.Core.Utils;

namespace Queue.Worker.Configuration;

public static class FilterExceptionConfig
{
    public static IServiceCollection ResolveFilterException(this IServiceCollection services)
    {
        services.AddMvc(options =>
        {
            options.Filters.Add(typeof(FilterException));
            options.Filters.Add(typeof(FilterExceptionModelStates));
        });

        services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

        return services;
    }
}

public class FilterException : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        context.Result = new ObjectResult(new Retorno(false, context.Exception.Message))
            {StatusCode = 400};
    }
}

public class FilterExceptionModelStates : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.ModelState.IsValid)
            return;

        var validationErrors = context.ModelState
            .Keys
            .SelectMany(k => context.ModelState[k].Errors)
            .Select(e => e.ErrorMessage)
            .ToArray();

        context.Result = new ObjectResult(new Retorno(false, string.Join(";", validationErrors)))
            {StatusCode = 400};
    }
}