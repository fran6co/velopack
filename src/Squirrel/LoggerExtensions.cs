﻿#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
using System;
using Microsoft.Extensions.Logging;

namespace Squirrel
{
    public static class LoggerExtensions
    {
        public static void Warn(this ILogger logger, string message)
        {
            logger.LogWarning(message);
        }

        public static void Warn(this ILogger logger, Exception ex, string message)
        {
            logger.LogWarning(ex, message);
        }

        public static void Warn(this ILogger logger, Exception ex)
        {
            logger.LogWarning(ex, ex.Message);
        }

        public static void Info(this ILogger logger, string message)
        {
            logger.LogInformation(message);
        }

        public static void Error(this ILogger logger, string message)
        {
            logger.LogError(message);
        }

        public static void Error(this ILogger logger, Exception ex, string message)
        {
            logger.LogError(ex, message);
        }

        public static void Error(this ILogger logger, Exception ex)
        {
            logger.LogError(ex, ex.Message);
        }

        public static void Debug(this ILogger logger, string message)
        {
            logger.LogDebug(message);
        }

        public static void Trace(this ILogger logger, string message)
        {
            logger.LogTrace(message);
        }
    }
}