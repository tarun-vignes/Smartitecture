using System;

namespace Smartitecture.Services.Safety
{
    public sealed class AuditLogger
    {
        public void Log(string message)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[Audit] {DateTime.UtcNow:o} {message}");
            }
            catch
            {
                // no-op
            }
        }
    }
}
