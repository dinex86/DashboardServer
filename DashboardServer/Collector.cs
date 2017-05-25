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
using AssettoCorsaSharedMemory;
using DashboardServer.Game;

namespace DashboardServer
{
    class Collector
    {
        public delegate void UpdateEventHandler(ExchangeData data);
        public event UpdateEventHandler Update;
        
        public void Run() {
            Console.WriteLine("Looking for a game...");

            Thread updateThread = null;
            AbstractGame runningGame = null;
            while (true)
            {
                while (runningGame == null)
                {
                    if (Utilities.IsRrreRunning())
                    {
                        Console.WriteLine("R3E is running!");
                        runningGame = new RaceRoomGame();
                    }

                    Thread.Sleep(1000);
                }

                if (updateThread != null && !updateThread.IsAlive)
                {
                    // Reset.
                    runningGame = null;
                    updateThread = null;
                }
                else
                {
                    if (updateThread == null)
                    {
                        Console.WriteLine("Starting listener.");

                        updateThread = new Thread(new ThreadStart(runningGame.Run));
                        updateThread.Start();
                    }

                    if (Update != null && runningGame.Update())
                    {
                        ExchangeData data = runningGame.GetData();
                        Update(data);
                    }
                }

                Thread.Sleep(50);
            }
        }
    }
}
