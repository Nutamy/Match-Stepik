using System.Collections.Generic;
using Game.GridSystem;
using Game.Tiles;
using Game.Utils;
using UnityEngine;
using VContainer;
using Grid = Game.GridSystem.Grid;

namespace Game.Board
{
    public class GameBoard : MonoBehaviour
    {
        
        [SerializeField] private TileConfig _tileConfig;
        [SerializeField] private bool isDebug = true;
        private readonly List<Tile> _tilesToRefill = new List<Tile>();
        private Grid _grid;
        private BlankTileSetup _blankTileSetup;
        private TilePool _tilePool;
        private SetupCamera _setupCamera;
        private GameDebug _gameDebug;
        

        private void Start()
        {
            _grid.SetupGrid(10,10);
            _blankTileSetup.SetupBlanks(_grid.Width, _grid.Height);
            CreateBoard();
            _setupCamera.SetCamera(_grid.Width, _grid.Height, false);
            if (isDebug)
            {
                _gameDebug.ShowDebug(transform);
            }
        }

        public void CreateBoard()
        {
            FillBoard();
        }

        private void FillBoard()
        {
            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    if (_blankTileSetup.Blanks[x, y])
                    {
                        if(_grid.GetValue(x,y)) continue;
                        var blankTile = _tilePool.CreateBlankTile(_grid.GridToWorld(x, y), transform);
                        _grid.SetValue(x,y,blankTile);
                    }
                    
                    else 
                    {
                        var tile = _tilePool.GetTile(_grid.GridToWorld(x, y), transform);
                        _grid.SetValue(x,y,tile);
                        tile.gameObject.SetActive(true);
                        _tilesToRefill.Add(tile);
                    }
                }
            }
        }

        [Inject]
        private void Construct(Grid grid, SetupCamera setupCamera, TilePool tilePool, GameDebug gameDebug, BlankTileSetup blankTileSetup)
        {
            _grid = grid;
            _setupCamera = setupCamera;
            _tilePool = tilePool;
            _gameDebug = gameDebug;
            _blankTileSetup = blankTileSetup;
        }
    }
}