using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using VityazevFinalWork.Solution.Models;

namespace VityazevFinalWork.Solution.Misc
{
    internal class Shared
    {
        public static List<T>? DeepCopy<T>(List<T> oldList)
        {
            string json = JsonConvert.SerializeObject(oldList);
            return JsonConvert.DeserializeObject<List<T>>(json);
        }

        public static float ConvertToUnixTime2(DateTime date)
        {
            var moscowTime = TimeZoneInfo.ConvertTimeToUtc(date, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time"));
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return (float)(moscowTime - epoch).TotalSeconds;
        }

        public static float ConvertToUnixTime(DateTime date)
        {
            return (float)(date - new DateTime(1970, 1, 1)).TotalDays;
        }


        public static List<TData> SmoothData(List<TData> data, int windowSize)
        {
            var smoothedData = new List<TData>();

            for (int i = 0; i < data.Count; i++)
            {
                int windowStart = Math.Max(0, i - windowSize / 2);
                int windowEnd = Math.Min(data.Count - 1, i + windowSize / 2);
                double windowSum = 0;

                for (int j = windowStart; j <= windowEnd; j++)
                {
                    windowSum += data[j].amount;
                }

                var smoothedValue = windowSum / (windowEnd - windowStart + 1);
                smoothedData.Add(new TData { date = data[i].date, amount = smoothedValue });
            }

            return smoothedData;
        }
    }
}
