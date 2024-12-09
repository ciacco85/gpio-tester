using Ciacco85.IoTTester.Shared;
using Iot.Device.Card.Mifare;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Ciacco85.IoTTester.Tests;

public class Pn532Test : TestBed<TestProjectFixture>
{
    private readonly BadgerSettings _options;
    public Pn532Test(ITestOutputHelper testOutputHelper, TestProjectFixture fixture)
       : base(testOutputHelper, fixture)
    {
        _options = _fixture.GetService<IOptions<BadgerSettings>>(_testOutputHelper)!.Value;
    }

    //[Theory]
    //[InlineData(1, 2)]
    [Fact]
    public async Task Test()
    {
        var manager = _fixture.GetService<IPn532ManagerTest>(_testOutputHelper)!;
        //await Parallel.ForEachAsync(Enumerable.Range(0, 100), new ParallelOptions(), async (index, token) =>
        //{
        //    var calculatedValue = await manager.Test();
        //    _testOutputHelper.WriteLine($"Iteration {index}; Data: {(calculatedValue.IsEmpty ? "N/A" : BitConverter.ToString(calculatedValue.ToArray()))}");
        //});

        ConcurrentBag<Task<Memory<byte>>> tasks = new();

        Parallel.For(0, 100, async index =>
        {
            tasks.Add(manager.Test());
            //var calculatedValue = await manager.Test();
            //_testOutputHelper.WriteLine($"Iteration {index}; Data: {(calculatedValue.IsEmpty ? "N/A" : BitConverter.ToString(calculatedValue.ToArray()))}");

        });
        await Task.WhenAll(tasks);
        foreach (var task in tasks)
        {
            _testOutputHelper.WriteLine($"Iteration {task.Id}; Data: {(task.Result.IsEmpty ? "N/A" : BitConverter.ToString(task.Result.ToArray()))}");
        }
        Assert.True(true);
    }
}
