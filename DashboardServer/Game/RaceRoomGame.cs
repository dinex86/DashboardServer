using R3E;
using R3E.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DashboardServer.Game
{
    class RaceRoomGame : AbstractGame
    {
        public override event UpdateEventHandler Update;
        
        private Thread updateThread = null;

        public override bool IsRunning
        {
            get
            {
                return Utilities.IsRrreRunning();
            }
        }

        public override void Start()
        {
            Console.WriteLine("Starting collector for RaceRoom Racing Experience!");

            updateThread = new Thread(new ThreadStart(Run));
            updateThread.Start();
        }

        public override void Stop()
        {
            Console.WriteLine("Starting collector for RaceRoom Racing Experience!");

            if (updateThread != null)
            {
                updateThread.Abort();
            }
        }

        private void Run()
        {
            while (IsRunning)
            {
                MemoryMappedFile file = null;

                try
                {
                    Console.WriteLine("Mapping R3E shared memory...");

                    byte[] buffer = null;
                    while (file == null)
                    {
                        try
                        {
                            file = MemoryMappedFile.OpenExisting(Constant.SharedMemoryName);
                            buffer = new Byte[Marshal.SizeOf(typeof(Shared))];

                            Console.WriteLine("Memory mapped successfully.");
                        }
                        catch (FileNotFoundException)
                        {
                            // Game not ready, wait a little bit.
                            Thread.Sleep(1000);
                            continue;
                        }
                    }

                    while (true)
                    {
                        if (Update != null)
                        {
                            var view = file.CreateViewStream();
                            BinaryReader stream = new BinaryReader(view);
                            buffer = stream.ReadBytes(Marshal.SizeOf(typeof(Shared)));
                            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

                            Shared data = (Shared)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Shared));
                            handle.Free();
                            stream.Close();
                            view.Close();

                            UpdateExchangeData(data);
                        }

                        Thread.Sleep(50);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception occured! " + e.Message);
                }
                finally
                {
                    if (file != null)
                    {
                        file.Dispose();
                        file = null;
                    }
                }
            }

            Console.WriteLine("All done with R3E!");
        }

        private void UpdateExchangeData(Shared shared)
        {
            if (shared.EngineRps > -1.0f)
            {
                //  RPM = Math.Round(Utilities.RpsToRpm(data.EngineRps));
            }

            if (shared.TrackSector < data.CurrentSector)
            {
                data.TriggerFuelCalculation();
            }

            // General.
            data.CarSpeed = (int)Math.Round(Utilities.MpsToKph(shared.CarSpeed));
            data.Gear = shared.Gear;
            data.MaxEngineRpm = (int)Math.Round(Utilities.RpsToRpm(shared.MaxEngineRps));
            data.EngineRpm = (int)Math.Round(Utilities.RpsToRpm(shared.EngineRps));

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

            // Misc.
            data.FuelLeft = shared.FuelLeft;
            data.NumberOfLaps = (shared.SessionLengthFormat == 0) ? shared.NumberOfLaps : -1;
            data.CompletedLaps = shared.CompletedLaps;
            data.Position = shared.Position;
            data.NumCars = shared.NumCars;
            data.PitLimiter = shared.PitLimiter;
            data.InPitLane = shared.InPitlane;

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
            data.TirePressureFrontLeft = Math.Round(shared.TirePressure.FrontLeft, 1);
            data.TirePressureFrontRight = Math.Round(shared.TirePressure.FrontRight, 1);
            data.TirePressureRearLeft = Math.Round(shared.TirePressure.RearLeft, 1);
            data.TirePressureRearRight = Math.Round(shared.TirePressure.RearRight, 1);

            // Tire dirt.
            data.TireDirtFrontLeft = Math.Round(shared.TireDirt.FrontLeft, 1);
            data.TireDirtFrontRight = Math.Round(shared.TireDirt.FrontRight, 1);
            data.TireDirtRearLeft = Math.Round(shared.TireDirt.RearLeft, 1);
            data.TireDirtRearRight = Math.Round(shared.TireDirt.RearRight, 1);

            // Timing.
            data.LapTimeCurrentSelf = Math.Round(shared.LapTimeCurrentSelf, 3);
            data.LapTimePreviousSelf = Math.Round(shared.LapTimePreviousSelf, 3);
            data.LapTimeBestSelf = Math.Round(shared.LapTimeBestSelf, 3);
            data.LapTimeBestLeader = Math.Round(shared.LapTimeBestLeader, 3);
            data.LapTimeBestLeaderClass = Math.Round(shared.LapTimeBestLeaderClass, 3);
            data.SessionTimeRemaining = Math.Round(shared.SessionTimeRemaining, 3);
            data.LapTimeDeltaLeader = Math.Round(shared.LapTimeDeltaLeader, 3);
            data.LapTimeDeltaLeaderClass = Math.Round(shared.LapTimeDeltaLeaderClass, 3);
            data.TimeDeltaBehind = Math.Round(shared.TimeDeltaBehind, 3);
            data.TimeDeltaFront = Math.Round(shared.TimeDeltaFront, 3);

            // Delta.
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
            
            Update?.Invoke(data);
        }

        private float CalcSectorDiff(Sectors<float> current, Sectors<float> last, Sectors<float> best)
        {
            if (best.Sector1 == 0 || current.Sector1 == 0)
            {
                return 0;
            }

            Sectors<float> compare = current;
            
            if (compare.Sector3 > 0)
            {
                return compare.Sector3 - best.Sector3;
            }

            if (compare.Sector2 > 0)
            {
                return compare.Sector2 - best.Sector2;
            }

            if (compare.Sector1 > 0)
            {
                return compare.Sector1 - best.Sector1;
            }

            return 0;
        }
    }
}
