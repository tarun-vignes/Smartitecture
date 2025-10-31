using System.Threading.Tasks;

namespace Smartitecture.Services.Hardware
{
    public class PerformanceSnapshot
    {
        public double CpuUsagePercent { get; set; }
        public double MemoryUsagePercent { get; set; }
        public double DiskUsagePercent { get; set; }
    }

    public class PerformanceMetrics
    {
        public async Task<PerformanceSnapshot> GetSnapshotAsync()
        {
            // Placeholder: wire up PerformanceConnector to fill values
            await Task.CompletedTask;
            return new PerformanceSnapshot();
        }
    }
}

