namespace Ciacco85.IoTTester.Shared
{
    public record BadgerSettings
    {
        //public string? DeviceId { get; set; }
        public bool? UseSerial { get; set; }
        public string? SerialPort { get; set; }
        public bool? UseDelay { get; set; }
        public int? DelayMs { get; set; }
        
    }
}
