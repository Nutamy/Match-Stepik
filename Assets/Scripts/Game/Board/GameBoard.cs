using System;
using System.Collections.Generic;
using Animations;
using Game.GridSystem;
using Game.MatchTiles;
using Game.Tiles;
using Game.Utils;
using Input;
using Levels;
using UnityEngine;
using VContainer;
using Grid = Game.GridSystem.Grid;

namespace Game.Board
{
    public class GameBoard : MonoBehaviour
    {
        private readonly List<Tile> _tilesToRefill = new List<Tile>();
        
        private Grid _grid;
        private TilePool _tilePool;
        private IAnimation _animation;
        private MatchFinder _matchFinder;
        private BlankTileSetup _blankTileSetup;


        private void ClickTest()
        {
            Debug.Log("Test");
        }

        public void CreateBoard()
        {
            FillBoard();
            int counter = 0;
            while (_matchFinder.CheckBoardForMatches(_grid))
            {
                ClearBoard();
                FillBoard();
                counter++;
            }
            _matchFinder.ClearTilesToRemove();
            Debug.Log("Board has been generated " + counter + " times");
            RevealTiles();
        }

        private void RevealTiles()
        {
            foreach (var tile in _tilesToRefill)
            {
                var gameObjectTile = tile.gameObject;
                _animation.Reveal(gameObjectTile, 1f);
            }
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

        private void ClearBoard()
        {
            if(_tilesToRefill == null) return;
            foreach (var tile in _tilesToRefill)
            {
                _grid.SetValue(tile.transform.position, null);
                tile.gameObject.SetActive(false);
            }
            _tilesToRefill.Clear();
        }

        [Inject]
        private void Construct(Grid grid, TilePool tilePool, IAnimation animation, MatchFinder matchFinder, BlankTileSetup blankTileSetup)
        {
            _grid = grid;
            _tilePool = tilePool;
            _animation = animation;
            _matchFinder = matchFinder;
            _blankTileSetup = blankTileSetup;
        }
    }
}