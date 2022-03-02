using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LightweightBenchmark.Extensions;
using Debug = UnityEngine.Debug;

namespace LightweightBenchmark
{
    public class Benchmark<T> where T : new()
    {
        private readonly T _benchmarkClass;
        private readonly MethodInfo[] _benchmarkMethods;

        private readonly Stopwatch _globalStopwatch;
        private readonly Stopwatch _stepStopwatch;

        private readonly uint _iterations;
        private readonly TimeUnit _timeUnit;

        private bool _isStopped;
        
        public Benchmark(uint iterations, TimeUnit timeUnit = TimeUnit.Milliseconds)
        {
            _iterations = iterations;
            _timeUnit = timeUnit;

            _benchmarkClass = new T();

            _benchmarkMethods = _benchmarkClass.GetType()
                .GetMethods()
                .Where(x => x.HasAttribute<BenchmarkMethodAttribute>())
                .ToArray();

            _stepStopwatch = new Stopwatch();
            _globalStopwatch = new Stopwatch();
        }

        public async void Run()
        {
            foreach (var method in _benchmarkMethods)
            {
                if (_isStopped)
                {
                    break;
                }
                
                _globalStopwatch.Restart();
                await Run(method);
                _globalStopwatch.Stop();
                
                Log($"{method.Name.B()} full time: {_globalStopwatch.Get(_timeUnit)}");
            }
        }

        public void Stop()
        {
            _isStopped = true;
        }
        

        private async Task Run(MethodBase method)
        {
            Warmup(method);

            double median = MeasureInvocation(method);
            double? error = null;

            for (var i = 1; i < _iterations; i++)
            {
                var val = MeasureInvocation(method);
                error = CalculateError(val, median, error);

                median += val;
                median /= 2;

                if (_isStopped)
                {
                    break;
                }
                
                if (i % 10_00_000 != 0)
                {
                    continue;
                }
                
                ProgressLog(method, i /(double) _iterations * 100);
                await Task.Yield();
            }

            Log($"{method.Name.B()}: median = {median}; error = {error.Value}");
        }

        private void Warmup(MethodBase method)
        {
            for (var i = 0; i < 10; i++)
            {
                MeasureInvocation(method);
            }
        }

        private double MeasureInvocation(MethodBase method)
        {
            _stepStopwatch.Restart();
            method.Invoke(_benchmarkClass, Array.Empty<object>());
            _stepStopwatch.Stop();

            return _stepStopwatch.Get(_timeUnit);
        }

        private static double? CalculateError(double val, double median, double? error)
        {
            var diff = Math.Abs(val - median);

            if (error.HasValue)
            {
                error += diff;
                error /= 2;
            }
            else
            {
                error = diff;
            }

            return error;
        }

        private static void Log(string message)
        {
            Debug.LogWarning($"<color=yellow>[Benchmark]</color> {message}");
        }

        private static void ProgressLog(MemberInfo method, double percentage)
        {
            Debug.Log($"<color=yellow>[Benchmark]</color>{method.Name.B()}: {percentage}%");
        }
    }
}