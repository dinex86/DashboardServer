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

            data = new ExchangeData();
        }

        private void GameStatusChanged(object sender, GameStatusEventArgs e)
        {
            if (e.GameStatus == AC_STATUS.AC_OFF)
            {
                data = new ExchangeData();
            }
        }

        private void GraphicsInfoUpdated(object sender, GraphicsEventArgs e)
        {
            if (Update == null)
            {
                return;
            }

            if (e.Graphics.CurrentSectorIndex < data.CurrentSector)
            {
                data.TriggerFuelCalculation();
            }

            data.CompletedLaps = e.Graphics.CompletedLaps;
            data.NumberOfLaps = e.Graphics.NumberOfLaps;
            data.Position = e.Graphics.Position;

            data.LapTimeCurrentSelf = Math.Round(e.Graphics.iCurrentTime / 1000.0, 3);
            data.LapTimePreviousSelf = Math.Round(e.Graphics.iLastTime / 1000.0, 3);
            data.LapTimeBestSelf = Math.Round(e.Graphics.iBestTime / 1000.0, 3);
            data.SessionTimeRemaining = Math.Round(e.Graphics.SessionTimeLeft / 1000.0, 3);
            data.CurrentSector = e.Graphics.CurrentSectorIndex;
            data.PitLimiter = e.Graphics.IsInPitLane;
            // Flags.
            switch (e.Graphics.Flag)
            {
                case AC_FLAG_TYPE.AC_YELLOW_FLAG:
                    data.CurrentFlag = (int)ExchangeData.FlagIndex.YELLOW;
                    break;
                case AC_FLAG_TYPE.AC_BLACK_FLAG:
                    data.CurrentFlag = (int)ExchangeData.FlagIndex.BLACK;
                    break;
                case AC_FLAG_TYPE.AC_BLUE_FLAG:
                    data.CurrentFlag = (int)ExchangeData.FlagIndex.BLUE;
                    break;
                case AC_FLAG_TYPE.AC_CHECKERED_FLAG:
                    data.CurrentFlag = (int)ExchangeData.FlagIndex.CHECKERED;
                    break;
                case AC_FLAG_TYPE.AC_NO_FLAG:
                    data.CurrentFlag = (int)ExchangeData.FlagIndex.GREEN;
                    break;
                case AC_FLAG_TYPE.AC_WHITE_FLAG:
                    data.CurrentFlag = (int)ExchangeData.FlagIndex.WHITE;
                    break;
                case AC_FLAG_TYPE.AC_PENALTY_FLAG:
                    data.CurrentFlag = (int)ExchangeData.FlagIndex.PENALTY;
                    break;
            }
            
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

            data.ErsEquipped = e.StaticInfo.HasERS;
            data.KersEquipped = e.StaticInfo.HasKERS;

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
            //data.PitLimiter = e.Physics.PitLimiterOn; // doesn't work, only 1 when speed = speed limit

            data.AirTemperature = e.Physics.AirTemp;
            data.TrackTemperature = e.Physics.RoadTemp;

            data.DeltaBestSelf = e.Physics.PerformanceMeter;
            
            Update(data);
        }
    }
}
