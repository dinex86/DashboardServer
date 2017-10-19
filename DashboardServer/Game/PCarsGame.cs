using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using pCarsAPI_Demo;

namespace DashboardServer.Game
{
    class PCarsGame : MemoryMappedGame<pCarsAPIStruct>
    {
        public override bool IsRunning
        {
            get
            {
                return true;
            }
        }

        protected override string SharedMemoryName
        {
            get
            {
                return "$pcars$";
            }
        }

        protected override void UpdateExchangeData(pCarsAPIStruct shared)
        {
            // ControlType 0 = Player.
           /* bool isPlayer = shared.ControlType == 0;
            bool isDriving = shared.GameInMenus != 1 && shared.GamePaused != 1;

            // Add 5% to lap distance fraction to prevent from triggering while spinning or after a crash.
            bool newLapStart = isPlayer && shared.LapDistanceFraction < data.LapDistanceFraction - 0.05 && isDriving && shared.LapTimeCurrentSelf > 0 && shared.LapTimeCurrentSelf < data.LapTimeCurrentSelf;

            // Session info.
            switch (shared.SessionLengthFormat)
            {
                case 0:
                    data.RaceFormat = (int)ExchangeData.RaceFormatIndex.LAP;
                    break;
                case 1:
                    data.RaceFormat = (int)ExchangeData.RaceFormatIndex.TIME;
                    break;
                case 2:
                    data.RaceFormat = (int)ExchangeData.RaceFormatIndex.TIME_AND_EXTRA_LAP;
                    break;
            }

            switch (shared.SessionType)
            {
                case 0:
                    data.Session = (int)ExchangeData.SessionIndex.PRACTICE;
                    break;
                case 1:
                    data.Session = (int)ExchangeData.SessionIndex.QUALIFY;
                    break;
                case 2:
                    data.Session = (int)ExchangeData.SessionIndex.RACE;
                    break;
            }

            data.SessionIteration = shared.SessionIteration;


            if (newLapStart)
            {

                Console.WriteLine("-- NEW LAP DETECTED --");

                double currentLapTimeInMillis = Math.Round(shared.LapTimeCurrentSelf, 3) * 1000;
                double currentLapStartTimeInMillis = Math.Round(Now - currentLapTimeInMillis);
                if (data.CurrentLapValid != 1)
                {
                    data.LapTimePreviousSelf = Math.Round((currentLapStartTimeInMillis - data.LastLapStart) / 1000, 3);
                    data.LastLapValid = 0;
                    Console.WriteLine("set last insvalid");
                }

                data.LastLapStart = (long)currentLapStartTimeInMillis;
                data.TriggerFuelCalculation();
            }*/

            // General.
            data.CarSpeed = (int)Math.Round(shared.mSpeed);
            data.Gear = shared.mGear;
            data.MaxEngineRpm = (int)Math.Round(shared.mMaxRPM);
            data.EngineRpm = (int)Math.Round(shared.mRPM);
            data.FuelLeft = shared.mFuelLevel;
            data.NumberOfLaps = (shared.mLapsInEvent == 0) ? (int)shared.mLapsInEvent : -1;
            /*data.CompletedLaps = shared.;
            data.Position = shared.Position;
            data.NumCars = shared.NumCars;
            data.PitLimiter = shared.PitLimiter;
            data.InPitLane = shared.InPitlane;
            data.BreakBias = shared.BrakeBias;
            data.LapDistanceFraction = shared.LapDistanceFraction;
            data.TractionControl = -1;
            data.Abs = -1;
            */
            /*
            // DRS.
            data.DrsAvailable = shared.Drs.Available;
            data.DrsEngaged = shared.Drs.Engaged;
            data.DrsEquipped = shared.Drs.Equipped;
            data.DrsNumActivationsLeft = shared.Drs.NumActivationsLeft;

            // P2P.
            data.PushToPassEquipped = shared.PushToPass.Available;
            data.PushToPassAvailable = shared.PushToPass.Available;
            data.PushToPassEngaged = shared.PushToPass.Engaged;
            data.PushToPassNumActivationsLeft = shared.PushToPass.AmountLeft;
            data.PushToPassEngagedTimeLeft = shared.PushToPass.EngagedTimeLeft;
            data.PushToPassWaitTimeLeft = shared.PushToPass.WaitTimeLeft;

            // Temps.
            data.OilTemperature = shared.EngineOilTemp;
            data.WaterTemperature = shared.EngineWaterTemp;

            // Break temps.
            data.BreakTempFrontLeft = Math.Round(shared.BrakeTemp.FrontLeft, 1);
            data.BreakTempFrontRight = Math.Round(shared.BrakeTemp.FrontRight, 1);
            data.BreakTempRearLeft = Math.Round(shared.BrakeTemp.RearLeft, 1);
            data.BreakTempRearRight = Math.Round(shared.BrakeTemp.RearRight, 1);

            // Damage.
            data.DamageAerodynamics = shared.CarDamage.Aerodynamics;
            data.DamageEngine = shared.CarDamage.Engine;
            data.DamageTransmission = shared.CarDamage.Transmission;

            // Tire temps.
            data.TireTempFrontLeft = Math.Round(shared.TireTemp.FrontLeft_Center, 1);
            data.TireTempFrontRight = Math.Round(shared.TireTemp.FrontRight_Center, 1);
            data.TireTempRearLeft = Math.Round(shared.TireTemp.RearLeft_Center, 1);
            data.TireTempRearRight = Math.Round(shared.TireTemp.RearRight_Center, 1);

            // Tire wear.
            data.TireWearFrontLeft = Math.Round(shared.TireWear.FrontLeft, 3);
            data.TireWearFrontRight = Math.Round(shared.TireWear.FrontRight, 3);
            data.TireWearRearLeft = Math.Round(shared.TireWear.RearLeft, 3);
            data.TireWearRearRight = Math.Round(shared.TireWear.RearRight, 3);

            // Tire pressure.
            data.TirePressureFrontLeft = Math.Round(ConvertPressureToPsi(shared.TirePressure.FrontLeft), 1);
            data.TirePressureFrontRight = Math.Round(ConvertPressureToPsi(shared.TirePressure.FrontRight), 1);
            data.TirePressureRearLeft = Math.Round(ConvertPressureToPsi(shared.TirePressure.RearLeft), 1);
            data.TirePressureRearRight = Math.Round(ConvertPressureToPsi(shared.TirePressure.RearRight), 1);

            // Tire dirt.
            data.TireDirtFrontLeft = Math.Round(shared.TireDirt.FrontLeft, 1);
            data.TireDirtFrontRight = Math.Round(shared.TireDirt.FrontRight, 1);
            data.TireDirtRearLeft = Math.Round(shared.TireDirt.RearLeft, 1);
            data.TireDirtRearRight = Math.Round(shared.TireDirt.RearRight, 1);

            // Timing.
            data.CurrentLapValid = shared.CurrentLapValid;
            data.LapTimeCurrentSelf = Math.Round(shared.CurrentLapValid > 0 ? shared.LapTimeCurrentSelf : (data.LastLapStart > 0 ? ((isDriving ? Now : data.LastTimeOnTrack) - data.LastLapStart) / 1000.0 : -1), 3);

            if (shared.LapTimePreviousSelf > 0)
            {
                if (data.LastLapValid == 0)
                {
                    Console.WriteLine("set last valid");
                    data.LastLapValid = 1;
                }
                data.LapTimePreviousSelf = Math.Round(shared.LapTimePreviousSelf, 3);
            }

            data.LapTimeBestSelf = Math.Round(shared.LapTimeBestSelf, 3);
            data.LapTimeBestLeader = Math.Round(shared.LapTimeBestLeader, 3);
            data.LapTimeBestLeaderClass = Math.Round(shared.LapTimeBestLeaderClass, 3);
            data.SessionTimeRemaining = Math.Round(shared.SessionTimeRemaining, 3);
            data.LapTimeDeltaLeader = Math.Round(shared.LapTimeDeltaLeader, 3);
            data.LapTimeDeltaLeaderClass = Math.Round(shared.LapTimeDeltaLeaderClass, 3);


            // Delta.
            if (data.Session == (int)ExchangeData.SessionIndex.RACE)
            {
                data.TimeDeltaBehind = Math.Round(shared.TimeDeltaBehind, 3);
                data.TimeDeltaFront = Math.Round(shared.TimeDeltaFront, 3);
            }
            else
            {
                // Get from leaderboard.
                bool foundFront = false;
                bool foundBehind = false;
                for (int i = 0; i < shared.DriverData.Length; i++)
                {
                    DriverData otherDriver = shared.DriverData[i];
                    if (!foundFront && otherDriver.Place == data.Position - 1)
                    {
                        data.TimeDeltaFront = CalcSectorDiff(shared.SectorTimesBestSelf, otherDriver.SectorTimeBestSelf);
                        foundFront = true;
                    }

                    if (!foundBehind && otherDriver.Place == data.Position + 1)
                    {
                        data.TimeDeltaBehind = CalcSectorDiff(otherDriver.SectorTimeBestSelf, shared.SectorTimesBestSelf);
                        foundBehind = true;
                    }

                    if (foundFront && foundBehind)
                    {
                        break;
                    }
                }

                if (!foundFront)
                {
                    data.TimeDeltaFront = -1;
                }

                if (!foundBehind)
                {
                    data.TimeDeltaBehind = -1;
                }
            }

            data.DeltaBestSelf = CalcSectorDiff(shared.SectorTimesCurrentSelf, shared.SectorTimesPreviousSelf, shared.SectorTimesBestSelf);
            data.DeltaBestSession = CalcSectorDiff(shared.SectorTimesCurrentSelf, shared.SectorTimesPreviousSelf, shared.SectorTimesSessionBestLap);

            data.CurrentSector = shared.TrackSector;

            // Flags.
            data.YellowFlagAhead = shared.ClosestYellowDistanceIntoTrack > 0 && shared.ClosestYellowDistanceIntoTrack < 1000;
            data.YellowSector1 = shared.SectorYellow.Sector1;
            data.YellowSector2 = shared.SectorYellow.Sector2;
            data.YellowSector3 = shared.SectorYellow.Sector3;

            if (shared.NumPenalties > 0 || shared.ExtendedFlags2.YellowPositionsGained > 0)
            {
                data.CurrentFlag = (int)ExchangeData.FlagIndex.PENALTY;
            }
            else if (shared.ExtendedFlags.Green == 1)
            {
                data.CurrentFlag = (int)ExchangeData.FlagIndex.GREEN;
            }
            else if (shared.Flags.Yellow == 1)
            {
                data.CurrentFlag = (int)ExchangeData.FlagIndex.YELLOW;
            }
            else if (shared.ExtendedFlags2.White == 1)
            {
                data.CurrentFlag = (int)ExchangeData.FlagIndex.WHITE;
            }
            else if (shared.Flags.Black == 1)
            {
                data.CurrentFlag = (int)ExchangeData.FlagIndex.BLACK;
            }
            else if (shared.Flags.Blue == 1)
            {
                data.CurrentFlag = (int)ExchangeData.FlagIndex.BLUE;
            }
            else if (shared.ExtendedFlags.Checkered == 1)
            {
                data.CurrentFlag = (int)ExchangeData.FlagIndex.CHECKERED;
            }
            else if (shared.ExtendedFlags.BlackAndWhite > 0)
            {
                data.CurrentFlag = (int)ExchangeData.FlagIndex.BLACK_WHITE;
            }
            else
            {
                data.CurrentFlag = (int)ExchangeData.FlagIndex.NO_FLAG;
            }

            // Timestamps.
            if (shared.InPitlane == 1 || shared.GameInMenus == 1)
            {
                data.LastTimeInPit = Now;

                if (shared.GameInMenus == 1 || data.LastTimeOnTrack <= 0)
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
            */
            UpdateHandler?.Invoke(data);
        }
    }
}
