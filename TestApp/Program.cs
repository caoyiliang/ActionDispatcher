// See https://aka.ms/new-console-template for more information
using ActionDispatcher;

Console.WriteLine("Hello, World!");

var dispatcher = new Dispatcher();
dispatcher.IntermediateResultEvent += Dispatcher_IntermediateResultEvent;

async Task Dispatcher_IntermediateResultEvent(float intermediateResult)
{
    Console.WriteLine(intermediateResult);
    await Task.CompletedTask;
}

Console.WriteLine(await dispatcher.RunAsync("Test1.json"));

Console.ReadKey();