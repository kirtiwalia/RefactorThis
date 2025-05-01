using System;

namespace RefactorThis.Domain.Loggers
{
    public class AppLogger<T> : IAppLogger
    {
        private readonly string _typeName = typeof(T).Name;

        public void LogInformation(string message, params object[] args)
        {
            Console.WriteLine($"[INFO] [{_typeName}] {string.Format(message, args)}");
        }

        public void LogWarning(string message, params object[] args)
        {
            Console.WriteLine($"[WARN] [{_typeName}] {string.Format(message, args)}");
        }

        public void LogError(Exception ex, string message, params object[] args)
        {
            Console.WriteLine($"[ERROR] [{_typeName}] {string.Format(message, args)}");
            Console.WriteLine($"Exception: {ex.GetType().Name} - {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }
}