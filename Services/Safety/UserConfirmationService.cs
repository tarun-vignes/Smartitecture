using System.Threading.Tasks;

namespace Smartitecture.Services.Safety
{
    public class UserConfirmationService
    {
        public async Task<bool> RequestConfirmationAsync(string summary)
        {
            // Placeholder: integrate with UI prompt to confirm
            await Task.CompletedTask;
            return false; // default to safe
        }
    }
}

