using System;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace Smartitecture.Services.Hardware
{
    public class SensorMonitor
    {
        public async Task<double?> GetCpuTemperatureAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    try
                    {
                        using var searcher = new ManagementObjectSearcher(@"root\\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
                        var results = searcher.Get().Cast<ManagementObject>()
                            .Select(mo => ConvertToCelsius(mo))
                            .Where(v => v != null)
                            .Select(v => v!.Value)
                            .ToList();
                        if (results.Count > 0)
                        {
                            // Some systems expose multiple zones; pick median to avoid outliers
                            results.Sort();
                            return (double?)results[results.Count / 2];
                        }
                    }
                    catch { }
                    return null;
                });
            }
            catch { return null; }
        }

        public async Task<double?> GetGpuTemperatureAsync()
        {
            await Task.CompletedTask;
            return null;
        }

        private static double? ConvertToCelsius(ManagementObject mo)
        {
            try
            {
                // Temperature in tenths of Kelvin (per ACPI spec)
                var value = mo["CurrentTemperature"];
                if (value == null) return null;
                var kelvinX10 = System.Convert.ToDouble(value);
                var kelvin = kelvinX10 / 10.0;
                var celsius = kelvin - 273.15;
                if (celsius < -50 || celsius > 150) return null; // discard bogus
                return celsius;
            }
            catch { return null; }
        }
    }
}
