using System.Collections.Generic;
using Game.Tiles;
using UnityEngine;

namespace Levels
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Configs/LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Grid")] 
        [SerializeField] private List<BlankTile> _blankTilesLayout;
        [SerializeField] private int _width;
        [SerializeField] private int _height;        
        
        [Header("Level")]
        [SerializeField] private int _goalScore;
        [SerializeField] private int _moves;
        [SerializeField] private TileSets _tileSets;
        [SerializeField] private int _levelNumber;

        public List<BlankTile> BlankTilesLayout => _blankTilesLayout;
        public int Width => _width;
        public int Height => _height;
        public int GoalScore => _goalScore;
        public int Moves => _moves;
        public TileSets TileSets => _tileSets;
        public int LevelNumber => _levelNumber;
    }

    public enum TileSets
    {
        Kingdom,
        Setkz01,
        Gem
    }
}