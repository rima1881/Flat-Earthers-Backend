using System.Text.Json;
using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LandsatReflectance.Backend.Tests.Services;

[TestFixture]
public class FileUserServiceTests
{
    private WebApplicationFactory<Program> m_factory = null!;
    
    
    [SetUp]
    public void Setup()
    {
        m_factory = new WebApplicationFactory<Program>();
        
        using var serviceScope = m_factory.Services.CreateScope();
        var userService = serviceScope.ServiceProvider.GetRequiredService<IUserService>();
        TestContext.WriteLine($"Testing: {userService.GetType()}");
    }

    [Test]
    public async Task TestAddGetAndRemoveUser()
    {
        using var serviceScope = m_factory.Services.CreateScope();
        var userService = serviceScope.ServiceProvider.GetRequiredService<IUserService>();
        
#pragma warning disable CS0618 // Type or member is obsolete
        userService.ClearAll();
#pragma warning restore CS0618 // Type or member is obsolete

        var passwordHasher = new PasswordHasher<string>();
        var email1 = "amir@gmail.com";
        var passwordHash1 = passwordHasher.HashPassword(email1, "somePassword1234!");
        var user1 = new User
        {
            Email = email1,
            PasswordHash = passwordHash1
        };
        await userService.AddUser(user1);

        var retrievedUser1 = await userService.TryGetUser(email1);
        Assert.That(retrievedUser1, Is.Not.Null);
        Assert.That(retrievedUser1.Email, Is.EqualTo(email1));
        Assert.That(retrievedUser1.PasswordHash, Is.EqualTo(passwordHash1));

        var deletedUser = await userService.TryRemoveUser(email1);
        Assert.That(deletedUser, Is.Not.Null);
        Assert.That(deletedUser.Email, Is.EqualTo(email1));
        Assert.That(deletedUser.PasswordHash, Is.EqualTo(passwordHash1));
    }
    
    [Test]
    public async Task TestEditUser()
    {
        using var serviceScope = m_factory.Services.CreateScope();
        var userService = serviceScope.ServiceProvider.GetRequiredService<IUserService>();
        
#pragma warning disable CS0618 // Type or member is obsolete
        userService.ClearAll();
#pragma warning restore CS0618 // Type or member is obsolete

        var passwordHasher = new PasswordHasher<string>();
        var email1 = "amir@gmail.com";
        var passwordHash1 = passwordHasher.HashPassword(email1, "somePassword1234!");
        var user1 = new User
        {
            Email = email1,
            PasswordHash = passwordHash1
        };
        await userService.AddUser(user1);
        
        var passwordHash2 = passwordHasher.HashPassword(email1, "someNewPassword!2345");
        await userService.TryEditUser(email1, user => user.PasswordHash = passwordHash2);
        
        var retrievedUser1 = await userService.TryGetUser(email1);
        Assert.That(retrievedUser1, Is.Not.Null);
        Assert.That(retrievedUser1.Email, Is.EqualTo(email1));
        Assert.That(retrievedUser1.PasswordHash, Is.EqualTo(passwordHash2));
    }
    
    [Test]
    public async Task TestDifferentScopes()
    {
        var serviceScope = m_factory.Services.CreateScope();
        var userService = serviceScope.ServiceProvider.GetRequiredService<IUserService>();
        
#pragma warning disable CS0618 // Type or member is obsolete
        userService.ClearAll();
#pragma warning restore CS0618 // Type or member is obsolete

        var passwordHasher = new PasswordHasher<string>();
        var email1 = "amir@gmail.com";
        var passwordHash1 = passwordHasher.HashPassword(email1, "somePassword1234!");
        var user1 = new User
        {
            Email = email1,
            PasswordHash = passwordHash1
        };
        await userService.AddUser(user1);
        
        serviceScope.Dispose();
        serviceScope = m_factory.Services.CreateScope();
        userService = serviceScope.ServiceProvider.GetRequiredService<IUserService>();

        var retrievedUser1 = await userService.TryGetUser(email1);
        Assert.That(retrievedUser1, Is.Not.Null);
        Assert.That(retrievedUser1.Email, Is.EqualTo(email1));
        Assert.That(retrievedUser1.PasswordHash, Is.EqualTo(passwordHash1));
    }

    [TearDown]
    public void Dispose()
    {
        using var serviceScope = m_factory.Services.CreateScope();
        var userService = serviceScope.ServiceProvider.GetRequiredService<IUserService>();
        
#pragma warning disable CS0618 // Type or member is obsolete
        userService.ClearAll();
#pragma warning restore CS0618 // Type or member is obsolete
        
        m_factory.Dispose();
    }
}