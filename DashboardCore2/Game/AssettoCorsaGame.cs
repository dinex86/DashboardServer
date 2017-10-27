using AssettoCorsaSharedMemory;
using System;

namespace DashboardCore.Game
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
            ac.StaticInfoInterval = 1000; // Get StaticInfo updates ever 1 seconds
            ac.PhysicsInterval = 50; // Getting physics every 50 ms should be okay.
            ac.GraphicsInterval = 50;
            ac.Start();
        }

        public override void Start()
        {
            Console.WriteLine("Starting collector for Assetto Corsa!");

            // Initial read.
            UpdateExchangeData(ac.ReadStaticInfo());
            UpdateExchangeData(ac.ReadGraphics());
            UpdateExchangeData(ac.ReadPhysics());

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
            UpdateExchangeData(e.Graphics);
        }
        
        private void StaticInfoUpdated(object sender, StaticInfoEventArgs e)
        {
            UpdateExchangeData(e.StaticInfo);
        }

        private void PhysicsUpdated(object sender, PhysicsEventArgs e)
        {
            UpdateExchangeData(e.Physics);
        }

        private void UpdateExchangeData(Graphics graphics)
        {
            if (graphics.CurrentSectorIndex < data.CurrentSector)
            {
                data.TriggerFuelCalculation();

                data.LastLapValid = data.CurrentLapValid;
                data.CurrentLapValid = 1; // New lap is always valid.
            }

            data.CompletedLaps = graphics.CompletedLaps;
            data.NumberOfLaps = graphics.NumberOfLaps;
            data.Position = graphics.Position;

            data.LapTimeCurrentSelf = Math.Round(graphics.iCurrentTime / 1000.0, 3);
            data.LapTimePreviousSelf = Math.Round(graphics.iLastTime / 1000.0, 3);
            data.LapTimeBestSelf = Math.Round(graphics.iBestTime / 1000.0, 3);

            data.SessionTimeRemaining = Math.Round(graphics.SessionTimeLeft / 1000.0, 3);

            switch (graphics.Session)
            {
                case AC_SESSION_TYPE.AC_PRACTICE:
                    data.Session = (int)ExchangeData.SessionIndex.PRACTICE;
                    break;
                case AC_SESSION_TYPE.AC_QUALIFY:
                    data.Session = (int)ExchangeData.SessionIndex.QUALIFY;
                    break;
                case AC_SESSION_TYPE.AC_RACE:
                    data.Session = (int)ExchangeData.SessionIndex.RACE;
                    break;
                default:
                    data.Session = (int)ExchangeData.SessionIndex.UNKNOWN;
                    break;
            }
            
            data.CurrentSector = graphics.CurrentSectorIndex;
            data.PitLimiter = graphics.IsInPitLane; // AC does not have a separate pit limiter, but there is an automatic one in the pit lane.
            data.InPitLane = graphics.IsInPitLane;

            // Timestamps.
            if (graphics.IsInPitLane == 1)
            {
                data.CurrentLapValid = 1;

                data.LastTimeInPit = Now;

                if (data.LastTimeOnTrack <= 0)
                {
                    data.LastTimeOnTrack = Now;
                }
            }
            else
            {
                data.LastTimeOnTrack = Now;

                if (data.LastTimeInPit <= 0)
                {
                    data.LastTimeInPit = Now;
                }
            }
            
            // Flags.
            switch (graphics.Flag)
            {
                case AC_FLAG_TYPE.AC_YELLOW_FLAG:
                    data.CurrentFlag = (int) ExchangeData.FlagIndex.YELLOW;
                    break;
                case AC_FLAG_TYPE.AC_BLACK_FLAG:
                    data.CurrentFlag = (int) ExchangeData.FlagIndex.BLACK;
                    break;
                case AC_FLAG_TYPE.AC_BLUE_FLAG:
                    data.CurrentFlag = (int) ExchangeData.FlagIndex.BLUE;
                    break;
                case AC_FLAG_TYPE.AC_CHECKERED_FLAG:
                    data.CurrentFlag = (int) ExchangeData.FlagIndex.CHECKERED;
                    break;
                case AC_FLAG_TYPE.AC_NO_FLAG:
                    data.CurrentFlag = (int)ExchangeData.FlagIndex.NO_FLAG;
                    break;
                case AC_FLAG_TYPE.AC_WHITE_FLAG:
                    data.CurrentFlag = (int) ExchangeData.FlagIndex.WHITE;
                    break;
                case AC_FLAG_TYPE.AC_PENALTY_FLAG:
                    data.CurrentFlag = (int) ExchangeData.FlagIndex.PENALTY;
                    break;
            }
        }

        private void UpdateExchangeData(StaticInfo staticInfo)
        {
            if (staticInfo.IsTimedRace > 0 && staticInfo.HasExtraLap > 0)
            {
                data.RaceFormat = (int)ExchangeData.RaceFormatIndex.TIME_AND_EXTRA_LAP;
            }
            else if (staticInfo.IsTimedRace > 0)
            {
                data.RaceFormat = (int)ExchangeData.RaceFormatIndex.TIME;
            }
            else
            {
                data.RaceFormat = (int)ExchangeData.RaceFormatIndex.LAP;
            }

            if (staticInfo.MaxRpm > 0)
            {
                data.MaxEngineRpm = staticInfo.MaxRpm;
            }

            data.FuelMax = staticInfo.MaxFuel;
            data.NumCars = staticInfo.NumCars;

            data.DrsEquipped = staticInfo.HasDRS;
            data.ErsEquipped = staticInfo.HasERS;
            data.KersEquipped = staticInfo.HasKERS;
        }

        private void UpdateExchangeData(Physics physics)
        {
            // Basic.
            data.CarSpeed = (int) Math.Floor(physics.SpeedKmh);
            data.Gear = physics.Gear - 1;
            data.EngineRpm = physics.Rpms;

            if (physics.Rpms > data.MaxEngineRpm)
            {
                data.MaxEngineRpm = physics.Rpms;
            }

            data.FuelLeft = physics.Fuel;
            data.DrsAvailable = physics.DrsAvailable;
            data.DrsEngaged = physics.DrsEnabled;
            data.BreakBias = physics.BrakeBias;
            //data.PitLimiter = physics.PitLimiterOn; // doesn't work, only 1 when speed = speed limit
            data.TractionControl = physics.TC;
            data.Abs = physics.Abs;

            // Lap time valid?
             data.CurrentLapValid = physics.NumberOfTyresOut < 3 ? data.CurrentLapValid : 0;

            // Break temps.
            data.BreakTempFrontLeft = Math.Round(physics.BrakeTemp[0], 1);
            data.BreakTempFrontRight = Math.Round(physics.BrakeTemp[1], 1);
            data.BreakTempRearLeft = Math.Round(physics.BrakeTemp[2], 1);
            data.BreakTempRearRight = Math.Round(physics.BrakeTemp[3], 1);
            
            // Tire temps.
            data.TireTempFrontLeft = Math.Round(physics.TyreCoreTemperature[0], 1);
            data.TireTempFrontRight = Math.Round(physics.TyreCoreTemperature[1], 1);
            data.TireTempRearLeft = Math.Round(physics.TyreCoreTemperature[2], 1);
            data.TireTempRearRight = Math.Round(physics.TyreCoreTemperature[3], 1);

            // Tire wear.
            data.TireWearFrontLeft = ConvertTireWear(physics.TyreWear[0]);
            data.TireWearFrontRight = ConvertTireWear(physics.TyreWear[1]);
            data.TireWearRearLeft = ConvertTireWear(physics.TyreWear[2]);
            data.TireWearRearRight = ConvertTireWear(physics.TyreWear[3]);

            // Tire pressure.
            data.TirePressureFrontLeft = Math.Round(physics.WheelsPressure[0], 1);
            data.TirePressureFrontRight = Math.Round(physics.WheelsPressure[1], 1);
            data.TirePressureRearLeft = Math.Round(physics.WheelsPressure[2], 1);
            data.TirePressureRearRight = Math.Round(physics.WheelsPressure[3], 1);

            // Tire dirt. Max value in game is 5.0.
            data.TireDirtFrontLeft = Math.Round(physics.TyreDirtyLevel[0] / 5, 3);
            data.TireDirtFrontRight = Math.Round(physics.TyreDirtyLevel[1] / 5, 3);
            data.TireDirtRearLeft= Math.Round(physics.TyreDirtyLevel[2] / 5, 3);
            data.TireDirtRearRight = Math.Round(physics.TyreDirtyLevel[3] / 5, 3);

            // Temperatures.
            data.AirTemperature = physics.AirTemp;
            data.TrackTemperature = physics.RoadTemp;

            // Delta.
            data.DeltaBestSelf = physics.PerformanceMeter;

            // Car damage.
            // 0 + 4 => tires front?
            // 1 => ?
            // 2 + 3 => tires rear?
            //Console.WriteLine(physics.CarDamage);

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
