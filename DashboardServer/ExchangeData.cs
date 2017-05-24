using R3E;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R3E.Data;
using System.Globalization;

namespace DashboardServer
{
    class ExchangeData
    {
        public int CarSpeed { get; private set; }
        public int Gear { get; private set; }
        public int MaxEngineRpm { get; private set; }
        public int EngineRpm { get; private set; }

        public int DrsAvailable { get; private set; }
        public int DrsEngaged { get; private set; }
        public int DrsEquipped { get; private set; }
        public int DrsNumActivationsLeft { get; private set; }

        public double FuelLapsLeftEstimate { get; private set; }
        public double FuelLeft { get; private set; }
        public double FuelPerLap { get; private set; }
        public int NumberOfLaps { get; private set; }
        public int CompletedLaps { get; private set; }
        public int Position { get; private set; }
        public int NumCars { get; private set; }

        public double LapTimeCurrentSelf { get; private set; }
        public double LapTimePreviousSelf { get; private set; }
        public double LapTimeBestSelf { get; private set; }
        public double SessionTimeRemaining { get; private set; }
        public double LapTimeBestLeader { get; private set; }
        public double LapTimeBestLeaderClass { get; private set; }
        public double LapTimeDeltaLeader { get; private set; }
        public double LapTimeDeltaLeaderClass { get; private set; }
        public double TimeDeltaBehind { get; private set; }
        public double TimeDeltaFront { get; private set; }
        public bool YellowFlagAhead { get; private set; }

        public ExchangeData(R3E.Data.Shared data)
        {
            if (data.EngineRps  > -1.0f)
            {
              //  RPM = Math.Round(Utilities.RpsToRpm(data.EngineRps));
            }
            
            CarSpeed = (int)Math.Round(Utilities.MpsToKph(data.CarSpeed));
            Gear = data.Gear;
            MaxEngineRpm = (int)Math.Round(Utilities.RpsToRpm(data.MaxEngineRps));
            EngineRpm = (int)Math.Round(Utilities.RpsToRpm(data.EngineRps));
            DrsAvailable = data.Drs.Available;
            DrsEngaged = data.Drs.Engaged;
            DrsEquipped = data.Drs.Equipped;
            DrsNumActivationsLeft = data.Drs.NumActivationsLeft;
            FuelLapsLeftEstimate = -1;
            FuelLeft = -1;
            FuelPerLap = -1;
            NumberOfLaps = data.NumberOfLaps;
            CompletedLaps = data.CompletedLaps;
            Position = data.Position;
            NumCars = data.NumCars;
                        
            LapTimeCurrentSelf = Math.Round(data.LapTimeCurrentSelf, 3);
            LapTimePreviousSelf = Math.Round(data.LapTimePreviousSelf, 3);
            LapTimeBestSelf = Math.Round(data.LapTimeBestSelf, 3);
            LapTimeBestLeader = Math.Round(data.LapTimeBestLeader, 3);
            LapTimeBestLeaderClass = Math.Round(data.LapTimeBestLeaderClass, 3);
            SessionTimeRemaining = Math.Round(data.SessionTimeRemaining, 3);
            LapTimeDeltaLeader = Math.Round(data.LapTimeDeltaLeader, 3);
            LapTimeDeltaLeaderClass = Math.Round(data.LapTimeDeltaLeaderClass, 3);
            TimeDeltaBehind = Math.Round(data.TimeDeltaBehind, 3);
            TimeDeltaFront = Math.Round(data.TimeDeltaFront, 3);
            YellowFlagAhead = data.ClosestYellowDistanceIntoTrack > 0 && data.ClosestYellowDistanceIntoTrack < 1000;
        }
        
        public String ToJSON()
        {
            StringBuilder sb = new StringBuilder();
            String appender = "";

            foreach (var prop in GetType().GetProperties())
            {
                sb.Append(appender);

                String niceValue = "";
                Object value = prop.GetValue(this, null);

                if (value is String)
                {
                    niceValue = "\"" + value.ToString() + "\"";
                }
                else if (value is double)
                {
                    niceValue = ((double)value).ToString("0.000", CultureInfo.CreateSpecificCulture("EN-us"));
                }
                else if (value is int)
                {
                    niceValue = ((int)value).ToString("0", CultureInfo.CreateSpecificCulture("EN-us"));
                }
                else if (value is bool)
                {
                    niceValue = (bool)value ? "true" : "false";
                }

                sb.AppendFormat("\"{0}\":{1}", prop.Name, niceValue);

                appender = ",";
            }

            return String.Format("{{{0}}}", sb.ToString());
        }
    }
}
