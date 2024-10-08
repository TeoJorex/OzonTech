﻿using Grpc.Core.Interceptors;
using Grpc.Core;
using Domain.Exeptions;

namespace Homework2.Interceptors
{
    public class ExceptionInterceptor : Interceptor
    {
        private readonly ILogger<ExceptionInterceptor> _logger;

        public ExceptionInterceptor(ILogger<ExceptionInterceptor> logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            try
            {
                return await continuation(request, context);
            }
            catch (NotFoundExeption ex) 
            {
                _logger.LogError(ex, "problem");

                throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
            }
        }

    }
}
