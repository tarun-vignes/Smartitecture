using System;
using System.Collections.Generic;
using System.Linq;
using Smartitecture.Services.Interfaces;

namespace Smartitecture.Services.Core
{
    public sealed class AIModeRouter
    {
        private readonly Dictionary<AIModeType, IAIMode> _modes;

        public AIModeRouter(IEnumerable<IAIMode> modes)
        {
            _modes = modes.ToDictionary(m => m.Mode, m => m);
        }

        public IAIMode GetMode(AIModeType mode)
        {
            if (_modes.TryGetValue(mode, out var found))
            {
                return found;
            }

            return _modes.Values.First();
        }

        public AIModeType ResolveModeFromModel(string? modelName)
        {
            var value = (modelName ?? string.Empty).ToLowerInvariant();
            if (value.Contains("fortis") || value.Contains("security") || value.Contains("defense"))
            {
                return AIModeType.Fortis;
            }

            if (value.Contains("nexa") || value.Contains("performance") || value.Contains("optimiz"))
            {
                return AIModeType.Nexa;
            }

            return AIModeType.Lumen;
        }
    }
}
