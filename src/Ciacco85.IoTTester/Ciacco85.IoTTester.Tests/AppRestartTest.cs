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
            SemaphoreSlim _semaphoreSlim = new(1, 1);
            PeriodicTimer RunTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000));
            TimeSpan untilRestart = TimeSpan.FromSeconds(5);
            using CancellationTokenSource cts = new(untilRestart);

            while (!cts.IsCancellationRequested && await RunTimer.WaitForNextTickAsync(cts.Token))
            {

                try
                {
                    await _semaphoreSlim.WaitAsync(cts.Token);
                    await Task.Delay(2000, cts.Token);
                    _testOutputHelper.WriteLine($"{DateTimeOffset.Now.ToString()} Running...");
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }
            Assert.True(true);
        }
        //catch (TaskCanceledException ex)
        //{
        //    _testOutputHelper.WriteLine("OK");
        //    _testOutputHelper.WriteLine($"{DateTimeOffset.Now.ToString()} {ex.ToString()}");
        //    Assert.True(true);
        //}
        //catch (Exception ex)
        //{
        //    _testOutputHelper.WriteLine("KO");
        //    _testOutputHelper.WriteLine($"{DateTimeOffset.Now.ToString()} {ex.ToString()}");
        //    Assert.True(false);
        //    throw;          
        //}
        catch (Exception ex)
        {
            _testOutputHelper.WriteLine($"{DateTimeOffset.Now.ToString()} {ex.ToString()}");
            if (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                _testOutputHelper.WriteLine("OK");
                Assert.True(true);
            }
            else
            {
                _testOutputHelper.WriteLine("KO");
                Assert.True(false);
                throw;
            }
        }
    }
}
