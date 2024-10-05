using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LandsatReflectance.Backend.Tests.Services;

// TODO: Fix tests

[TestFixture]
[Description("Tests the registered 'ITargetService'. Tests do not include any 'join' table operations.")]
public class TargetServiceTestsWithoutJoin
{
    private WebApplicationFactory<Program> m_factory = null!;
    
    
    [SetUp]
    public void Setup()
    {
        m_factory = new WebApplicationFactory<Program>();
        
        using var serviceScope = m_factory.Services.CreateScope();
        var targetService = serviceScope.ServiceProvider.GetRequiredService<ITargetService>();
        TestContext.WriteLine($"Testing: {targetService.GetType()}");
    }
    
    
#region Without Join

    [Test]
    public void TestAddGetAndRemoveTarget()
    {
        using var serviceScope = m_factory.Services.CreateScope();
        var targetService = serviceScope.ServiceProvider.GetRequiredService<ITargetService>();
        
#pragma warning disable CS0618 // Type or member is obsolete
        targetService.ClearAll();
#pragma warning restore CS0618 // Type or member is obsolete
        
        var passwordHasher = new PasswordHasher<string>();
        var email1 = "amir@gmail.com";
        var passwordHash1 = passwordHasher.HashPassword(email1, "somePassword1234!");
        var user = new User
        {
            Email = email1,
            PasswordHash = passwordHash1
        };

        var target1 = new Target
        {
            Path = 14,
            Row = 28,
            Latitude = 45.45764113167635,
            Longitude = -73.65145134943786,
            NotificationOffset = TimeSpan.FromHours(1)
        };
        
        var target2 = new Target
        {
            Path = 15,
            Row = 28,
            Latitude = 45.45764113167635,
            Longitude = -73.65145134943786,
            NotificationOffset = TimeSpan.FromHours(1)
        };
        
        targetService.AddTargets([(user, target1), (user, target2)]);

        /*
        var retrievedTarget1 = targetService.GetTargets(GetTargetGuidFilter(target1)).ToList();
        Assert.That(retrievedTarget1, Has.Count.EqualTo(1));
        AssertTargetEquals(target1, retrievedTarget1[0]);

        var retrievedTarget2 = targetService.GetTargets(GetTargetGuidFilter(target2)).ToList();
        Assert.That(retrievedTarget2, Has.Count.EqualTo(1));
        AssertTargetEquals(target2, retrievedTarget2[0]);
        
        
        var removedTarget = targetService.TryRemoveTarget(GetTargetGuidFilter(target1));
        Assert.That(removedTarget, Is.Not.Null);
        AssertTargetEquals(target1, removedTarget);
         */
    }
    
    [Test]
    public void TestEditTarget()
    {
        using var serviceScope = m_factory.Services.CreateScope();
        var targetService = serviceScope.ServiceProvider.GetRequiredService<ITargetService>();
        
#pragma warning disable CS0618 // Type or member is obsolete
        targetService.ClearAll();
#pragma warning restore CS0618 // Type or member is obsolete
        
        var passwordHasher = new PasswordHasher<string>();
        var email1 = "amir@gmail.com";
        var passwordHash1 = passwordHasher.HashPassword(email1, "somePassword1234!");
        var user = new User
        {
            Email = email1,
            PasswordHash = passwordHash1
        };

        var target = new Target
        {
            Path = 14,
            Row = 28,
            Latitude = 45.45764113167635,
            Longitude = -73.65145134943786,
            NotificationOffset = TimeSpan.FromHours(1)
        };
        
        targetService.AddTargets([(user, target)]);

        /*
        Assert.That(retrievedTarget, Is.Not.Null);
        Assert.That(retrievedTarget.Path, Is.EqualTo(2));
        Assert.That(target.Guid, Is.EqualTo(retrievedTarget.Guid));
        Assert.That(target.Row, Is.EqualTo(retrievedTarget.Row));
        Assert.That(target.Latitude, Is.EqualTo(retrievedTarget.Latitude).Within(0.1).Percent);
        Assert.That(target.Longitude, Is.EqualTo(retrievedTarget.Longitude).Within(0.1).Percent);
        Assert.That(target.NotificationOffset, Is.EqualTo(retrievedTarget.NotificationOffset));
         */
    }
    
    [Test]
    public void TestDifferentScopes()
    {
        var serviceScope = m_factory.Services.CreateScope();
        var targetService = serviceScope.ServiceProvider.GetRequiredService<ITargetService>();
        
#pragma warning disable CS0618 // Type or member is obsolete
        targetService.ClearAll();
#pragma warning restore CS0618 // Type or member is obsolete
        
        var passwordHasher = new PasswordHasher<string>();
        var email1 = "amir@gmail.com";
        var passwordHash1 = passwordHasher.HashPassword(email1, "somePassword1234!");
        var user = new User
        {
            Email = email1,
            PasswordHash = passwordHash1
        };

        var target = new Target
        {
            Path = 14,
            Row = 28,
            Latitude = 45.45764113167635,
            Longitude = -73.65145134943786,
            NotificationOffset = TimeSpan.FromHours(1)
        };
        
        targetService.AddTargets([(user, target)]);
        
        serviceScope.Dispose();
        serviceScope = m_factory.Services.CreateScope();
        targetService = serviceScope.ServiceProvider.GetRequiredService<ITargetService>();
        
        /*
        var retrievedTarget = targetService.GetTargets(GetTargetGuidFilter(target)).ToList();
        Assert.That(retrievedTarget, Has.Count.EqualTo(1));
        AssertTargetEquals(target, retrievedTarget[0]);
         */
    }
    
#endregion


#region With Join

    [Test]
    public void TestGetAllTargetsForUser()
    {
        using var serviceScope = m_factory.Services.CreateScope();
        var targetService = serviceScope.ServiceProvider.GetRequiredService<ITargetService>();

    #pragma warning disable CS0618 // Type or member is obsolete
        targetService.ClearAll();
    #pragma warning restore CS0618 // Type or member is obsolete

        var passwordHasher = new PasswordHasher<string>();
        var email1 = "amir@gmail.com";
        var passwordHash1 = passwordHasher.HashPassword(email1, "somePassword1234!");
        var user = new User
        {
            Email = email1,
            PasswordHash = passwordHash1
        };

        var target1 = new Target
        {
            Path = 14,
            Row = 28,
            Latitude = 45.45764113167635,
            Longitude = -73.65145134943786,
            NotificationOffset = TimeSpan.FromHours(1)
        };

        var target2 = new Target
        {
            Path = 15,
            Row = 28,
            Latitude = 45.45764113167635,
            Longitude = -73.65145134943786,
            NotificationOffset = TimeSpan.FromHours(1)
        };

        targetService.AddTargets([(user, target1), (user, target2)]);


        var retrievedTargets = targetService.GetTargets(_ => true, guid => guid == user.Guid).ToList();
        Assert.That(retrievedTargets, Has.Count.EqualTo(2));
        AssertTargetEquals(target1, retrievedTargets.First(target => target.Guid == target1.Guid));
        AssertTargetEquals(target2, retrievedTargets.First(target => target.Guid == target2.Guid));
    }

#endregion 


    [TearDown]
    public void Dispose()
    {
        using var serviceScope = m_factory.Services.CreateScope();
        var targetService = serviceScope.ServiceProvider.GetRequiredService<ITargetService>();
        
#pragma warning disable CS0618 // Type or member is obsolete
        targetService.ClearAll();
#pragma warning restore CS0618 // Type or member is obsolete
        
        m_factory.Dispose();
    }
    
    
    Predicate<Target> GetTargetGuidFilter(Target target) => 
        (innerTarget => innerTarget.Guid == target.Guid);

    private static void AssertTargetEquals(Target @base, Target toCompare)
    {
        Assert.That(@base.Guid, Is.EqualTo(toCompare.Guid));
        Assert.That(@base.Path, Is.EqualTo(toCompare.Path));
        Assert.That(@base.Row, Is.EqualTo(toCompare.Row));
        Assert.That(@base.Latitude, Is.EqualTo(toCompare.Latitude).Within(0.1).Percent);
        Assert.That(@base.Longitude, Is.EqualTo(toCompare.Longitude).Within(0.1).Percent);
        Assert.That(@base.NotificationOffset, Is.EqualTo(toCompare.NotificationOffset));
    }
}