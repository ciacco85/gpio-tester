using Ciacco85.IoTTester.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Microsoft.DependencyInjection;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace Ciacco85.IoTTester.Tests;

public class TestProjectFixture : TestBedFixture
{
    protected override void AddServices(IServiceCollection services, IConfiguration? configuration) => services
        .AddSingleton<IPn532ManagerTest, Pn532ManagerTest>()
        .AddLogging()
        .Configure<BadgerSettings>(config => configuration?.GetSection("BadgerSettings").Bind(config))        
        ;


    protected override ValueTask DisposeAsyncCore() => new();

    protected override IEnumerable<TestAppSettings> GetTestAppSettings()
    {
        yield return new() { Filename = "appsettings.json", IsOptional = false };
    }

}
