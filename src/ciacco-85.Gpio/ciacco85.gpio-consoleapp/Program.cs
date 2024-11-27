using GLEMA.IoT.Badger.Device;

namespace ciacco85.gpio_consoleapp;

internal static class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Hello, GPIO Controller!");
            ILedManager ledManager = new LedManager();
            if (ledManager.Init())
            {
                ledManager.SwitchAllLed();
                await ledManager.StartupProcedure();
                Console.WriteLine("Finish");
            }
            else
            {
                Console.WriteLine("Failed Init");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
