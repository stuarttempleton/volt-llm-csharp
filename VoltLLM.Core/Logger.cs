using System;
namespace VoltLLM.Core
{

    /// <summary>
    /// Provides logging functionality for informational, warning, and error messages.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The informational message to log.</param>
        public static void LogInformation(string message)
        {
            Log($"[INFO] {message}");
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        public static void LogWarning(string message)
        {
            Log($"[WARNING] {message}");
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        public static void LogError(string message)
        {
            Log($"[ERROR] {message}");
        }

        /// <summary>
        /// Logs a message to the console.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Log(string message)
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
            UnityEngine.Debug.Log(message);
#else
        Console.WriteLine(message);
#endif
        }
    }
}