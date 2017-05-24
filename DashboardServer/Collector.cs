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

namespace DashboardServer
{
    class Collector
    {
        public delegate void UpdateEventHandler(ExchangeData data);
        public event UpdateEventHandler Update;
        
        public void Run() {
            Console.WriteLine("Looking for a game...");

            Thread updateThread = null;
            IGame runningGame = null;
            while (true)
            {
                while (runningGame == null)
                {
                    if (Utilities.IsRrreRunning())
                    {
                        Console.WriteLine("R3E is running!");
                        runningGame = new R3EListener();
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

        private interface IGame
        {
            void Run();

            Boolean Update();

            ExchangeData GetData();
        }

        private class R3EListener : IGame, IDisposable
        {
            private bool Mapped
            {
                get { return (_file != null); }
            }

            public Shared data
            {
                private set;
                get;
            }

            private MemoryMappedFile _file;
            private byte[] _buffer;

            private readonly TimeSpan _timeAlive = TimeSpan.FromMinutes(10);
            private readonly TimeSpan _timeInterval = TimeSpan.FromMilliseconds(100);

            public void Dispose()
            {
                _file.Dispose();
            }

            public void Run()
            {
                var timeReset = DateTime.UtcNow;
                var timeLast = timeReset;

                Console.WriteLine("Looking for RRRE.exe...");

                while (true)
                {
                    var timeNow = DateTime.UtcNow;

                    if (timeNow.Subtract(timeReset) > _timeAlive)
                    {
                        break;
                    }

                    if (timeNow.Subtract(timeLast) < _timeInterval)
                    {
                        Thread.Sleep(1);
                        continue;
                    }

                    timeLast = timeNow;

                    if (Utilities.IsRrreRunning() && !Mapped)
                    {
                        Console.WriteLine("Found RRRE.exe, mapping shared memory...");

                        if (Map())
                        {
                            Console.WriteLine("Memory mapped successfully");
                            timeReset = DateTime.UtcNow;

                            _buffer = new Byte[Marshal.SizeOf(typeof(Shared))];
                        }
                    }
                }

                Console.WriteLine("All done!");
            }

            private bool Map()
            {
                try
                {
                    _file = MemoryMappedFile.OpenExisting(Constant.SharedMemoryName);
                    return true;
                }
                catch (FileNotFoundException)
                {
                    return false;
                }
            }

            public bool Update()
            {
                try
                {
                    var _view = _file.CreateViewStream();
                    BinaryReader _stream = new BinaryReader(_view);
                    _buffer = _stream.ReadBytes(Marshal.SizeOf(typeof(Shared)));
                    GCHandle _handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
                    data = (Shared)Marshal.PtrToStructure(_handle.AddrOfPinnedObject(), typeof(Shared));
                    _handle.Free();

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            public ExchangeData GetData()
            {
                return new ExchangeData(data);
            }
        }
    }
}
