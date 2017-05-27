using AssettoCorsaSharedMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DashboardServer.Game
{
    class AssettoCorsaGame : AbstractGame
    {
        public override event UpdateEventHandler Update;

        private ExchangeData data = new ExchangeData();
        private AssettoCorsa ac = new AssettoCorsa();

        public override bool IsRunning
        {
            get
            {
                if (ac == null)
                {
                    return false;
                }
                
                return ac.IsRunning;
            }
        }

        public AssettoCorsaGame()
        {
            ac.StaticInfoInterval = 5000; // Get StaticInfo updates ever 5 seconds
            ac.PhysicsInterval = 50; // Getting physics every 50 ms should be okay.
            ac.GraphicsInterval = 50;
            ac.Start();
        }

        public override void Start()
        {
            Console.WriteLine("Starting collector for Assetto Corsa!");

            // Add the listeners.
            ac.GameStatusChanged += GameStatusChanged;
            ac.StaticInfoUpdated += StaticInfoUpdated;
            ac.PhysicsUpdated += PhysicsUpdated;
            ac.GraphicsUpdated += GraphicsInfoUpdated;
        }

        public override void Stop()
        {
            Console.WriteLine("Stopping collector for Assetto Corsa!");

            // Remove listeners, but keep the assetto code running as it looks for the game.
            ac.GameStatusChanged -= GameStatusChanged;
            ac.StaticInfoUpdated -= StaticInfoUpdated;
            ac.PhysicsUpdated -= PhysicsUpdated;
            ac.GraphicsUpdated -= GraphicsInfoUpdated;
        }

        private void GameStatusChanged(object sender, GameStatusEventArgs e)
        {
            // Wrapper to avoid null pointer exception in the AC code. They forgot a null check for this event.
        }

        private void GraphicsInfoUpdated(object sender, GraphicsEventArgs e)
        {
            if (Update == null)
            {
                return;
            }

            data.CompletedLaps = e.Graphics.CompletedLaps;
            data.NumberOfLaps = e.Graphics.NumberOfLaps;
            data.Position = e.Graphics.Position;

            data.LapTimeCurrentSelf = Math.Round(e.Graphics.iCurrentTime / 1000.0, 3);
            data.LapTimePreviousSelf = Math.Round(e.Graphics.iLastTime / 1000.0, 3);
            data.LapTimeBestSelf = Math.Round(e.Graphics.iBestTime / 1000.0, 3);
            data.SessionTimeRemaining = Math.Round(e.Graphics.SessionTimeLeft / 1000.0, 3);
            data.CurrentSector = e.Graphics.CurrentSectorIndex;
            
            Update(data);
        }

        private void StaticInfoUpdated(object sender, StaticInfoEventArgs e)
        {
            if (Update == null)
            {
                return;
            }

            data.MaxEngineRpm = e.StaticInfo.MaxRpm;
            data.FuelMax = e.StaticInfo.MaxFuel;
            data.NumCars = e.StaticInfo.NumCars;
            data.DrsEquipped = e.StaticInfo.HasDRS;

            Update(data);
        }

        private void PhysicsUpdated(object sender, PhysicsEventArgs e)
        {
            if (Update == null)
            {
                return;
            }
                        
            data.CarSpeed = (int) Math.Floor(e.Physics.SpeedKmh);
            data.Gear = e.Physics.Gear - 1;
            data.EngineRpm = e.Physics.Rpms;
            data.FuelLeft = e.Physics.Fuel;
            data.DrsAvailable = e.Physics.DrsAvailable;
            data.DrsEngaged = e.Physics.DrsEnabled;
            data.PitLimiter = e.Physics.PitLimiterOn;
            data.AirTemperature = e.Physics.AirTemp;
            data.TrackTemperature = e.Physics.RoadTemp;
            data.Delta = e.Physics.PerformanceMeter;
            
            Update(data);
        }
    }
}
