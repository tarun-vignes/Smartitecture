using System.Security.Principal;
using System.Threading.Tasks;
using Windows.Security.Authorization.AppCapabilityAccess;

namespace AIPal.Security
{
    public class PermissionManager
    {
        public bool IsElevated()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public async Task<bool> RequestPermissionAsync(string capability)
        {
            try
            {
                var status = await AppCapability.Create(capability).RequestAccessAsync();
                return status == AppCapabilityAccessStatus.Allowed;
            }
            catch
            {
                return false;
            }
        }

        public bool ValidateCommand(string command)
        {
            // TODO: Implement command validation and sanitization
            return !string.IsNullOrWhiteSpace(command);
        }
    }
}
