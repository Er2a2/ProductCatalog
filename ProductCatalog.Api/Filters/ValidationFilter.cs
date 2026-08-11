using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProductCatalog.Api.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>)
                .MakeGenericType(argument.GetType());

            var validator = _serviceProvider.GetService(validatorType);

            if (validator is null)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);

            var result = await ((IValidator)validator)
                .ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        x => x.Key,
                        x => x.Select(e => e.ErrorMessage).ToArray());

                context.Result = new BadRequestObjectResult(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = "Validation failed.",
                    errors
                });

                return;
            }
        }

        await next();
    }
}
