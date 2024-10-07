﻿using Grpc.Core.Interceptors;
using Grpc.Core;

namespace Homework2.Interceptors
{
    public class LoggerInterceptor : Interceptor
    {
        private readonly ILogger<LoggerInterceptor> _logger;

        public LoggerInterceptor(ILogger<LoggerInterceptor> logger)
        {
            _logger = logger;
        }

        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request, ServerCallContext context, UnaryServerMethod<TRequest, TResponse> continuation)
        {
            _logger.LogInformation("Request {0}", request);

            var response = await continuation(request, context);
            _logger.LogInformation("Resposne {0}", response);

            return response;
        }
    }
}
