using Game.MatchTiles;
using UnityEngine;

namespace Game.Score
{
    public class ScoreCalculator
    {
        private GameProgress _gameProgress;

        public ScoreCalculator(GameProgress gameProgress)
        {
            _gameProgress = gameProgress;
        }

        public void CalculateScoreToAdd(MatchDirection matchDirection)
        {
            if (matchDirection == MatchDirection.Horizontal || matchDirection == MatchDirection.Vertical)
            {
                _gameProgress.AddScore(20);
                Debug.Log("+20");
            }
            if (matchDirection == MatchDirection.LongHorizontal || matchDirection == MatchDirection.LongVertical)
            {
                _gameProgress.AddScore(50);
                Debug.Log("+50");
            }

            if (matchDirection == MatchDirection.Multiply)
            {
                _gameProgress.AddScore(100);
                Debug.Log("+100");
            }
        }
    }
}