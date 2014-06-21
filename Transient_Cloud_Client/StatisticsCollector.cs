using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace Transient_Cloud_Client
{
    class StatisticsCollector
    {
        private ConcurrentQueue<Event> events;
        private Dictionary<File, long> fileList = new Dictionary<File, long>();
        public StatisticsCollector(ConcurrentQueue<Event> events)
        {
            this.events = events;
        }
        public void Collect()
        {
            Event current;
            while (true)
            {
                events.TryDequeue(out current);
                if (current != null)
                    Console.WriteLine("An {0} event was fired on {1}", current.Action, current.FileName);
            }
        }
    }
}
