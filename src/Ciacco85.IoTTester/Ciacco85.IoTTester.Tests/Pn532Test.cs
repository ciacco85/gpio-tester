using Ciacco85.IoTTester.Shared;
using Iot.Device.Card.Mifare;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        Parallel.For(0, 10000, new ParallelOptions(), async a =>
        {
            var calculatedValue = await manager.Test();
            _testOutputHelper.WriteLine($"Iteration {a}; Data: {(calculatedValue.IsEmpty ? "N/A" : BitConverter.ToString(calculatedValue.ToArray()))}");
        });

        Assert.True(true);
    }
}
