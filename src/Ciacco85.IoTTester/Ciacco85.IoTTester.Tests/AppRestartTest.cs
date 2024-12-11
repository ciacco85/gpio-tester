using Ciacco85.IoTTester.Shared;
using Iot.Device.Card.Mifare;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Ciacco85.IoTTester.Tests;

public class AppRestartTest : TestBed<TestProjectFixture>
{
    public AppRestartTest(ITestOutputHelper testOutputHelper, TestProjectFixture fixture)
       : base(testOutputHelper, fixture)
    {
    }
    
    [Fact]
    public async Task ConcurrentPn532Access()
    {
        try
        {
            PeriodicTimer RunTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
            TimeSpan untilRestart = TimeSpan.FromSeconds(5);
            using CancellationTokenSource cts = new(untilRestart);

            while (!cts.IsCancellationRequested && await RunTimer.WaitForNextTickAsync(cts.Token))
            {
                _testOutputHelper.WriteLine($"{DateTimeOffset.Now.ToString()} Running...");
            }
            Assert.True(true);
        }
        catch (TaskCanceledException ex)
        {
            _testOutputHelper.WriteLine($"{DateTimeOffset.Now.ToString()} {ex.ToString()}");
            Assert.True(true);
        }
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine($"{DateTimeOffset.Now.ToString()} {ex.ToString()}");
            Assert.True(false);
            throw;          
        }
        //catch (Exception ex)
        //{
        //    _testOutputHelper.WriteLine($"{DateTimeOffset.Now.ToString()} {ex.ToString()}");
        //    if (ex is TaskCanceledException)// || ex is OperationCanceledException))
        //    {
        //        _testOutputHelper.WriteLine("Trowing...");
        //        Assert.True(false);
        //        throw;
        //    }

        //}
        
        
    }
}
