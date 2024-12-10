using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog;
using System;
using System.IO;

namespace CV_Manager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Create the logs directory if it doesn't exist
            var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            Directory.CreateDirectory(logDirectory);

            // Build the log file path
            var logFile = Path.Combine(logDirectory, "cv-manager-.json");

            try
            {
                // Configure Serilog with a more basic setup initially
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.File(
                        formatter: new CompactJsonFormatter(),
                        path: logFile,
                        rollingInterval: RollingInterval.Day)
                    .WriteTo.Console() // Keep console logging for debugging
                    .CreateLogger();

                Log.Information("Starting CV Manager API");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                // If Serilog fails, write to console at least
                Console.WriteLine($"Fatal error: {ex.Message}");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog() // This integrates Serilog into the application
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}