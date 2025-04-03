using System.Collections.Generic;
using Game.Tiles;
using UnityEngine;
using VContainer;
using Grid = Game.GridSystem.Grid;

namespace Game.Board
{
    public class GameBoard : MonoBehaviour
    {
        [SerializeField] private GameObject _tilePrefab;
        [SerializeField] private TileConfig _tileConfig;
        private readonly List<Tile> _tilesToRefill = new List<Tile>();
        private Grid _grid;

        private void Start()
        {
            _grid.SetupGrid(10,10);
            CreateBoard();
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
                    if(_grid.GetValue(x,y)) continue;
                    var tile = Instantiate(_tilePrefab, transform);
                    tile.transform.position = _grid.GridToWorld(x, y);
                    var tileComponent = tile.GetComponent<Tile>();
                    tileComponent.SetTileConfig(_tileConfig);
                    _grid.SetValue(x,y,tileComponent);
                }
            }
        }

        [Inject]
        private void Construct(Grid grid)
        {
            _grid = grid;
        }
    }
}