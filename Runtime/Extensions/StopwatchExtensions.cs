using System;
using System.Diagnostics;

namespace LightweightBenchmark.Extensions
{
    public static class StopwatchExtensions
    {
        public static double Get(this Stopwatch stopwatch, TimeUnit unit)
        {
            long ticks = stopwatch.ElapsedTicks;
            double ns = 1000000000.0 * ticks / Stopwatch.Frequency;
            double ms = ns / 1000000.0;
            double s = ms / 1000;
            
            return unit switch
            {
                TimeUnit.Nanoseconds => ns,
                TimeUnit.Ticks => stopwatch.ElapsedTicks,
                TimeUnit.Milliseconds => ms,
                TimeUnit.Seconds => s,
                _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null)
            };
        }
    }
}