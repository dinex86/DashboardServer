using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;

namespace DashboardCore.Game
{
    class F12017Game : AbstractGame
    {
        public override string[] GameExecutables
        {
            get
            {
                return new string[] { "F1_2017" };
            }
        }

        private IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 20777);
        //private IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 20777);
        
        private UdpClient udp = new UdpClient();
        private Thread updateThread;

        public override event UpdateEventHandler Update;

        public override void Start()
        {
            Console.WriteLine("Starting collector for F1 2017!");

            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udp.ExclusiveAddressUse = false;
            udp.Client.Bind(endPoint);

            updateThread = new Thread(new ThreadStart(Receive));
            updateThread.Start();
        }

        public override void Stop()
        {
            Console.WriteLine("Stopping collector for F1 2017!");
            
            updateThread.Abort();
            udp.Close();
        }

        private void Receive()
        {
            while (true)
            {
                try
                {
                    byte[] data = udp.Receive(ref endPoint);
                    udp.Send(new byte[] { 1 }, 1, endPoint); // if data is received reply letting the client know that we got his data
                    Parse(data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Thread.Sleep(20);
                }
               
            }
        }

        private void Parse(byte[] udpData)
        {
            // Marshal the byte array into the telemetryPacket structure
            var gcHandle = GCHandle.Alloc(udpData, GCHandleType.Pinned);
            var f1Data = (F1Data)Marshal.PtrToStructure(
                gcHandle.AddrOfPinnedObject(), typeof(F1Data));
            gcHandle.Free();

            data.CarSpeed = (int)Math.Round(f1Data.Speed * 3.6);
            data.Gear = (int)f1Data.Gear - 1;
            data.LapTimeCurrentSelf = f1Data.LapTime;
            data.EngineRpm = (int)Math.Round(f1Data.EngineRevs);
            data.MaxEngineRpm = 12600; // it's actually 13500, but the shift indicator is better :)

            Console.WriteLine(DateTime.Now.ToLongTimeString() + ": " + data.CarSpeed + "/" + data.Gear + "/" + data.LapTimeCurrentSelf + "/" + data.EngineRpm + "/" + data.MaxEngineRpm);

            Update(data);
        }
    }
    
    public struct F1Data
    {
        public F1Data(SerializationInfo info, StreamingContext context)
        {
            Time = (float)info.GetValue("Time", typeof(float));
            LapTime = (float)info.GetValue("LapTime", typeof(float));
            LapDistance = (float)info.GetValue("LapDistance", typeof(float));
            Distance = (float)info.GetValue("Distance", typeof(float));
            Speed = (float)info.GetValue("Speed", typeof(float));
            Lap = (float)info.GetValue("Lap", typeof(float));
            X = (float)info.GetValue("X", typeof(float));
            Y = (float)info.GetValue("Y", typeof(float));
            Z = (float)info.GetValue("Z", typeof(float));
            WorldSpeedX = (float)info.GetValue("WorldSpeedX", typeof(float));
            WorldSpeedY = (float)info.GetValue("WorldSpeedY", typeof(float));
            WorldSpeedZ = (float)info.GetValue("WorldSpeedZ", typeof(float));
            XR = (float)info.GetValue("XR", typeof(float));
            Roll = (float)info.GetValue("Roll", typeof(float));
            ZR = (float)info.GetValue("ZR", typeof(float));
            XD = (float)info.GetValue("XD", typeof(float));
            Pitch = (float)info.GetValue("Pitch", typeof(float));
            ZD = (float)info.GetValue("ZD", typeof(float));
            SuspensionPositionRearLeft = (float)info.GetValue("SuspensionPositionRearLeft", typeof(float));
            SuspensionPositionRearRight = (float)info.GetValue("SuspensionPositionRearRight", typeof(float));
            SuspensionPositionFrontLeft = (float)info.GetValue("SuspensionPositionFrontLeft", typeof(float));
            SuspensionPositionFrontRight = (float)info.GetValue("SuspensionPositionFrontRight", typeof(float));
            SuspensionVelocityRearLeft = (float)info.GetValue("SuspensionVelocityRearLeft", typeof(float));
            SuspensionVelocityRearRight = (float)info.GetValue("SuspensionVelocityRearRight", typeof(float));
            SuspensionVelocityFrontLeft = (float)info.GetValue("SuspensionVelocityFrontLeft", typeof(float));
            SuspensionVelocityFrontRight = (float)info.GetValue("SuspensionVelocityFrontRight", typeof(float));
            WheelSpeedBackLeft = (float)info.GetValue("WheelSpeedBackLeft", typeof(float));
            WheelSpeedBackRight = (float)info.GetValue("WheelSpeedBackRight", typeof(float));
            WheelSpeedFrontLeft = (float)info.GetValue("WheelSpeedFrontLeft", typeof(float));
            WheelSpeedFrontRight = (float)info.GetValue("WheelSpeedFrontRight", typeof(float));
            Throttle = (float)info.GetValue("Throttle", typeof(float));
            Steer = (float)info.GetValue("Steer", typeof(float));
            Brake = (float)info.GetValue("Brake", typeof(float));
            Clutch = (float)info.GetValue("Clutch", typeof(float));
            Gear = (float)info.GetValue("Gear", typeof(float));
            LateralAcceleration = (float)info.GetValue("LateralAcceleration", typeof(float));
            LongitudinalAcceleration = (float)info.GetValue("LongitudinalAcceleration", typeof(float));
            EngineRevs = (float)info.GetValue("EngineRevs", typeof(float));
        }
        
        public float Time;
        public float LapTime;
        public float LapDistance;
        public float Distance;
        public float X;
        public float Y;
        public float Z;
        public float Speed;
        public float WorldSpeedX;
        public float WorldSpeedY;
        public float WorldSpeedZ;
        public float XR;
        public float Roll;
        public float ZR;
        public float XD;
        public float Pitch;
        public float ZD;
        public float SuspensionPositionRearLeft;
        public float SuspensionPositionRearRight;
        public float SuspensionPositionFrontLeft;
        public float SuspensionPositionFrontRight;
        public float SuspensionVelocityRearLeft;
        public float SuspensionVelocityRearRight;     
        public float SuspensionVelocityFrontLeft;       
        public float SuspensionVelocityFrontRight;
        public float WheelSpeedBackLeft;
        public float WheelSpeedBackRight;
        public float WheelSpeedFrontLeft;
        public float WheelSpeedFrontRight;
        public float Throttle;
        public float Steer;
        public float Brake;
        public float Clutch;
        public float Gear;
        public float LateralAcceleration;
        public float LongitudinalAcceleration;
        public float Lap;
        
        public float EngineRevs;
        
        public float SpeedInKmPerHour
        {
            get { return Speed * 3.60f; }
        }
        
        public bool IsSittingInPits
        {
            get {
                return false; // return Math.Abs(LapTime - 0) < Constants.Epsilon && Math.Abs(Speed - 0) < Constants.Epsilon;
            }
        }
        
        public bool IsInPitLane
        {
            get { return true; }
        }
    }
}