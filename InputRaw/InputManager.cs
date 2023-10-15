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

        public record KeyboardEvent(KeyEvent Event, long SourceId, KeyboardScanCode Key, int PlayerId);

        #region Public properties
        public bool InputHookSet { get; private set; } = false;
        #endregion

        #region Private fields
        private readonly ConcurrentQueue<KeyboardEvent> _eventQueue;
        private readonly Dictionary<long, DeviceInfoBase> _knownKeyboards;
        private readonly Dictionary<long, Dictionary<KeyboardScanCode, int>> _playerButtonMap;
        private PropagationMode _propagationMode;
        #endregion

        public InputManager(ConcurrentQueue<KeyboardEvent> eventQueue)
        {
            _eventQueue = eventQueue;
            _knownKeyboards = new();
            _playerButtonMap = new();
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

        public bool TryRegisterWindowForKeyboardInput(IntPtr windowHandle, out string errorMessage)
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

        public bool TryAddPlayerKeyMapping(int playerId, long sourceId, KeyboardScanCode key)
        {
            if (!_playerButtonMap.ContainsKey(sourceId))
                _playerButtonMap.Add(sourceId, new());

#pragma warning disable CA1854 // This warning is emitted incorrectly
            if (_playerButtonMap[sourceId].ContainsKey(key))
#pragma warning restore CA1854
                return false;

            _playerButtonMap[sourceId].Add(key, playerId);
            return true;
        }

        public void RemovePlayerKeyMappingIfNeeded(int playerId, long sourceId, KeyboardScanCode key)
        {
            if (!_playerButtonMap.ContainsKey(sourceId))
                return;

            if (_playerButtonMap[sourceId].TryGetValue(key, out var id) && id == playerId)
                _playerButtonMap[sourceId].Remove(key);
        }
        #endregion

        #region Private methods
        private IntPtr InputEventHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WinApiWrapper.WM_INPUT)
            {
                if (_propagationMode == PropagationMode.None)
                    return IntPtr.Zero;

                if (!WinApiWrapper.TryGetRawInput(lParam, out var input, out _))
                    return IntPtr.Zero;

                if (input is RawKeyboardInput keyboardInput)
                {
                    var hasMapping = KeyHasMapping(keyboardInput.Header.DeviceHandle, keyboardInput.ScanCode);
                    var playerId = hasMapping ? _playerButtonMap[keyboardInput.Header.DeviceHandle][keyboardInput.ScanCode] : -1;

                    if (_propagationMode == PropagationMode.All || hasMapping)
                    {
                        _eventQueue.Enqueue(
                            new KeyboardEvent(keyboardInput.IsKeyUp ? KeyEvent.KeyUp : KeyEvent.KeyDown,
                            keyboardInput.Header.DeviceHandle,
                            keyboardInput.ScanCode,
                            playerId));

                        if (_propagationMode == PropagationMode.OnlyMappedKeys)
                            handled = true;
                    }
                }
            }
            return IntPtr.Zero;
        }

        private bool KeyHasMapping(long deviceId, KeyboardScanCode scanCode)
            => _playerButtonMap.ContainsKey(deviceId) && _playerButtonMap[deviceId].ContainsKey(scanCode);
        #endregion
    }
}
