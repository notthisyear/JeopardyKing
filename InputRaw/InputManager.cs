using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Interop;
using JeopardyKing.InputRaw.InternalTypes;
using static JeopardyKing.InputRaw.Enumerations;

namespace JeopardyKing.InputRaw
{
    internal class InputManager
    {
        public enum KeyEvent
        {
            KeyUp,
            KeyDown
        };

        public enum PropagationMode
        {
            None,
            OnlyMappedKeys,
            All
        }

        public record KeyboardEvent(KeyEvent Event, int PlayerId = 0);

        #region Public properties
        public bool InputHookSet { get; private set; } = false;
        #endregion

        #region Private fields
        private readonly ConcurrentQueue<KeyboardEvent> _eventQueue;
        private readonly Dictionary<long, DeviceInfoBase> _knownKeyboards;
        private PropagationMode _propagationMode;
        #endregion

        public InputManager(ConcurrentQueue<KeyboardEvent> eventQueue)
        {
            _eventQueue = eventQueue;
            _knownKeyboards = new();
            SetPropagationMode(PropagationMode.None);
        }

        #region Public methods
        public void SetPropagationMode(PropagationMode mode)
        {
            _propagationMode = mode;
        }

        public bool TryEnumerateKeyboardDevices(out string errorMessage)
        {
            WinApiWrapper.TryGetAllInputDevices(out var devices, out errorMessage);

            if (!string.IsNullOrEmpty(errorMessage))
                return false;

            var keyboards = devices.Where(x => x.Type == Enumerations.RawInputDeviceType.RIM_TYPEKEYBOARD);
            List<DeviceInfoBase> deviceInfo = new();
            foreach (var keyboard in keyboards)
            {
                if (WinApiWrapper.TryGetDeviceInfoForDevice(keyboard, out var info, out errorMessage) && info != default)
                    deviceInfo.Add(info);
                else
                    break;
            }

            if (string.IsNullOrEmpty(errorMessage) && deviceInfo.Any())
            {
                _knownKeyboards.Clear();
                foreach (var device in deviceInfo)
                    _knownKeyboards.Add(device.Device.DeviceId, device);
            }

            return string.IsNullOrEmpty(errorMessage);
        }

        public bool TryRegisterWindowForKeyboardnput(IntPtr windowHandle, out string errorMessage)
        {
            var success = WinApiWrapper.TryRegisterInputDevice(windowHandle,
                UsagePageAndIdBase.GetGenericDesktopControlUsagePageAndFlag(HidGenericDesktopControls.HID_USAGE_GENERIC_KEYBOARD),
                out errorMessage,
                DeviceRegistrationModeFlag.RIDEV_NOLEGACY, DeviceRegistrationModeFlag.RIDEV_INPUTSINK);

            if (success)
            {
                var source = HwndSource.FromHwnd(windowHandle);
                source.AddHook(InputEventHook);
                InputHookSet = true;
            }
            return success;
        }
        #endregion

        #region Private methods
        private IntPtr InputEventHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WinApiWrapper.WM_INPUT)
            {
                if (_propagationMode == PropagationMode.None)
                    return IntPtr.Zero;

                if (WinApiWrapper.TryGetRawInput(lParam, out var input, out _))
                {
                    if (input!.Header.DeviceType == RawInputDeviceType.RIM_TYPEKEYBOARD)
                    {
                        var keyboardInput = (RawKeyboardInput)input;
                    }
                }
                handled = true;
            }
            return IntPtr.Zero;
        }
        #endregion
    }
}
