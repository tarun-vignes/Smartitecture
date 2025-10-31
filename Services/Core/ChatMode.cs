namespace Smartitecture.Services.Core
{
    /// <summary>
    /// Enumeration of available AI chat modes in Smartitecture
    /// </summary>
    public enum ChatMode
    {
        /// <summary>
        /// LUMEN - General AI assistant with OS integration, file search, and web research
        /// </summary>
        Lumen = 0,

        /// <summary>
        /// FORTIS - Security-focused AI with threat detection and system protection
        /// </summary>
        Fortis = 1,

        /// <summary>
        /// NEXA - Performance optimization AI with hardware control and system monitoring
        /// </summary>
        Nexa = 2
    }

    /// <summary>
    /// Extension methods for ChatMode enum
    /// </summary>
    public static class ChatModeExtensions
    {
        public static string GetDisplayName(this ChatMode mode)
        {
            return mode switch
            {
                ChatMode.Lumen => "LUMEN",
                ChatMode.Fortis => "FORTIS",
                ChatMode.Nexa => "NEXA",
                _ => "UNKNOWN"
            };
        }

        public static string GetIcon(this ChatMode mode)
        {
            return mode switch
            {
                ChatMode.Lumen => "💡",
                ChatMode.Fortis => "🛡️",
                ChatMode.Nexa => "⚡",
                _ => "❓"
            };
        }

        public static string GetColor(this ChatMode mode)
        {
            return mode switch
            {
                ChatMode.Lumen => "#3B82F6",    // Blue
                ChatMode.Fortis => "#EF4444",   // Red
                ChatMode.Nexa => "#8B5CF6",     // Purple
                _ => "#6B7280"                   // Gray
            };
        }

        public static string GetDescription(this ChatMode mode)
        {
            return mode switch
            {
                ChatMode.Lumen => "General assistant with file search, web research, and OS integration",
                ChatMode.Fortis => "Security expert with threat detection, malware scanning, and system protection",
                ChatMode.Nexa => "Performance optimizer with hardware control, system monitoring, and automation",
                _ => "Unknown mode"
            };
        }
    }
}
