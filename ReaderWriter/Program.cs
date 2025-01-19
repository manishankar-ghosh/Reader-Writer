// See https://aka.ms/new-console-template for more information
using ReaderWriter;

Worker worker = new Worker();
await worker.Start();

Console.WriteLine("Done!");
