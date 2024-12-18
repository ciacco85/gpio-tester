﻿using Ciacco85.IoTTester.Shared;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Ciacco85.IoTTester.Tests;

public class Pn532Test : TestBed<TestProjectFixture>
{
    private readonly BadgerSettings _options;
    public Pn532Test(ITestOutputHelper testOutputHelper, TestProjectFixture fixture)
       : base(testOutputHelper, fixture) => _options = _fixture.GetService<IOptions<BadgerSettings>>(_testOutputHelper)!.Value;

    //[Theory]
    //[InlineData(1, 2)]
    [Fact]
    public async Task Test()
    {
        var manager = _fixture.GetService<IPn532ManagerTest>(_testOutputHelper)!;
        var calculatedValue = await manager.Test();
        Assert.False(calculatedValue.Equals(Memory<byte>.Empty));

    }
}
