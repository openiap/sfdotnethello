using System;
using System.IO;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            string logFile = "/tmp/dotnet_output.log";
            
            // Write to both console and file
            string[] messages = {
                "=== .NET Hello World Application ===",
                $"Current time: {DateTime.Now}",
                $"Environment: {Environment.OSVersion}",
                $"Runtime version: {Environment.Version}",
                $"Current directory: {Environment.CurrentDirectory}",
                $"Command line: {Environment.CommandLine}",
                $"Args count: {args.Length}"
            };
            
            foreach (var message in messages)
            {
                Console.WriteLine(message);
                File.AppendAllText(logFile, message + "\n");
            }
            
            for (int i = 0; i < args.Length; i++)
            {
                string argMessage = $"Arg[{i}]: {args[i]}";
                Console.WriteLine(argMessage);
                File.AppendAllText(logFile, argMessage + "\n");
            }
            
            string successMessage = "Application completed successfully!";
            Console.WriteLine(successMessage);
            File.AppendAllText(logFile, successMessage + "\n");
            
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            string errorMessage = $"ERROR: {ex.Message}\nStack trace: {ex.StackTrace}";
            Console.WriteLine(errorMessage);
            try
            {
                File.AppendAllText("/tmp/dotnet_error.log", errorMessage + "\n");
            }
            catch { /* ignore file write errors */ }
            Environment.Exit(1);
        }
    }
}
