using DashboardCore;
using System;
using System.IO.Ports;
using System.Threading;

namespace DashboardTm1638
{
    class Tm1638Handler
    {
        private SerialPort port = null;
        private Collector collector;
        private Thread collectorThread = null;
        
        public bool IsConnected
        {
            get
            {
                return port != null && port.IsOpen;
            }
        }

        public Tm1638Handler()
        {
            collector = new Collector();
            collector.Update += Collector_Update;
            collectorThread = new Thread(collector.Run);
        }

        private void Collector_Update(ExchangeData data)
        {
            Send(data);
        }

        public bool Connect(String portName)
        {
            // Old one open?
            Disconnect();

            try
            {
                port = new SerialPort(portName, 9600, Parity.None, 8);
                port.Open();

                collectorThread.Start();
            } catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                collectorThread.Interrupt();
                port.Close();
            }
        }

        private void Send(ExchangeData data)
        {
            if (!IsConnected)
            {
                return;
            }

            short rpm16Bit = Convert.ToInt16(data.EngineRpm);
            short speed16Bit = Convert.ToInt16(data.CarSpeed);

            byte[] serialdata = new byte[9];
            //byte[] shiftlights = new byte[16];
            serialdata[0] = 255;
            serialdata[1] = Convert.ToByte(data.Gear + 1);
            serialdata[2] = Convert.ToByte((speed16Bit >> 8) & 0x00FF);
            serialdata[3] = Convert.ToByte(speed16Bit & 0x00FF);
            serialdata[4] = Convert.ToByte((rpm16Bit >> 8) & 0x00FF);
            serialdata[5] = Convert.ToByte(rpm16Bit & 0x00FF);
            serialdata[6] = Convert.ToByte(Math.Round(data.FuelLeft));
            serialdata[7] = Convert.ToByte(GetLedIndex(data));
            serialdata[8] = data.PitLimiter > 0 ? (byte)0x10 : (byte)0x00; // Engine.
            
            port.Write(serialdata, 0, serialdata.Length);
        }

        private uint GetLedIndex(ExchangeData data)
        {
            double rpmInPercent = data.MaxEngineRpm > 0 ? (double)data.EngineRpm / (double)data.MaxEngineRpm : 0;

            if (rpmInPercent < 0.85)
            {
                return 0; // no leds
            }

            if (rpmInPercent < 0.87)
            {
                return 1; // 1x green
            }

            if (rpmInPercent < 0.89)
            {
                return 2; // 2x green
            }

            if (rpmInPercent < 0.91)
            {
                return 3; // 3x green
            }

            if (rpmInPercent < 0.93)
            {
                return 4; // 3x green + 1x orange
            }

            if (rpmInPercent < 0.94)
            {
                return 5; // 3x green + 2x orange
            }

            if (rpmInPercent < 0.95)
            {
                return 6; // 3x green + 3x orange
            }

            if (rpmInPercent < 0.96)
            {
                return 7; // 3x green + 3x orange + 1x red
            }

            if (rpmInPercent < 0.97)
            {
                return 8; // 3x green + 3x orange + 2x red
            }

            if (rpmInPercent < 0.98)
            {
                return 9; // full red
            }

            return 10; // blink full red
        }
    }
}
