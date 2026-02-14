namespace Smartitecture.Services.Safety
{
    public sealed class UserConfirmationService
    {
        public bool IsConfirmed(string toolName)
        {
            // UI hook will prompt the user. Default to false until wired.
            return false;
        }
    }
}
