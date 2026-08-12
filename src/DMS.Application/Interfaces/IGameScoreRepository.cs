using DMS.Domain.Entities;

namespace DMS.Application.Interfaces;

public interface IGameScoreRepository
{
    Task<IReadOnlyList<GameScore>> ListTopAsync(int count, CancellationToken ct = default);
    Task<bool> TrySaveTopScoreAsync(
        Guid userId,
        string playerName,
        int score,
        DateTime achievedAtUtc,
        int leaderboardSize,
        CancellationToken ct = default);
}
