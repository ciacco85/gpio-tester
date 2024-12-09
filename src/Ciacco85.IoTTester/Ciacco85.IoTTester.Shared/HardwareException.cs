using System;

namespace Ciacco85.IoTTester.Shared;

public class HardwareException : Exception
{
    public HardwareDevice HardwareDevice { get; }

    public HardwareException(string message, Exception inner) : base(message, inner) { }
    public HardwareException(string message, Exception inner, HardwareDevice device)
        : base(message, inner) => HardwareDevice = device;
    public HardwareException(string message) : base(message) { }
    public HardwareException(string message, HardwareDevice device) : base(message) => HardwareDevice = device;
}

public enum HardwareDevice
{
    Unknown = 0,
    GPIOController = 1,
    PN532 = 2,
    I2C = 3
}
