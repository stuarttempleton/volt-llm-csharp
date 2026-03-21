using System;
namespace VoltLLM.Core
{

    /// <summary>
    /// Provides logging functionality for informational, warning, and error messages.
    /// </summary>
    public static class Logger
    {
        private static bool UseColor = false;
        private static ConsoleColor OriginalColor = Console.ForegroundColor;
        
        /// <summary>
        /// Sets whether to use colored output in console logging.
        /// </summary>
        /// <param name="value">True to enable colored output; false to disable it.</param>
        public static void SetUseColor(bool value)
        {
            UseColor = value;
        }
        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The informational message to log.</param>
        public static void LogInformation(string message)
        {
            Log($"[INFO] {message}", ConsoleColor.DarkGray);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The warning message to log.</param>
        public static void LogWarning(string message)
        {
            Log($"[WARNING] {message}", ConsoleColor.Yellow);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The error message to log.</param>
        public static void LogError(string message)
        {
            Log($"[ERROR] {message}", ConsoleColor.Red);
        }

        /// <summary>
        /// Logs a message to the console.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="color">The color to use for the message.</param>
        public static void Log(string message, ConsoleColor? color = null)
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
            UnityEngine.Debug.Log(message);
#else
        if (UseColor && color.HasValue)
        {
            OriginalColor = Console.ForegroundColor;
            Console.ForegroundColor = color.Value;
        }
        Console.WriteLine(message);
        if (UseColor && color.HasValue)
        {
            Console.ForegroundColor = OriginalColor;
        }
#endif
        }
    }
}