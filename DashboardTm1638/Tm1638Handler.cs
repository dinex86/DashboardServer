using DashboardCore;
using System;
using System.IO.Ports;
using System.Threading;

namespace DashboardTm1638
{
    class Tm1638Handler
    {
        private SerialPort port = null;
        private GameDataCollector collector;
        private Thread collectorThread = null;
        private Thread testThread = null;

        public bool IsConnected
        {
            get
            {
                return port != null && port.IsOpen;
            }
        }

        public Tm1638Handler()
        {
            collector = new GameDataCollector();
            collector.Update += Collector_Update;
            collectorThread = new Thread(collector.Run);
            testThread = new Thread(Test);
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
            } catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void Start()
        {
            collectorThread.Start();
        }

        public void Stop()
        {
            collectorThread.Interrupt();
        }

        public void StartTest()
        {
            testThread.Start();
        }

        public void StopTest()
        {
            testThread.Interrupt();
        }

        private void Test()
        {
            int speed = 1;
            int rpm = 810;
            int speedIncrement = 1;
            int rpmIncrement = 5;
            while (testThread.IsAlive)
            {
                if (speed >= 125 || speed <= 0)
                {
                    speedIncrement *= -1;
                }

                if (rpm >= 1000 || rpm <= 800)
                {
                    rpmIncrement *= -1;
                }

                speed += speedIncrement;
                rpm += rpmIncrement;

                Send(rpm, 1000, speed, 0, 0, 0, 0, 100, 140, 99, 123, 1, 12, 11 * (60000) + 9 * 1000 + 321);
                Thread.Sleep(200);
            }
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
            Send(data.EngineRpm, data.MaxEngineRpm, data.CarSpeed, data.Gear, data.FuelLeft, data.FuelLapsLeftEstimate, data.PitLimiter, data.WaterTemperature, data.OilTemperature, data.CompletedLaps, data.NumberOfLaps, data.Position, data.NumCars, (long)Math.Round(data.LapTimeCurrentSelf * 1000.0));
        }

        private void Send(int rpm, int rpmMax, int carSpeed, int gear, double fuelLeft, double fuelLapsLeft, int pitLimiter, double waterTemp, double oilTemp, int lap, int laps, int pos, int cars, long lapTimeInMillis)
        {
            if (!IsConnected)
            {
                return;
            }

            try
            {
                short rpm16Bit = Convert.ToInt16(rpm);
                short speed16Bit = Convert.ToInt16(carSpeed);

                byte[] serialdata = new byte[20];
                int index = 0;
                serialdata[index++] = 255;
                serialdata[index++] = Convert.ToByte(gear + 1);
                serialdata[index++] = Convert.ToByte((speed16Bit >> 8) & 0x00FF);
                serialdata[index++] = Convert.ToByte(speed16Bit & 0x00FF);
                serialdata[index++] = Convert.ToByte((rpm16Bit >> 8) & 0x00FF);
                serialdata[index++] = Convert.ToByte(rpm16Bit & 0x00FF);
                serialdata[index++] = Convert.ToByte(Math.Floor(fuelLeft));
                serialdata[index++] = Convert.ToByte(fuelLapsLeft >= 0 ? Math.Floor(fuelLapsLeft) : 0);
                serialdata[index++] = Convert.ToByte(GetLedIndex(rpm, rpmMax));
                serialdata[index++] = Convert.ToByte(pitLimiter < 0 ? 0 : pitLimiter);
                serialdata[index++] = Convert.ToByte(Math.Round(waterTemp));
                serialdata[index++] = Convert.ToByte(Math.Round(oilTemp));
                serialdata[index++] = Convert.ToByte(lap);
                serialdata[index++] = Convert.ToByte(laps < 0 ? 0 : laps);
                serialdata[index++] = Convert.ToByte(pos);
                serialdata[index++] = Convert.ToByte(cars);

                // Convert lap time in minutes, seconds and millis.
                long minuteInMillis = (60 * 1000);
                long secondInMillis = 1000;

                long tmpTime = lapTimeInMillis < 0 ? 0 : lapTimeInMillis;
                long minutesLapTime = (tmpTime - (tmpTime % minuteInMillis)) / minuteInMillis;
                tmpTime -= minutesLapTime * minuteInMillis;

                long secondsLapTime = (tmpTime - (tmpTime % secondInMillis)) / secondInMillis;
                long millisLapTime = tmpTime - secondsLapTime * secondInMillis;

                serialdata[index++] = Convert.ToByte(minutesLapTime);
                serialdata[index++] = Convert.ToByte(secondsLapTime);
                serialdata[index++] = Convert.ToByte((millisLapTime >> 8) & 0x00FF);
                serialdata[index++] = Convert.ToByte(millisLapTime & 0x00FF);

                port.Write(serialdata, 0, serialdata.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        private uint GetLedIndex(int rpm, int rpmMax)
        {
            double rpmInPercent = rpmMax > 0 ? (double)rpm / (double)rpmMax : 0;

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
