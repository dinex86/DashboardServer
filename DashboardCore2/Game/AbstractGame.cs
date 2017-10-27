using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashboardCore.Game
{
    abstract class AbstractGame
    {
        private DateTime _epoch = new DateTime(1970, 1, 1);

        protected ExchangeData data = new ExchangeData();
        protected long Now
        {
            get
            {
                return (long)DateTime.UtcNow.Subtract(_epoch).TotalMilliseconds;
            }
        }

        public delegate void UpdateEventHandler(ExchangeData data);

        public abstract event UpdateEventHandler Update;

        public abstract bool IsRunning { get; }

        public abstract void Start();

        public abstract void Stop();
    }
}
