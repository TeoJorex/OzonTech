using FluentValidation;
using Grpc.Core.Interceptors;
using Grpc.Core;

namespace Homework2.Interceptors
{
    public class ValidationInterceptor : Interceptor
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationInterceptor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            // Получаем валидатор для типа TRequest
            var validator = _serviceProvider.GetService<IValidator<TRequest>>();

            if (validator != null)
            {
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                    throw new RpcException(new Status(StatusCode.InvalidArgument, $"Validation failed: {errors}"));
                }
            }

            // Передаем запрос дальше, если валидация успешна
            return await continuation(request, context);
        }
    }
}
