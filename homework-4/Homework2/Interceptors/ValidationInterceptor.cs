using FluentValidation;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.DependencyInjection;

public class ValidationInterceptor : Interceptor
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ValidationInterceptor(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var validator = scope.ServiceProvider.GetService<IValidator<TRequest>>();

            if (validator != null)
            {
                var validationResult = await validator.ValidateAsync(request);

                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    throw new RpcException(new Status(StatusCode.InvalidArgument, errors));
                }
            }

            return await continuation(request, context);
        }
    }
}