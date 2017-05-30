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
            data.PitLimiter = e.Graphics.IsInPitLane; // AC does not have a separate pit limiter, but there is an automatic one in the pit lane.
            data.InPitLane = e.Graphics.IsInPitLane;

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

            Update?.Invoke(data);
        }

        private void StaticInfoUpdated(object sender, StaticInfoEventArgs e)
        {
            data.MaxEngineRpm = e.StaticInfo.MaxRpm;
            data.FuelMax = e.StaticInfo.MaxFuel;
            data.NumCars = e.StaticInfo.NumCars;

            data.DrsEquipped = e.StaticInfo.HasDRS;
            data.ErsEquipped = e.StaticInfo.HasERS;
            data.KersEquipped = e.StaticInfo.HasKERS;
            
            Update?.Invoke(data);
        }

        private void PhysicsUpdated(object sender, PhysicsEventArgs e)
        {
            // Basic.
            data.CarSpeed = (int) Math.Floor(e.Physics.SpeedKmh);
            data.Gear = e.Physics.Gear - 1;
            data.EngineRpm = e.Physics.Rpms;
            data.FuelLeft = e.Physics.Fuel;
            data.DrsAvailable = e.Physics.DrsAvailable;
            data.DrsEngaged = e.Physics.DrsEnabled;
            //data.PitLimiter = e.Physics.PitLimiterOn; // doesn't work, only 1 when speed = speed limit

            // Tire temps.
            data.TireTempFrontLeft = Math.Round(e.Physics.TyreCoreTemperature[0], 1);
            data.TireTempFrontRight = Math.Round(e.Physics.TyreCoreTemperature[1], 1);
            data.TireTempRearLeft = Math.Round(e.Physics.TyreCoreTemperature[2], 1);
            data.TireTempRearRight = Math.Round(e.Physics.TyreCoreTemperature[3], 1);

            // Tire wear.
            data.TireWearFrontLeft = ConvertTireWear(e.Physics.TyreWear[0]);
            data.TireWearFrontRight = ConvertTireWear(e.Physics.TyreWear[1]);
            data.TireWearRearLeft = ConvertTireWear(e.Physics.TyreWear[2]);
            data.TireWearRearRight = ConvertTireWear(e.Physics.TyreWear[3]);

            // Tire pressure.
            data.TirePressureFrontLeft = Math.Round(e.Physics.WheelsPressure[0], 1);
            data.TirePressureFrontRight = Math.Round(e.Physics.WheelsPressure[1], 1);
            data.TirePressureRearLeft = Math.Round(e.Physics.WheelsPressure[2], 1);
            data.TirePressureRearRight = Math.Round(e.Physics.WheelsPressure[3], 1);

            // Tire dirt. Max value in game is 5.0.
            data.TireDirtFrontLeft = Math.Round(e.Physics.TyreDirtyLevel[0] / 5, 3);
            data.TireDirtFrontRight = Math.Round(e.Physics.TyreDirtyLevel[1] / 5, 3);
            data.TireDirtRearLeft= Math.Round(e.Physics.TyreDirtyLevel[2] / 5, 3);
            data.TireDirtRearRight = Math.Round(e.Physics.TyreDirtyLevel[3] / 5, 3);

            // Temperatures.
            data.AirTemperature = e.Physics.AirTemp;
            data.TrackTemperature = e.Physics.RoadTemp;

            // Delta.
            data.DeltaBestSelf = e.Physics.PerformanceMeter;

            Update?.Invoke(data);
        }

        private double ConvertTireWear(float wear)
        {
            double baseValue = 93;
            if (wear < baseValue)
            {
                return 0;
            }

            return Math.Round(((wear - baseValue) / (100 - baseValue)), 3);
        }
    }
}
