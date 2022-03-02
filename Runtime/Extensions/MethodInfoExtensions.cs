using System.Reflection;

namespace LightweightBenchmark.Extensions
{
    public static class MethodInfoExtensions
    {
        public static bool HasAttribute<T>(this MethodInfo methodInfo, bool inherit = false)
        {
            return methodInfo.GetCustomAttributes(typeof(BenchmarkMethodAttribute), inherit).Length > 0;
        }
    }
}