using System;

namespace Smartitecture.Core.Models
{
    /// <summary>
    /// Event arguments for process output events
    /// </summary>
    public class ProcessOutputEventArgs : EventArgs
    {
        public string Data { get; }

        public ProcessOutputEventArgs(string data)
        {
            Data = data;
        }
    }
}
