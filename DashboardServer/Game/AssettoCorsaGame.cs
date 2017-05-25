using AssettoCorsaSharedMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashboardServer.Game
{
    class AssettoCorsaGame : AbstractGame
    {
        private StaticInfo staticInfo;

        public override void Run()
        {
            AssettoCorsa ac = new AssettoCorsa();
            ac.StaticInfoInterval = 5000; // Get StaticInfo updates ever 5 seconds
            ac.StaticInfoUpdated += ac_StaticInfoUpdated; // Add event listener for StaticInfo
            ac.PhysicsInterval = 50;
            //ac.
            ac.Start(); // Connect to shared memory and start interval timers 
        }

        static void ac_StaticInfoUpdated(object sender, StaticInfoEventArgs e)
        {
            // Print out some data from StaticInfo
            Console.WriteLine("StaticInfo");
            Console.WriteLine("  Car Model: " + e.StaticInfo.CarModel);
            Console.WriteLine("  Track:     " + e.StaticInfo.Track);
            Console.WriteLine("  Max RPM:   " + e.StaticInfo.MaxRpm);
        }

        public override ExchangeData GetData()
        {
            return null;
            //return new ExchangeData(data);
        }

        public override bool Update()
        {
            return true;
        }
    }
}
