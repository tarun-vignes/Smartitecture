using System;
using System.IO;
using System.Reflection;

namespace DebugApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Smartitecture Debug Tool ===");
            Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");
            Console.WriteLine($"Assembly Location: {Assembly.GetExecutingAssembly().Location}");
            Console.WriteLine($"Base Directory: {AppDomain.CurrentDomain.BaseDirectory}");
            
            Console.WriteLine("\n=== Checking for required files ===");
            
            // Check for appsettings.json
            string appSettings = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            Console.WriteLine($"appsettings.json exists: {File.Exists(appSettings)}");
            if (File.Exists(appSettings))
            {
                Console.WriteLine($"appsettings.json content:\n{File.ReadAllText(appSettings)}");
            }
            
            // Check for minimal_server.py
            string pythonScript = Path.Combine(Directory.GetCurrentDirectory(), "minimal_server.py");
            Console.WriteLine($"minimal_server.py exists: {File.Exists(pythonScript)}");
            
            // Check for backend-python directory
            string backendDir = Path.Combine(Directory.GetCurrentDirectory(), "backend-python");
            Console.WriteLine($"backend-python directory exists: {Directory.Exists(backendDir)}");
            if (Directory.Exists(backendDir))
            {
                Console.WriteLine("Files in backend-python:");
                foreach (var file in Directory.GetFiles(backendDir))
                {
                    Console.WriteLine($"  - {Path.GetFileName(file)}");
                }
            }
            
            Console.WriteLine("\n=== Environment Information ===");
            Console.WriteLine($"OS Version: {Environment.OSVersion}");
            Console.WriteLine($".NET Version: {Environment.Version}");
            Console.WriteLine($"Is 64-bit: {Environment.Is64BitProcess}");
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
