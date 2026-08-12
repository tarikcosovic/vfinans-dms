namespace DMS.Domain.Entities;

public sealed class GameScore
{
    private GameScore() { }

    public Guid UserId { get; private set; }
    public string PlayerName { get; private set; } = string.Empty;
    public int Score { get; private set; }
    public DateTime AchievedAtUtc { get; private set; }

    public static GameScore Create(
        Guid userId,
        string playerName,
        int score,
        DateTime achievedAtUtc) =>
        new()
        {
            UserId = userId,
            PlayerName = playerName.Trim(),
            Score = score,
            AchievedAtUtc = achievedAtUtc,
        };

    public void SetBestScore(int score, string playerName, DateTime achievedAtUtc)
    {
        Score = score;
        PlayerName = playerName.Trim();
        AchievedAtUtc = achievedAtUtc;
    }
}
