﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashboardServer.Game
{
    abstract class AbstractGame
    {
        public delegate void UpdateEventHandler(ExchangeData data);
        public abstract event UpdateEventHandler Update;

        public abstract bool IsRunning { get; }

        public abstract void Start();

        public abstract void Stop();
    }

    public class ExchangeData
    {
        public int CarSpeed { get; internal set; }
        public int Gear { get; internal set; }
        public int MaxEngineRpm { get; internal set; }
        public int EngineRpm { get; internal set; }

        public int DrsAvailable { get; internal set; }
        public int DrsEngaged { get; internal set; }
        public int DrsEquipped { get; internal set; }
        public int DrsNumActivationsLeft { get; internal set; }

        public double FuelLapsLeftEstimate { get; internal set; }
        public double FuelLeft { get; internal set; }
        public double FuelPerLap { get; internal set; }
        public double FuelMax { get; internal set; }
        public int NumberOfLaps { get; internal set; }
        public int CompletedLaps { get; internal set; }
        public int Position { get; internal set; }
        public int NumCars { get; internal set; }

        // Lap times.
        public double LapTimeCurrentSelf { get; internal set; }
        public double LapTimePreviousSelf { get; internal set; }
        public double LapTimeBestSelf { get; internal set; }
        public double SessionTimeRemaining { get; internal set; }
        public double LapTimeBestLeader { get; internal set; }
        public double LapTimeBestLeaderClass { get; internal set; }

        // Deltas.
        public double Delta { get; internal set; }
        public double LapTimeDeltaLeader { get; internal set; }
        public double LapTimeDeltaLeaderClass { get; internal set; }
        public double TimeDeltaBehind { get; internal set; }
        public double TimeDeltaFront { get; internal set; }

        public bool YellowFlagAhead { get; internal set; }
        public int CurrentSector { get; internal set; }

        public int PitLimiter { get; internal set; }
        public double DistanceTravelled { get; internal set; }

        public double AirTemperature { get; internal set; }
        public double TrackTemperature { get; internal set; }

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