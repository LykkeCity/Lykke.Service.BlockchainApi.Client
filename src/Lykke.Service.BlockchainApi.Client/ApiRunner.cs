﻿using System;
using System.Net;
using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Contract;
using Polly;
using Refit;

namespace Lykke.Service.BlockchainApi.Client
{
    internal class ApiRunner
    {
        private readonly int _defaultRetriesCount;

        public ApiRunner(int defaultRetriesCount = int.MaxValue)
        {
            _defaultRetriesCount = defaultRetriesCount;
        }

        public async Task RunAsync(Func<Task> method)
        {
            try
            {
                await method();
            }
            catch (ApiException ex)
            {
                throw new ErrorResponseException(GetErrorResponse(ex), ex);
            }
        }

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public async Task<T> RunAsync<T>(Func<Task<T>> method)
        {
            try
            {
                return await method();
            }
            catch (ApiException ex)
            {
                throw new ErrorResponseException(GetErrorResponse(ex), ex);
            }
        }

        public Task RunWithRetriesAsync(Func<Task> method, int? retriesCount = null)
        {
            // TODO: Update retries telemetry
            return Policy
                .Handle<Exception>(FilterRetryExceptions)
                .WaitAndRetryAsync(
                    retriesCount ?? _defaultRetriesCount,
                    GetRetryDelay)
                .ExecuteAsync(async () =>
                {
                    try
                    {
                        await method();
                    }
                    catch (ApiException ex)
                    {
                        throw new ErrorResponseException(GetErrorResponse(ex), ex);
                    }
                });
        }

        public Task<T> RunWithRetriesAsync<T>(Func<Task<T>> method, int? retriesCount = null)
        {
            // TODO: Update retries telemetry
            return Policy
                .Handle<Exception>(FilterRetryExceptions)
                .WaitAndRetryAsync(
                    retriesCount ?? _defaultRetriesCount,
                    GetRetryDelay)
                .ExecuteAsync(async () =>
                {
                    try
                    {
                        return await method();
                    }
                    catch (ApiException ex)
                    {
                        throw new ErrorResponseException(GetErrorResponse(ex), ex);
                    }
                });
        }

        private static BlockchainErrorResponse GetErrorResponse(ApiException ex)
        {
            BlockchainErrorResponse errorResponse;

            try
            {
                errorResponse = ex.GetContentAs<BlockchainErrorResponse>();
            }
            catch (Exception)
            {
                errorResponse = null;
            }

            return errorResponse ?? BlockchainErrorResponse.FromUnknownError("Blockchain API is not specify the error response");
        }

        private static bool FilterRetryExceptions(Exception ex)
        {
            if (ex.InnerException is ApiException innerApiException)
            {
                return innerApiException.StatusCode == HttpStatusCode.InternalServerError ||
                        innerApiException.StatusCode == HttpStatusCode.BadGateway ||
                        innerApiException.StatusCode == HttpStatusCode.ServiceUnavailable ||
                        innerApiException.StatusCode == HttpStatusCode.GatewayTimeout ||
                        innerApiException.StatusCode == HttpStatusCode.RequestTimeout;
            }

            if (ex is ApiException apiException)
            {
                return apiException.StatusCode == HttpStatusCode.InternalServerError ||
                       apiException.StatusCode == HttpStatusCode.BadGateway ||
                       apiException.StatusCode == HttpStatusCode.ServiceUnavailable ||
                       apiException.StatusCode == HttpStatusCode.GatewayTimeout ||
                       apiException.StatusCode == HttpStatusCode.RequestTimeout;
            }

            return true;
        }

        private static TimeSpan GetRetryDelay(int retryAttempt)
        {
            if (retryAttempt < 3)
            {
                return TimeSpan.FromMilliseconds(500 * retryAttempt);
            }

            if (retryAttempt < 8)
            {
                return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 2));
            }

            return TimeSpan.FromMinutes(1);
        }
    }
}
