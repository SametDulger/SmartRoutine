using SmartRoutine.Domain.Entities;
using SmartRoutine.Domain.Enums;
using SmartRoutine.Domain.ValueObjects;

namespace SmartRoutine.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context, bool isDevelopment = false)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if data already exists
        if (context.Users.Any())
        {
            return; // Database has been seeded
        }

        if (isDevelopment)
        {
            await SeedDevelopmentDataAsync(context);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedDevelopmentDataAsync(ApplicationDbContext context)
    {
        // Seed test user
        var email = Email.Create("test@smartroutine.com");
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("Test123456");
        var testUser = new User(email, passwordHash);

        context.Users.Add(testUser);
        await context.SaveChangesAsync(); // Save user first to get ID

        // Seed test routines
        var morningRoutine = new Routine(
            testUser.Id,
            "Sabah Egzersizi", 
            "Günlük 30 dakika kardio",
            RoutineRepeatType.Daily,
            null,
            null);

        var eveningRoutine = new Routine(
            testUser.Id,
            "Kitap Okuma", 
            "Her akşam en az 30 sayfa",
            RoutineRepeatType.Daily,
            null,
            null);

        var weeklyRoutine = new Routine(
            testUser.Id,
            "Haftalık Planlama", 
            "Gelecek hafta için planlama yap",
            RoutineRepeatType.Weekly,
            null,
            null);

        context.Routines.AddRange(morningRoutine, eveningRoutine, weeklyRoutine);
        await context.SaveChangesAsync(); // Save routines to get IDs

        // Seed some routine logs for the past few days
        var completionDates = new[]
        {
            DateTime.UtcNow.AddDays(-3),
            DateTime.UtcNow.AddDays(-2),
            DateTime.UtcNow.AddDays(-1)
        };

        foreach (var date in completionDates)
        {
            var morningLog = new RoutineLog(morningRoutine.Id, "Tamamlandı");
            var eveningLog = new RoutineLog(eveningRoutine.Id, "Güzel kitaptı");
            
            context.RoutineLogs.AddRange(morningLog, eveningLog);
        }
    }
} 