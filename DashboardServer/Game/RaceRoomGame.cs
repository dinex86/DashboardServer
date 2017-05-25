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
    class RaceRoomGame : AbstractGame, IDisposable
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

        public override void Run()
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

        public override bool Update()
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

        public override ExchangeData GetData()
        {
            return new ExchangeData(data);
        }
    }
}
