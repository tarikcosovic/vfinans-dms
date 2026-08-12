using DMS.Application.DTOs.Games;
using DMS.Application.Interfaces;

namespace DMS.Application.UseCases.Games;

public sealed class ListGameLeaderboardUseCase(IGameScoreRepository scores)
{
    public async Task<IReadOnlyList<GameScoreDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var leaderboard = await scores.ListTopAsync(10, ct);
        return leaderboard
            .Select((entry, index) => new GameScoreDto(index + 1, entry.PlayerName, entry.Score))
            .ToList();
    }
}
