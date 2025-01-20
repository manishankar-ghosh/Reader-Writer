using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReaderWriter
{
    internal class Worker
    {
        private AutoResetEvent _signal = new (false);
        private ConcurrentQueue<int> _data = new();
        private bool readInProgress;
        private async Task ReadAsync()
        {
            readInProgress = true;
            for (int i = 0; i < 5; i++) // Simulate few reads
            {
                Console.WriteLine("Reading data...");
                //_signal.Reset();
                await Task.Delay(500);
                _data.Enqueue(i);
                _signal.Set();
            }
            _data.Enqueue(-1);
            _signal.Set();
            readInProgress = false;
            Console.WriteLine("End of Reader Thread");
        }

        private async Task WritAsync()
        {
            while (true)
            {
                if (_data.IsEmpty || readInProgress)
                {
                    Console.WriteLine("Waiting for data...");
                    _signal.WaitOne();
                }

                if(_data.TryDequeue(out int data))
                {
                    if (data == -1)
                    {
                        break;
                    }

                    Console.Write($"\r\nData received {data}. Writing into db...");
                    await Task.Delay(3000);
                    Console.WriteLine($" Write complete!");
                }
            }

            Console.WriteLine("End of Writer Thread");
        }

        public async Task Start() 
        { 
            Task t1 = ReadAsync();
            Task t2 = WritAsync();

            await Task.WhenAll(t1, t2);
        }
    }
}
