using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashboardServer.Game
{
    abstract class AbstractGame
    {
        public class Data
        {

        }

        public abstract void Run();

        public abstract Boolean Update();

        public abstract ExchangeData GetData();
    }
}
