using System.Collections.Generic;
using Game.Tiles;

namespace Game.MatchTiles
{
    // Добавляем это прямо здесь!
    public enum MatchDirection
    {
        Horizontal,
        Vertical,
        LongHorizontal,
        LongVertical,
        FiveInARow,
        Multiply,
        None
    }

    public class MatchResult
    {
        public List<Tile> ConnectedTiles { get; }
        public MatchDirection Direction { get; }

        public MatchResult(List<Tile> connectedTiles, MatchDirection direction)
        {
            ConnectedTiles = connectedTiles;
            Direction = direction;
        }
    }
}