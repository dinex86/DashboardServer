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

        private ExchangeData data = new ExchangeData();
        private Thread updateThread = null;

        public override bool IsRunning
        {
            get {
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
                    byte[] buffer = null;

                    while (file == null)
                    {
                        Console.WriteLine("Mapping R3E shared memory...");

                        try
                        {
                            file = MemoryMappedFile.OpenExisting(Constant.SharedMemoryName);
                            Console.WriteLine("Memory mapped successfully.");
                            buffer = new Byte[Marshal.SizeOf(typeof(Shared))];
                        }
                        catch (FileNotFoundException)
                        {
                            // Game not ready, wait a little bit.
                            Thread.Sleep(500);
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
                catch (Exception)
                {
                    if (file != null)
                    {
                        file.Dispose();
                    }
                    file = null;
                }
                finally
                {
                    if (file != null)
                    {
                        file.Dispose();
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

            data.CarSpeed = (int)Math.Round(Utilities.MpsToKph(shared.CarSpeed));
            data.Gear = shared.Gear;
            data.MaxEngineRpm = (int)Math.Round(Utilities.RpsToRpm(shared.MaxEngineRps));
            data.EngineRpm = (int)Math.Round(Utilities.RpsToRpm(shared.EngineRps));

            data.DrsAvailable = shared.Drs.Available;
            data.DrsEngaged = shared.Drs.Engaged;
            data.DrsEquipped = shared.Drs.Equipped;
            data.DrsNumActivationsLeft = shared.Drs.NumActivationsLeft;

            data.PushToPassEquipped = shared.PushToPass.Available;
            data.PushToPassAvailable = shared.PushToPass.Available;
            data.PushToPassEngaged = shared.PushToPass.Engaged;
            data.PushToPassNumActivationsLeft = shared.PushToPass.AmountLeft;
            data.PushToPassEngagedTimeLeft = shared.PushToPass.EngagedTimeLeft;
            data.PushToPassWaitTimeLeft = shared.PushToPass.WaitTimeLeft;

            data.OilTemperature = shared.EngineOilTemp;
            data.WaterTemperature = shared.EngineWaterTemp;

            data.FuelLapsLeftEstimate = -1;
            data.FuelLeft = -1;
            data.FuelPerLap = -1;
            data.NumberOfLaps = shared.NumberOfLaps;
            data.CompletedLaps = shared.CompletedLaps;
            data.Position = shared.Position;
            data.NumCars = shared.NumCars;

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

            data.YellowFlagAhead = shared.ClosestYellowDistanceIntoTrack > 0 && shared.ClosestYellowDistanceIntoTrack < 1000;
            data.CurrentSector = shared.TrackSector;

            Update?.Invoke(data);
        }
    }
}
