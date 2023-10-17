using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using WindowsNativeRawInputWrapper;
using WindowsNativeRawInputWrapper.Types;

namespace JeopardyKing.Communication
{
    internal class InputManager : IDisposable
    {
        public enum KeyEvent
        {
            None,
            KeyUp,
            KeyDown
        };

        public enum PropagationMode
        {
            None,
            OnlyMappedKeys,
            All
        }

        public record KeyboardEvent(KeyEvent Event, long SourceId, RawKeyboardInput.KeyboardScanCode Key, int PlayerId);

        #region Public properties
        public bool InputHookSet { get; private set; } = false;
        #endregion

        #region Private fields
        private readonly ConcurrentQueue<KeyboardEvent> _eventQueue;
        private readonly Dictionary<long, KeyboardDeviceInfo> _knownKeyboards;
        private readonly Dictionary<long, Dictionary<RawKeyboardInput.KeyboardScanCode, int>> _playerButtonMap;
        private readonly Dictionary<int, List<(long deviceId, RawKeyboardInput.KeyboardScanCode key)>> _playerMappingLookup;
        private PropagationMode _propagationMode;
        private bool _disposedValue;
        private readonly UdpBroadcastListener _listener;
        #endregion

        public InputManager(ConcurrentQueue<KeyboardEvent> eventQueue)
        {
            _eventQueue = eventQueue;
            _knownKeyboards = new();
            _playerButtonMap = new();
            _playerMappingLookup = new();
            _listener = new(HandleNewMessage);
            SetPropagationMode(PropagationMode.None);
        }

        #region Public methods
        public void SetPropagationMode(PropagationMode mode)
        {
            _propagationMode = mode;
        }

        public bool PlayerHasMapping(int playerId)
            => _playerMappingLookup.ContainsKey(playerId);

        public bool MappingAvailable(long deviceId, RawKeyboardInput.KeyboardScanCode key)
            => !(_playerButtonMap.TryGetValue(deviceId, out var keys) && keys.Select(x => x.Key).Contains(key));

        public bool TryEnumerateKeyboardDevices(out string errorMessage)
        {
            WinApiWrapper.TryGetAllInputDevices(out var devices, out errorMessage);

            if (!string.IsNullOrEmpty(errorMessage))
                return false;

            var keyboards = devices.Where(x => x.Type == DeviceType.Keyboard);
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
                    _knownKeyboards.Add(device.Device.DeviceId, (KeyboardDeviceInfo)device);
            }

            return string.IsNullOrEmpty(errorMessage);
        }

        public bool TryGetInformationForKeyboard(long keyboardId, out KeyboardDeviceInfo? deviceInfo)
            => _knownKeyboards.TryGetValue(keyboardId, out deviceInfo);

        public bool TryAddPlayerKeyMapping(int playerId, long deviceId, RawKeyboardInput.KeyboardScanCode key)
        {
            if (!MappingAvailable(deviceId, key))
                return false;

            if (!_playerButtonMap.ContainsKey(deviceId))
                _playerButtonMap.Add(deviceId, new());
            _playerButtonMap[deviceId].Add(key, playerId);

            if (!_playerMappingLookup.ContainsKey(playerId))
                _playerMappingLookup.Add(playerId, new());

            _playerMappingLookup[playerId].Add((deviceId, key));
            return true;
        }

        public void RemovePlayerKeyMappingIfNeeded(int playerId)
        {
            if (!_playerMappingLookup.ContainsKey(playerId))
                return;

            var mappings = _playerMappingLookup[playerId];
            foreach (var (deviceId, key) in mappings)
            {
                if (_playerButtonMap[deviceId].TryGetValue(key, out var id) && id == playerId)
                {
                    _playerButtonMap[deviceId].Remove(key);
                    if (!_playerButtonMap[deviceId].Any())
                        _playerButtonMap.Remove(deviceId);
                }
            }
            _playerMappingLookup.Remove(playerId);
        }
        #endregion

        #region Private methods
        private void HandleNewMessage(byte[] msg)
        {
            if (_propagationMode == PropagationMode.None)
                return;

            if (!TryDeserialize(msg, out var keyEvent, out var deviceId, out var key))
                return;

            var hasMapping = KeyHasMapping(deviceId, key);
            var playerId = hasMapping ? _playerButtonMap[deviceId][key] : -1;

            if (_propagationMode == PropagationMode.All || hasMapping)
            {
                KeyboardEvent newEvent = new(keyEvent, deviceId, key, playerId);
                _eventQueue.Enqueue(newEvent);
            }
        }

        private bool KeyHasMapping(long deviceId, RawKeyboardInput.KeyboardScanCode scanCode)
            => _playerButtonMap.ContainsKey(deviceId) && _playerButtonMap[deviceId].ContainsKey(scanCode);

        private static bool TryDeserialize(ReadOnlySpan<byte> msg, out KeyEvent keyEvent, out long deviceId, out RawKeyboardInput.KeyboardScanCode key)
        {
            keyEvent = KeyEvent.None;
            deviceId = 0;
            key = RawKeyboardInput.KeyboardScanCode.UnknownScanCode;

            if (msg.Length != 11)
                return false;

            keyEvent = msg[0] == 0x00 ? KeyEvent.KeyUp : KeyEvent.KeyDown;
            for (var i = 0; i < 8; i++)
                deviceId |= ((long)msg[i + 1]) << (i * 8);
            key = (RawKeyboardInput.KeyboardScanCode)((ushort)(msg[9] | (msg[10] << 8)));
            return true;
        }
        #endregion

        #region Disposal
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                    _listener.Dispose();
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
