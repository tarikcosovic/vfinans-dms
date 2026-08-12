using System.Data;
using DMS.Application.Interfaces;
using DMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DMS.Infrastructure.Persistence.Repositories;

internal sealed class GameScoreRepository(DmsDbContext db) : IGameScoreRepository
{
    public async Task<IReadOnlyList<GameScore>> ListTopAsync(
        int count,
        CancellationToken ct = default) =>
        await db.GameScores
            .AsNoTracking()
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.AchievedAtUtc)
            .Take(count)
            .ToListAsync(ct);

    public async Task<bool> TrySaveTopScoreAsync(
        Guid userId,
        string playerName,
        int score,
        DateTime achievedAtUtc,
        int leaderboardSize,
        CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        var existing = await db.GameScores.FirstOrDefaultAsync(s => s.UserId == userId, ct);

        if (existing is not null)
        {
            if (score <= existing.Score)
            {
                return false;
            }

            existing.SetBestScore(score, playerName, achievedAtUtc);
        }
        else
        {
            var cutoff = await db.GameScores
                .OrderByDescending(s => s.Score)
                .ThenBy(s => s.AchievedAtUtc)
                .Skip(leaderboardSize - 1)
                .Select(s => (int?)s.Score)
                .FirstOrDefaultAsync(ct);

            if (cutoff.HasValue && score <= cutoff.Value)
            {
                return false;
            }

            await db.GameScores.AddAsync(
                GameScore.Create(userId, playerName, score, achievedAtUtc),
                ct);
        }

        await db.SaveChangesAsync(ct);

        var displaced = await db.GameScores
            .OrderByDescending(s => s.Score)
            .ThenBy(s => s.AchievedAtUtc)
            .Skip(leaderboardSize)
            .ToListAsync(ct);
        if (displaced.Count > 0)
        {
            db.GameScores.RemoveRange(displaced);
            await db.SaveChangesAsync(ct);
        }

        await transaction.CommitAsync(ct);
        return true;
    }
}
