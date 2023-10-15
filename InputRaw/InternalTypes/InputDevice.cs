using System;

namespace JeopardyKing.InputRaw
{
    internal record InputDevice(IntPtr DeviceId, Enumerations.RawInputDeviceType Type);
}
