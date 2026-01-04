using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Smartitecture.Commands
{
    public class VolumeCommand : ISystemCommand
    {
        public string CommandName => "Volume";

        public string Description => "Controls system volume";

        public bool RequiresElevation => false;

        public Task<bool> ExecuteAsync(string[] parameters)
        {
            if (parameters is null || parameters.Length == 0)
            {
                return Task.FromResult(false);
            }

            var command = parameters[0]?.Trim();
            if (string.IsNullOrEmpty(command))
            {
                return Task.FromResult(false);
            }

            try
            {
                if (!TryGetEndpointVolume(out var endpointVolume))
                {
                    return Task.FromResult(false);
                }

                var context = Guid.Empty;

                switch (command.ToLowerInvariant())
                {
                    case "up":
                        return Task.FromResult(AdjustVolume(endpointVolume, +0.10f, ref context));

                    case "down":
                        return Task.FromResult(AdjustVolume(endpointVolume, -0.10f, ref context));

                    case "mute":
                        return Task.FromResult(ToggleMute(endpointVolume, ref context));

                    default:
                        if (float.TryParse(command, NumberStyles.Float, CultureInfo.InvariantCulture, out var percent)
                            && percent >= 0
                            && percent <= 100)
                        {
                            var scalar = percent / 100f;
                            Marshal.ThrowExceptionForHR(endpointVolume.SetMasterVolumeLevelScalar(scalar, ref context));
                            return Task.FromResult(true);
                        }

                        return Task.FromResult(false);
                }
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        private static bool AdjustVolume(IAudioEndpointVolume endpointVolume, float delta, ref Guid context)
        {
            Marshal.ThrowExceptionForHR(endpointVolume.GetMasterVolumeLevelScalar(out var current));
            var next = Math.Clamp(current + delta, 0f, 1f);
            Marshal.ThrowExceptionForHR(endpointVolume.SetMasterVolumeLevelScalar(next, ref context));
            return true;
        }

        private static bool ToggleMute(IAudioEndpointVolume endpointVolume, ref Guid context)
        {
            Marshal.ThrowExceptionForHR(endpointVolume.GetMute(out var isMuted));
            Marshal.ThrowExceptionForHR(endpointVolume.SetMute(!isMuted, ref context));
            return true;
        }

        private static bool TryGetEndpointVolume(out IAudioEndpointVolume endpointVolume)
        {
            endpointVolume = null!;

            var enumeratorType = Type.GetTypeFromCLSID(new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E"));
            var enumerator = (IMMDeviceEnumerator)Activator.CreateInstance(enumeratorType)!;

            Marshal.ThrowExceptionForHR(
                enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out var device));

            var iid = typeof(IAudioEndpointVolume).GUID;
            Marshal.ThrowExceptionForHR(
                device.Activate(ref iid, CLSCTX.CLSCTX_INPROC_SERVER, IntPtr.Zero, out var endpointVolumeObject));

            endpointVolume = (IAudioEndpointVolume)endpointVolumeObject;
            return true;
        }

        private enum EDataFlow
        {
            eRender = 0,
            eCapture = 1,
            eAll = 2,
        }

        private enum ERole
        {
            eConsole = 0,
            eMultimedia = 1,
            eCommunications = 2,
        }

        [Flags]
        private enum CLSCTX : uint
        {
            CLSCTX_INPROC_SERVER = 0x1,
            CLSCTX_INPROC_HANDLER = 0x2,
            CLSCTX_LOCAL_SERVER = 0x4,
            CLSCTX_REMOTE_SERVER = 0x10,
            CLSCTX_ALL = CLSCTX_INPROC_SERVER | CLSCTX_INPROC_HANDLER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER,
        }

        [ComImport]
        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            int EnumAudioEndpoints(EDataFlow dataFlow, int dwStateMask, out object devices);
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice endpoint);
            int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string pwstrId, out IMMDevice device);
            int RegisterEndpointNotificationCallback(IntPtr client);
            int UnregisterEndpointNotificationCallback(IntPtr client);
        }

        [ComImport]
        [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            int Activate(ref Guid iid, CLSCTX dwClsCtx, IntPtr pActivationParams,
                [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

            int OpenPropertyStore(int stgmAccess, out IntPtr properties);
            int GetId([MarshalAs(UnmanagedType.LPWStr)] out string id);
            int GetState(out int state);
        }

        [ComImport]
        [Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioEndpointVolume
        {
            int RegisterControlChangeNotify(IntPtr notify);
            int UnregisterControlChangeNotify(IntPtr notify);
            int GetChannelCount(out uint channelCount);
            int SetMasterVolumeLevel(float levelDb, ref Guid eventContext);
            int SetMasterVolumeLevelScalar(float level, ref Guid eventContext);
            int GetMasterVolumeLevel(out float levelDb);
            int GetMasterVolumeLevelScalar(out float level);
            int SetChannelVolumeLevel(uint channelNumber, float levelDb, ref Guid eventContext);
            int SetChannelVolumeLevelScalar(uint channelNumber, float level, ref Guid eventContext);
            int GetChannelVolumeLevel(uint channelNumber, out float levelDb);
            int GetChannelVolumeLevelScalar(uint channelNumber, out float level);
            int SetMute([MarshalAs(UnmanagedType.Bool)] bool isMuted, ref Guid eventContext);
            int GetMute(out bool isMuted);
            int GetVolumeStepInfo(out uint step, out uint stepCount);
            int VolumeStepUp(ref Guid eventContext);
            int VolumeStepDown(ref Guid eventContext);
            int QueryHardwareSupport(out uint hardwareSupportMask);
            int GetVolumeRange(out float volumeMindB, out float volumeMaxdB, out float volumeIncrementdB);
        }
    }
}
