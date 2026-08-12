using DMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DMS.Infrastructure.Persistence.Configurations;

internal sealed class GameScoreConfiguration : IEntityTypeConfiguration<GameScore>
{
    public void Configure(EntityTypeBuilder<GameScore> builder)
    {
        builder.ToTable("game_scores");

        builder.HasKey(s => s.UserId);
        builder.Property(s => s.UserId).HasColumnName("user_id");
        builder.Property(s => s.PlayerName)
            .HasColumnName("player_name")
            .HasMaxLength(201)
            .IsRequired();
        builder.Property(s => s.Score).HasColumnName("score").IsRequired();
        builder.Property(s => s.AchievedAtUtc).HasColumnName("achieved_at_utc").IsRequired();

        builder.HasIndex(s => new { s.Score, s.AchievedAtUtc })
            .HasDatabaseName("ix_game_scores_rank");

        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<GameScore>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
