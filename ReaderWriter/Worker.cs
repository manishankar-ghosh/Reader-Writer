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
        private AutoResetEvent _signalReader = new(false);
        private AutoResetEvent _signalWriter = new(false);
        private int _readerThresholdCount = 3;
        private ConcurrentQueue<int> _data = new();
        private bool readInProgress;
        private async Task ReadAsync()
        {
            readInProgress = true;
            for (int i = 0; i < 10; i++) // Simulate few reads
            {
                if(_data.Count > _readerThresholdCount) // To ensure that we do not read too much data in memory while writer is struggling to consume data
                {
                    var timeNow = DateTime.Now;
                    Console.WriteLine("Reader Threshold reached halting for some time.");
                    _signalWriter.WaitOne();
                    var gap = (DateTime.Now - timeNow).TotalMilliseconds;
                    Console.WriteLine($"Resuming read after {gap} ms");
                }
                Console.Write("\r\nReader --> Reading data...");
                //_signal.Reset();
                await Task.Delay(1000);
                _data.Enqueue(i);
                Console.WriteLine($"Put data value '{i}' into queue.");
                _signalReader.Set();
            }
            //_data.Enqueue(-1);
            readInProgress = false;

            _signalReader.Set();
            Console.WriteLine("End of Reader Thread");
        }

        private async Task WritAsync()
        {
            while (true)
            {
                if (_data.IsEmpty && readInProgress)
                {
                    Console.WriteLine("Writer --> Waiting for data...");
                    _signalReader.WaitOne();
                }
                else if (_data.TryDequeue(out int data))
                {
                    _signalWriter.Set();

                    //if (data == -1)
                    //{
                    //    break;
                    //}

                    Console.WriteLine($"\r\nDequed data value '{data}'. Writing into db...");
                    await Task.Delay(2000);
                    Console.WriteLine($" Write complete!");
                }
                else
                {
                    break;
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
