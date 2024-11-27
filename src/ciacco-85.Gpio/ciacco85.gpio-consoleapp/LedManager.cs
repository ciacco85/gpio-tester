using System.Device.Gpio;

namespace GLEMA.IoT.Badger.Device;

public interface ILedManager
{
    bool Init();
    Task StartupProcedure();
    void SwitchLed(int color, bool ledOn = false);
    void SwitchLed(int[] colors, bool ledOn = false);
    void SwitchAllLed(bool ledOn = false);
    Task SwitchLedForTime(int color, TimeSpan duration);
    Task SwitchLedForTime(int[] color, TimeSpan duration);
}

public class LedManager : ILedManager
{
    private readonly Lazy<GpioController> gpioController;
    private GpioController _gpioController { get => gpioController.Value; }

    public LedManager()
    {
        gpioController = new(() => new GpioController());
    }
    public bool Init()
    {
        try
        {            
            var info = _gpioController.QueryComponentInformation();
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }

    }
    public async Task StartupProcedure()
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(100);
        for (int i = 0; i < 10; i++)
        {
            await SwitchLedForTime((int)Color.Red, timeSpan);
            await SwitchLedForTime((int)Color.Yellow, timeSpan);
            await SwitchLedForTime((int)Color.Green, timeSpan);
            await SwitchLedForTime((int)Color.Yellow, timeSpan);
        }
    }

    public async Task SwitchLedForTime(int color, TimeSpan duration)
    {
        SwitchLed(color, true);
        await Task.Delay(duration);
        SwitchLed(color);
    }

    public async Task SwitchLedForTime(int[] color, TimeSpan duration)
    {
        SwitchLed(color, true);
        await Task.Delay(duration);
        SwitchLed(color);
    }

    public void SwitchAllLed(bool ledOn = false)
    {
        SwitchLed(new int[] { (int)Color.Green, (int)Color.Yellow, (int)Color.Red }, ledOn);
    }

    public void SwitchLed(int[] colors, bool ledOn = false)
    {
        foreach (var color in colors)
        {
            SwitchLed(color, ledOn);
        }
    }

    public void SwitchLed(int color, bool ledOn = false)
    {
        var pin = _gpioController.OpenPin(color, PinMode.Output);
        pin.Write(ledOn ? PinValue.High : PinValue.Low);
    }
}

public enum Color
{
    Red = 17,
    Yellow = 27,
    Green = 22,
}