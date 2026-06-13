namespace SkylarkBimbleStreet;

using System;
using Microsoft.Xna.Framework;

internal sealed class Stage
{
    public readonly string Name;
    public readonly Vector2 PlayerStart;
    public readonly Rectangle ExitBounds;
    public readonly Rectangle BusStopBounds;
    public readonly Rectangle HospitalBounds;
    public readonly Color BackgroundColor;
    public readonly Rectangle[] Walls;
    public readonly Rectangle[] TicketPieces;
    public readonly Rectangle[] Gems;
    public readonly Hazard[] Hazards;

    public Stage(
        string name,
        Vector2 playerStart,
        Rectangle exitBounds,
        Rectangle busStopBounds,
        Rectangle hospitalBounds,
        Color backgroundColor,
        Rectangle[] walls,
        Rectangle[] collectibles,
        Hazard[] hazards)
    {
        Name = name;
        PlayerStart = playerStart;
        ExitBounds = exitBounds;
        BusStopBounds = busStopBounds;
        HospitalBounds = hospitalBounds;
        BackgroundColor = backgroundColor;
        Walls = walls;
        var ticketPieceIndexes = ChooseTicketPieceIndexes(collectibles.Length);
        TicketPieces = new Rectangle[ticketPieceIndexes.Length];
        Gems = new Rectangle[collectibles.Length - ticketPieceIndexes.Length];

        var ticketPieceIndex = 0;
        var gemIndex = 0;
        for (var i = 0; i < collectibles.Length; i++)
        {
            if (ticketPieceIndex < ticketPieceIndexes.Length && i == ticketPieceIndexes[ticketPieceIndex])
            {
                TicketPieces[ticketPieceIndex] = collectibles[i];
                ticketPieceIndex++;
                continue;
            }

            Gems[gemIndex] = collectibles[i];
            gemIndex++;
        }
        Hazards = hazards;
    }

    private static int[] ChooseTicketPieceIndexes(int collectibleCount)
    {
        var ticketPieceCount = Math.Min(3, collectibleCount);
        if (ticketPieceCount == 0)
        {
            return [];
        }

        if (ticketPieceCount == 1)
        {
            return [0];
        }

        if (ticketPieceCount == 2)
        {
            return [0, collectibleCount - 1];
        }

        return [0, collectibleCount / 2, collectibleCount - 1];
    }
}
