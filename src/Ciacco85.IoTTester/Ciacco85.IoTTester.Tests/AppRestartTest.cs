using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Ciacco85.IoTTester.Tests;

public class AppRestartTest : TestBed<TestProjectFixture>
{
    public AppRestartTest(ITestOutputHelper testOutputHelper, TestProjectFixture fixture)
       : base(testOutputHelper, fixture)
    {
    }

    [Theory]
    [InlineData(500, 1000, 5)]
    [InlineData(1000, 500, 5)]
    [InlineData(10, 1000, 5)]
    [InlineData(1000, 10, 5)]
    public async Task Run(int periodicTimerDelayMs, int delayMs, int durationS)
    {
        try
        {
            SemaphoreSlim _semaphoreSlim = new(1, 1);
            PeriodicTimer RunTimer = new(TimeSpan.FromMilliseconds(periodicTimerDelayMs));
            TimeSpan untilRestart = TimeSpan.FromSeconds(durationS);
            using CancellationTokenSource cts = new(untilRestart);

            while (!cts.IsCancellationRequested && await RunTimer.WaitForNextTickAsync(cts.Token))
            {
                try
                {
                    await _semaphoreSlim.WaitAsync(cts.Token);
                    await Task.Delay(delayMs, cts.Token);
                    _testOutputHelper.WriteLine($"{DateTimeOffset.Now.ToString()} Running...");
                }
                catch(Exception ex)
                {
                    _testOutputHelper.WriteLine($"{DateTimeOffset.Now.ToString()} Catch in while {ex.ToString()}");
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }
            _testOutputHelper.WriteLine("Exited while without exception");
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
            _testOutputHelper.WriteLine($"{DateTimeOffset.Now.ToString()} External catch {ex.ToString()}");
            if (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                _testOutputHelper.WriteLine("OK");
            }
            else
            {
                _testOutputHelper.WriteLine("KO");
                Assert.True(false);
                throw;
            }
        }
        Assert.True(true);
    }
}
