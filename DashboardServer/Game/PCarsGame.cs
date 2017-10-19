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
            pCarsAPIParticipantStruct player = shared.mParticipantData[shared.mViewedParticipantIndex];

            // ControlType 0 = Player.
            bool isPlayer = true;
            bool isDriving = (int)shared.mPitMode != (int)ePitMode.PIT_MODE_IN_GARAGE;
            bool inPit = (int)shared.mPitMode == (int)ePitMode.PIT_MODE_DRIVING_INTO_PITS || (int)shared.mPitMode == (int)ePitMode.PIT_MODE_IN_PIT || (int)shared.mPitMode == (int)ePitMode.PIT_MODE_DRIVING_OUT_OF_PITS;
            
            // Add 5% to lap distance fraction to prevent from triggering while spinning or after a crash.
            bool newLapStart = isPlayer && player.mCurrentLapDistance < player.mCurrentLapDistance - 0.05 && isDriving && shared.mCurrentTime > 0 && shared.mCurrentTime < data.LapTimeCurrentSelf;

            // Session info.
            /*switch ((int)shared.mRaceState)
            {
                case eSessionState.:
                data.RaceFormat = (int)ExchangeData.RaceFormatIndex.LAP;
                break;
                case 1:
                data.RaceFormat = (int)ExchangeData.RaceFormatIndex.TIME;
                break;
                case 2:
                data.RaceFormat = (int)ExchangeData.RaceFormatIndex.TIME_AND_EXTRA_LAP;
                break;
            }*/

            switch ((int)shared.mSessionState)
            {
                case (int)eSessionState.SESSION_PRACTICE:
                case (int)eSessionState.SESSION_TEST:
                case (int)eSessionState.SESSION_TIME_ATTACK:
                    data.Session = (int)ExchangeData.SessionIndex.PRACTICE;
                    break;
                case (int)eSessionState.SESSION_QUALIFY:
                    data.Session = (int)ExchangeData.SessionIndex.QUALIFY;
                    break;
                case (int)eSessionState.SESSION_FORMATIONLAP:
                case (int)eSessionState.SESSION_RACE:
                    data.Session = (int)ExchangeData.SessionIndex.RACE;
                    break;
            }

            data.SessionIteration = -1; // shared.SessionIteration;
            
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
            }

            // General.
            data.CarSpeed = (int)Math.Round(shared.mSpeed);
            data.Gear = shared.mGear;
            data.MaxEngineRpm = (int)Math.Round(shared.mMaxRPM);
            data.EngineRpm = (int)Math.Round(shared.mRPM);
            data.FuelLeft = shared.mFuelLevel * shared.mFuelCapacity;
            data.NumberOfLaps = (shared.mLapsInEvent == 0) ? (int)shared.mLapsInEvent : -1;
            data.CompletedLaps = (int)player.mLapsCompleted;
            data.Position = (int)player.mRacePosition;
            data.NumCars = shared.mNumParticipants;
            data.PitLimiter = -1; // (int)shared.mCarFlags[(int)eCarFlags.CAR_SPEED_LIMITER];
            data.InPitLane = inPit ? 1 : 0; // shared.mPitMode;
            data.BreakBias = -1;// player.
            data.LapDistanceFraction = player.mCurrentLapDistance / shared.mTrackLength;
            data.TractionControl = -1;
            data.Abs = -1;
            
            /*
            // DRS.
            data.DrsAvailable = shared.drs
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
            */

            // Temps.
            data.OilTemperature = shared.mOilTempCelsius;
            data.WaterTemperature = shared.mWaterTempCelsius;
            data.TrackTemperature = shared.mTrackTemperature;
            data.AirTemperature = shared.mAmbientTemperature;

            // Break temps.
            data.BreakTempFrontLeft = Math.Round(shared.mBrakeTempCelsius[(int)eTyres.TYRE_FRONT_LEFT], 1);
            data.BreakTempFrontRight = Math.Round(shared.mBrakeTempCelsius[(int)eTyres.TYRE_FRONT_RIGHT], 1);
            data.BreakTempRearLeft = Math.Round(shared.mBrakeTempCelsius[(int)eTyres.TYRE_REAR_LEFT], 1);
            data.BreakTempRearRight = Math.Round(shared.mBrakeTempCelsius[(int)eTyres.TYRE_REAR_RIGHT], 1);
            
            // Damage.
            /*
            data.DamageAerodynamics = shared.CarDamage.Aerodynamics;
            data.DamageEngine = shared.CarDamage.Engine;
            data.DamageTransmission = shared.CarDamage.Transmission;
            */

            // Tire temps.
            data.TireTempFrontLeft = Math.Round(shared.mTyreTemp[(int)eTyres.TYRE_FRONT_LEFT], 1);
            data.TireTempFrontRight = Math.Round(shared.mTyreTemp[(int)eTyres.TYRE_FRONT_RIGHT], 1);
            data.TireTempRearLeft = Math.Round(shared.mTyreTemp[(int)eTyres.TYRE_REAR_LEFT], 1);
            data.TireTempRearRight = Math.Round(shared.mTyreTemp[(int)eTyres.TYRE_REAR_RIGHT], 1);

            // Tire wear.
            data.TireWearFrontLeft = Math.Round(shared.mTyreWear[(int)eTyres.TYRE_FRONT_LEFT], 3);
            data.TireWearFrontRight = Math.Round(shared.mTyreWear[(int)eTyres.TYRE_FRONT_RIGHT], 3);
            data.TireWearRearLeft = Math.Round(shared.mTyreWear[(int)eTyres.TYRE_REAR_LEFT], 3);
            data.TireWearRearRight = Math.Round(shared.mTyreWear[(int)eTyres.TYRE_REAR_RIGHT], 3);

            // Tire pressure.
            /*
            data.TirePressureFrontLeft = Math.Round(ConvertPressureToPsi(shared.tyre[(int)eTyres.TYRE_FRONT_LEFT]), 1);
            data.TirePressureFrontRight = Math.Round(ConvertPressureToPsi(shared.TirePressure[(int)eTyres.TYRE_FRONT_RIGHT]), 1);
            data.TirePressureRearLeft = Math.Round(ConvertPressureToPsi(shared.TirePressure[(int)eTyres.TYRE_REAR_LEFT]), 1);
            data.TirePressureRearRight = Math.Round(ConvertPressureToPsi(shared.TirePressure[(int)eTyres.TYRE_REAR_RIGHT]), 1);
            */

            // Tire dirt.
            /*
            data.TireDirtFrontLeft = Math.Round(shared.[(int)eTyres.TYRE_FRONT_LEFT], 1);
            data.TireDirtFrontRight = Math.Round(shared.TireDirt[(int)eTyres.TYRE_FRONT_RIGHT], 1);
            data.TireDirtRearLeft = Math.Round(shared.TireDirt[(int)eTyres.TYRE_REAR_LEFT], 1);
            data.TireDirtRearRight = Math.Round(shared.TireDirt[(int)eTyres.TYRE_REAR_RIGHT], 1);
            */

            // Timing.
            data.CurrentLapValid = shared.mLapInvalidated ? 0 : 1;
            //data.LapTimeCurrentSelf = Math.Round(shared.CurrentLapValid > 0 ? shared.LapTimeCurrentSelf : (data.LastLapStart > 0 ? ((isDriving ? Now : data.LastTimeOnTrack) - data.LastLapStart) / 1000.0 : -1), 3);
            data.LapTimeCurrentSelf = shared.mCurrentTime;
            data.LapTimePreviousSelf = shared.mLastLapTime;
            /*if (shared.LapTimePreviousSelf > 0)
            {
                if (data.LastLapValid == 0)
                {
                    Console.WriteLine("set last valid");
                    data.LastLapValid = 1;
                }
                data.LapTimePreviousSelf = Math.Round(shared.LapTimePreviousSelf, 3);
            }*/
            
            data.LapTimeBestSelf = Math.Round(shared.mPersonalFastestLapTime, 3);
            data.LapTimeBestLeader = Math.Round(shared.mSessionFastestLapTime, 3);
            data.LapTimeBestLeaderClass = Math.Round(shared.mSessionFastestLapTime, 3);
            data.SessionTimeRemaining = Math.Round(shared.mEventTimeRemaining, 3);
            data.LapTimeDeltaLeader = -1;// Math.Round(shared.LapTimeDeltaLeader, 3);
            data.LapTimeDeltaLeaderClass = -1;//Math.Round(shared.LapTimeDeltaLeaderClass, 3);
            
            // Delta.
            data.TimeDeltaBehind = Math.Round(shared.mSplitTimeBehind, 3);
            data.TimeDeltaFront = Math.Round(shared.mSplitTimeAhead, 3);

            data.DeltaBestSelf = Math.Round(shared.mSplitTime, 3);
            data.DeltaBestSession = -1;//CalcSectorDiff(shared.SectorTimesCurrentSelf, shared.SectorTimesPreviousSelf, shared.SectorTimesSessionBestLap);

            data.CurrentSector = (int)player.mCurrentSector;

            // Flags.
            data.YellowFlagAhead = false;
            data.YellowSector1 = -1;
            data.YellowSector2 = -1;
            data.YellowSector3 = -1;
            
            if ((int)shared.mHighestFlagColour == (int)eFlagColors.FLAG_COLOUR_GREEN)
            {
                data.CurrentFlag = (int)ExchangeData.FlagIndex.GREEN;
            }
            else if ((int)shared.mHighestFlagColour == (int)eFlagColors.FLAG_COLOUR_YELLOW || (int)shared.mHighestFlagColour == (int)eFlagColors.FLAG_COLOUR_DOUBLE_YELLOW)
            {
                data.CurrentFlag = (int)ExchangeData.FlagIndex.YELLOW;
            }
            else if ((int)shared.mHighestFlagColour == (int)eFlagColors.FLAG_COLOUR_WHITE)
            {
                data.CurrentFlag = (int)ExchangeData.FlagIndex.WHITE;
            }
            else if ((int)shared.mHighestFlagColour == (int)eFlagColors.FLAG_COLOUR_BLACK)
            {
                data.CurrentFlag = (int)ExchangeData.FlagIndex.BLACK;
            }
            else if ((int)shared.mHighestFlagColour == (int)eFlagColors.FLAG_COLOUR_BLUE)
            {
                data.CurrentFlag = (int)ExchangeData.FlagIndex.BLUE;
            }
            else if ((int)shared.mHighestFlagColour == (int)eFlagColors.FLAG_COLOUR_CHEQUERED)
            {
                data.CurrentFlag = (int)ExchangeData.FlagIndex.CHECKERED;
            }
            else if ((int)shared.mHighestFlagColour == (int)eFlagColors.FLAG_COLOUR_WHITE)
            {
                data.CurrentFlag = (int)ExchangeData.FlagIndex.BLACK_WHITE;
            }
            else
            {
                data.CurrentFlag = (int)ExchangeData.FlagIndex.NO_FLAG;
            }

            // Timestamps.
            /*
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
