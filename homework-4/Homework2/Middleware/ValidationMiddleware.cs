using FluentValidation;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Text.Json;

namespace Homework2.Middleware
{
    public class ValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _scopeFactory;

        public ValidationMiddleware(RequestDelegate next, IServiceProvider serviceProvider, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _serviceProvider = serviceProvider;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.EnableBuffering();

            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;

            if (!string.IsNullOrEmpty(body))
            {
                var modelType = GetRequestModelType(context);
                if (modelType != null)
                {
                    try
                    {
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true 
                        };
                        var model = JsonSerializer.Deserialize(body, modelType, options);

                        using (var scope = _scopeFactory.CreateScope()) 
                        {
                            var validator = GetValidatorForType(modelType, scope.ServiceProvider);
                            if (validator != null)
                            {
                                var validationResult = await validator.ValidateAsync(new ValidationContext<object>(model));
                                if (!validationResult.IsValid)
                                {
                                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                                    var errors = validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                                    await context.Response.WriteAsJsonAsync(new { Errors = errors });
                                    return;
                                }
                            }
                        }
                    }
                    catch (JsonException jsonEx)
                    {
                        // Обработка ошибок десериализации
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(new { Errors = new[] { "Некорректный формат JSON", jsonEx.Message } });
                        return;
                    }
                    catch (Exception ex)
                    {
                        // Обработка других исключений
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await context.Response.WriteAsJsonAsync(new { Errors = new[] { "Внутренняя ошибка сервера", ex.Message } });
                        return;
                    }
                }
            }

            await _next(context);
        }

        private Type GetRequestModelType(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            if (endpoint != null)
            {
                var controllerActionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (controllerActionDescriptor != null)
                {
                    var parameter = controllerActionDescriptor.Parameters.FirstOrDefault();
                    return parameter?.ParameterType;
                }
            }
            return null;
        }

        private IValidator GetValidatorForType(Type type, IServiceProvider scopedProvider)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(type);
            var validator = scopedProvider.GetService(validatorType) as IValidator;
            return validator;
        }
    }
}