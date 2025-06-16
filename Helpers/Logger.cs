using System;
using System.IO;

namespace PLManager.Helpers
{
    public static class Logger
    {
        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "app.log");

        static Logger()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath)); // Crée le dossier logs si inexistant
        }

        public static void Log(string message)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(LogFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception)
            {
                // Si on ne peut pas logger, éviter que ça crash l'application.
            }
        }
    }
}
