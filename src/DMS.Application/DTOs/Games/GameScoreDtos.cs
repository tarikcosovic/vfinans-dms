namespace DMS.Application.DTOs.Games;

public sealed record GameScoreDto(
    int Rank,
    string PlayerName,
    int Score);

public sealed record SubmitGameScoreResult(
    bool Saved,
    IReadOnlyList<GameScoreDto> Leaderboard);
