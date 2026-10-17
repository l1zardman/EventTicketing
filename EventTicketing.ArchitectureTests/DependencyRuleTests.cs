using EventTicketing.Application.Events;
using EventTicketing.Domain.Events;
using NetArchTest.Rules;
using Xunit;

namespace EventTicketing.ArchitectureTests;

public class DependencyRuleTests
{
    [Fact]
    public void Domain_has_no_outward_dependencies()
    {
        var result = Types.InAssembly(typeof(Event).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "EventTicketing.Application",
                "EventTicketing.Infrastructure",
                "EventTicketing.Api",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Application_depends_only_on_domain()
    {
        var result = Types.InAssembly(typeof(GetEventAvailabilityQueryHandler).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "EventTicketing.Infrastructure",
                "EventTicketing.Api",
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(TestResult result)
        => "Offending types: " + string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>());
}

// Additional rules demonstrated in Module 10 (kept in a separate class for clarity).
public class LayerIndependenceTests
{
    [Fact]
    public void Domain_does_not_depend_on_dependency_injection()
    {
        var result = NetArchTest.Rules.Types.InAssembly(typeof(EventTicketing.Domain.Events.Event).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.Extensions.DependencyInjection")
            .GetResult();

        Xunit.Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Application_does_not_depend_on_web()
    {
        var result = NetArchTest.Rules.Types.InAssembly(typeof(EventTicketing.Application.Events.ReserveTicketsHandler).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        Xunit.Assert.True(result.IsSuccessful);
    }
}
