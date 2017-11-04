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
using DashboardCore.Game;

namespace DashboardCore
{
    public class GameDataCollector
    {
        public delegate void UpdateEventHandler(ExchangeData data);
        public event UpdateEventHandler Update;
        private List<int> detectedButtonPresses = new List<int>();

        public void Run()
        {
            List<AbstractGame> games = new List<AbstractGame>
            {
                new AssettoCorsaGame(),
                new RaceRoomGame(),
                new PCarsGame(),
                new F12017Game()
            };

            // Find the active game.
            AbstractGame runningGame = null;
            while (true)
            {
                try
                {
                    while (runningGame == null)
                    {
                        foreach (AbstractGame game in games)
                        {
                            if (game.IsRunning)
                            {
                                runningGame = game;
                                runningGame.Update += DataUpdated;
                                runningGame.Start();
                                break;
                            }
                        }

                        Thread.Sleep(1000);
                    }

                    // Game still running?
                    if (runningGame != null && !runningGame.IsRunning)
                    {
                        DataUpdated(new ExchangeData()); // Send empty data.
                        runningGame.Stop();
                        runningGame = null;
                    }

                    // Wait for next try.
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Something went wrong in this game: " + e.Message);
                }
            }
        }

        private void ButtonPressed(int button)
        {
            if (!detectedButtonPresses.Contains(button))
            {
                detectedButtonPresses.Add(button);
            }
        }

        private void DataUpdated(ExchangeData data)
        {
            if (Update != null)
            {
                data.PressedButtons.Clear();
                data.PressedButtons.AddRange(detectedButtonPresses);
                detectedButtonPresses.Clear();

                Update(data);
            }
        }
    }
}
