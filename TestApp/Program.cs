// See https://aka.ms/new-console-template for more information
using ActionDispatcher;

Console.WriteLine("Hello, World!");

Console.WriteLine(await new Dispatcher().RunAsync("Test1.json"));

Console.ReadKey();