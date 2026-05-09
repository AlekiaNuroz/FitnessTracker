using FitnessTracker.Data;
using FitnessTracker.DTOs;
using FitnessTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace FitnessTracker.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users");

        // Get user by username
        group.MapGet("/{username}", async (string username, AppDbContext db) =>
        {
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            return user is null
                ? Results.NotFound($"User '{username}' not found.")
                : Results.Ok(ToDto(user));
        });

        // Create user
        group.MapPost("/", async (CreateUserRequest request, AppDbContext db) =>
        {
            if (await db.Users.AnyAsync(u => u.Username == request.Username))
                return Results.Conflict($"Username '{request.Username}' already exists.");

            var user = new User
            {
                Username = request.Username,
                CurrentWeightLbs = request.CurrentWeightLbs,
                GoalWeightLbs = request.GoalWeightLbs,
                HeightInches = request.HeightInches
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return Results.Created($"/api/users/{user.Username}", ToDto(user));
        });

        // Update user stats
        group.MapPut("/{username}", async (string username, UpdateUserRequest request, AppDbContext db) =>
        {
            var user = await db.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user is null)
                return Results.NotFound($"User '{username}' not found.");

            user.CurrentWeightLbs = request.CurrentWeightLbs;
            user.GoalWeightLbs = request.GoalWeightLbs;
            user.HeightInches = request.HeightInches;

            await db.SaveChangesAsync();
            return Results.Ok(ToDto(user));
        });
    }

    private static UserDto ToDto(User user) => new(
        user.Id,
        user.Username,
        user.CreatedAt,
        user.CurrentWeightLbs,
        user.GoalWeightLbs,
        user.HeightInches);
}

// Request records
record CreateUserRequest(
    string Username,
    double CurrentWeightLbs,
    double GoalWeightLbs,
    double HeightInches);

record UpdateUserRequest(
    double CurrentWeightLbs,
    double GoalWeightLbs,
    double HeightInches);