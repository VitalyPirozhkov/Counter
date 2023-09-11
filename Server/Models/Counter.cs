using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Models
{
    public class Counter
    {
        private int count;
        private bool isRunning;
        private object lockObject = new object();
        private Thread updateThread;

        public Counter()
        {
            count = 0;
            isRunning = false;
            updateThread = null;
        }

        public int GetCount()
        {
            Console.WriteLine("Get count");
            return count;
        }

        public void Start()
        {
            lock (lockObject)
            {
                if (!isRunning)
                {
                    Console.WriteLine("Start");
                    isRunning = true;
                    updateThread = new Thread(UpdateCount);
                    updateThread.Start();
                }
            }
        }

        public void Stop()
        {
            lock (lockObject)
            {
                if (isRunning)
                {
                    Console.WriteLine("Stop");
                    isRunning = false;
                    updateThread.Abort();
                }
            }
        }

        public void Continue()
        {
            lock (lockObject)
            {
                if (!isRunning)
                {
                    Console.WriteLine("Continue");
                    isRunning = true;
                    updateThread = new Thread(UpdateCount);
                    updateThread.Start();
                }
            }
        }


        public void Reset()
        {
            lock (lockObject)
            {
                Console.WriteLine("Reset");
                count = 0;
            }
        }

        private void UpdateCount()
        {
            while (isRunning)
            {
                Thread.Sleep(1000);
                lock (lockObject)
                {
                    count++;
                    Console.WriteLine($"Current value: {count}");
                }
            }
        }
    }


}
