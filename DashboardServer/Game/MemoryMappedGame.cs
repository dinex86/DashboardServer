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
    abstract class MemoryMappedGame<Test> : AbstractGame
    {
        public override event UpdateEventHandler Update;
        protected abstract string SharedMemoryName { get; }
        protected bool HasUpdate
        {
            get
            {
                return Update != null;
            }
        }

        protected UpdateEventHandler UpdateHandler
        {
            get
            {
                return Update;
            }
        }

        private Thread updateThread = null;
        
        public override void Start()
        {
            Console.WriteLine("Starting collector for " + GetType().Name + "!");

            updateThread = new Thread(new ThreadStart(Run));
            updateThread.Start();
        }

        public override void Stop()
        {
            Console.WriteLine("Starting collector for " + GetType().Name + "!");

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
                    Console.WriteLine("Mapping " + GetType().Name + " shared memory...");

                    byte[] buffer = null;
                    while (file == null)
                    {
                        try
                        {
                            file = MemoryMappedFile.OpenExisting(SharedMemoryName);
                            buffer = new Byte[Marshal.SizeOf(typeof(Test))];

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
                            buffer = stream.ReadBytes(Marshal.SizeOf(typeof(Test)));
                            GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

                            Test data = (Test)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Test));
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

        protected abstract void UpdateExchangeData(Test shared);
    }
 }
