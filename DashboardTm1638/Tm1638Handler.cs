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
        }

        private void Collector_Update(ExchangeData data)
        {
            try
            {
                Send(data);
            } catch (Exception e)
            {
                Console.WriteLine("Something went wrong here: " + e.Message);
            }
        }

        public bool Connect(String portName)
        {
            // Old one open?
            Disconnect();

            try
            {
                port = new SerialPort(portName, 9600, Parity.None, 8);
                port.Open();

                // Wait 2 second.
                Thread.Sleep(2500);

                if (!port.IsOpen)
                {
                    Console.WriteLine("Cannot open port.");
                    return false;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Connection to serial port failed.");
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
            collectorThread.Abort();
        }

        public void Test()
        {
            int speed = 1;
            int rpm = 800;
            for (int i = 0; i < 10; i++)
            {
                speed += 20;
                rpm += 20;

                Send(rpm, 1000, speed, 0, 0, 0, 0, 100, 140, 99, 123, 1, 12, 11 * (60000) + 9 * 1000 + 321);
                Thread.Sleep(250);
            }

            Thread.Sleep(250);
            Send(1, 1000, 1, 0, 0, 0, 0, 100, 140, 99, 123, 1, 12, 11 * (60000) + 9 * 1000 + 321);

        }

        public void Disconnect()
        {
            if (IsConnected)
            {
                collectorThread.Abort();
                port.Close();
                Console.WriteLine("Disconnected from T1M1638 module.");
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
                Console.WriteLine("Not connected anymore.");
                return;
            }

            try
            {
                Console.WriteLine(DateTime.Now.ToLongTimeString() + " (TM1638): " + rpm + "/" + rpmMax + "/" + carSpeed + "/" + fuelLeft + "/" + fuelLapsLeft + "/" + pitLimiter + "/" + waterTemp + "/" + oilTemp + "/" + lap + "/" + laps + "/" + pos + "/" + cars + "/" + lapTimeInMillis);

                short rpm16Bit = 0;// Convert.ToInt16(rpm < 0 ? 0 : rpm);
                short speed16Bit = Convert.ToInt16(carSpeed < 0 ? 0 : carSpeed);

                byte[] serialdata = new byte[20];
                int index = 0;
                serialdata[index++] = 255;
                serialdata[index++] = Convert.ToByte(gear + 1);
                serialdata[index++] = Convert.ToByte((speed16Bit >> 8) & 0x00FF);
                serialdata[index++] = Convert.ToByte(speed16Bit & 0x00FF);
                serialdata[index++] = Convert.ToByte((rpm16Bit >> 8) & 0x00FF);
                serialdata[index++] = Convert.ToByte(rpm16Bit & 0x00FF);
                serialdata[index++] = Convert.ToByte(fuelLeft < 0 ? 0 : Math.Floor(fuelLeft));
                serialdata[index++] = Convert.ToByte(fuelLapsLeft < 0 ? 0 : Math.Floor(fuelLapsLeft));
                serialdata[index++] = Convert.ToByte(GetLedIndex(rpm, rpmMax));
                serialdata[index++] = Convert.ToByte(pitLimiter < 0 ? 0 : pitLimiter);
                serialdata[index++] = Convert.ToByte(waterTemp < 0 ? 0 : Math.Round(waterTemp));
                serialdata[index++] = Convert.ToByte(oilTemp < 0 ? 0 : Math.Round(oilTemp));
                serialdata[index++] = Convert.ToByte(lap < 0 ? 0 : lap);
                serialdata[index++] = Convert.ToByte(laps < 0 ? 0 : laps);
                serialdata[index++] = Convert.ToByte(pos < 0 ? 0 : pos);
                serialdata[index++] = Convert.ToByte(cars < 0 ? 0 : cars);

                // Convert lap time in minutes, seconds and millis.
                long minuteInMillis = (60 * 1000);
                long secondInMillis = 1000;

                long tmpTime = lapTimeInMillis < 0 ? 0 : lapTimeInMillis;
                long minutesLapTime = (tmpTime - (tmpTime % minuteInMillis)) / minuteInMillis;
                tmpTime -= minutesLapTime * minuteInMillis;

                long secondsLapTime = (tmpTime - (tmpTime % secondInMillis)) / secondInMillis;
                long millisLapTime = tmpTime - secondsLapTime * secondInMillis;

                serialdata[index++] = Convert.ToByte(minutesLapTime < 0 ? 0 : minutesLapTime);
                serialdata[index++] = Convert.ToByte(secondsLapTime < 0 ? 0 : secondsLapTime);
                serialdata[index++] = Convert.ToByte(millisLapTime < 0 ? 0 : ((millisLapTime >> 8) & 0x00FF));
                serialdata[index++] = Convert.ToByte(millisLapTime < 0 ? 0 : millisLapTime & 0x00FF);

                port.Write(serialdata, 0, serialdata.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + " / " + e.StackTrace);
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
