using DMS.Application.DTOs.Games;
using DMS.Application.Interfaces;
using DMS.Domain.Exceptions;

namespace DMS.Application.UseCases.Games;

public sealed class SubmitGameScoreUseCase(
    IGameScoreRepository scores,
    IUserRepository users,
    IClock clock,
    ListGameLeaderboardUseCase listLeaderboard)
{
    public async Task<SubmitGameScoreResult> ExecuteAsync(
        Guid userId,
        int score,
        CancellationToken ct = default)
    {
        if (score < 0)
            throw new DomainException("Rezultat igre nije ispravan.");

        var user = await users.FindByIdAsync(userId, ct)
            ?? throw new DomainException("Korisnik nije pronađen.");
        var playerName = $"{user.FirstName} {user.LastName}".Trim();

        var saved = await scores.TrySaveTopScoreAsync(
            userId,
            playerName,
            score,
            clock.UtcNow,
            leaderboardSize: 10,
            ct);
        var leaderboard = await listLeaderboard.ExecuteAsync(ct);

        return new SubmitGameScoreResult(saved, leaderboard);
    }
}
