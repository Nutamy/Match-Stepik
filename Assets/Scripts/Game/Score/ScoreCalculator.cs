using Game.MatchTiles;

namespace Game.Score
{
    public class ScoreCalculator
    {
        private readonly GameProgress _gameProgress;

        public ScoreCalculator(GameProgress gameProgress)
        {
            _gameProgress = gameProgress;
        }

        public static int PointsFor(MatchDirection matchDirection)
        {
            switch (matchDirection)
            {
                case MatchDirection.Horizontal:
                case MatchDirection.Vertical:
                    return 20;
                case MatchDirection.LongHorizontal:
                case MatchDirection.LongVertical:
                    return 50;
                case MatchDirection.Multiply:
                    return 100;
                case MatchDirection.FiveInARow:
                    return 150;
                default:
                    return 0;
            }
        }

        /// <summary>Начисляет очки за группу и возвращает, сколько начислено.</summary>
        public int CalculateScoreToAdd(MatchDirection matchDirection)
        {
            int points = PointsFor(matchDirection);
            if (points > 0) _gameProgress.AddScore(points);
            return points;
        }
    }
}
