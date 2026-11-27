using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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
        private const int MaxBoardGenerations = 100;
        private const float ShuffleDuration = 0.35f;
        private readonly System.Random _random = new System.Random();

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
            CreateBoardMask();
            FillBoard();
            int counter = 0;
            // Стартовое поле: без готовых совпадений и хотя бы с одним ходом
            while ((_matchFinder.CheckBoardForMatches(_grid) || !HasPossibleMove()) && counter < MaxBoardGenerations)
            {
                ClearBoard();
                FillBoard();
                counter++;
            }
            if (_matchFinder.CheckBoardForMatches(_grid) || !HasPossibleMove()) ArrangeWithoutAnimation();
            _matchFinder.ClearTilesToRemove();
            Debug.Log("Board has been generated " + counter + " times");
            RevealTiles();
        }

        // Запас вокруг поля: выбранная фишка увеличивается до 1.2 и не должна обрезаться
        private const float MaskMargin = 0.15f;

        private void CreateBoardMask()
        {
            if (transform.Find("BoardMask") != null) return;

            var maskObject = new GameObject("BoardMask", typeof(SpriteMask));
            maskObject.transform.SetParent(transform, false);
            maskObject.transform.localPosition = _grid.GridToWorld(0, 0)
                + new Vector3((_grid.Width - 1) / 2f, (_grid.Height - 1) / 2f, 0f);
            maskObject.transform.localScale = new Vector3(_grid.Width + MaskMargin * 2f, _grid.Height + MaskMargin * 2f, 1f);

            // Квадрат 1x1 единица из белой текстуры
            var texture = Texture2D.whiteTexture;
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), texture.width);
            maskObject.GetComponent<SpriteMask>().sprite = sprite;
        }

        public bool HasPossibleMove() =>
            MoveFinder.HasPossibleMove(_grid.Width, _grid.Height, GetKey);

        // Ход для подсказки: пара соседних клеток, обмен которых даёт линию
        public bool TryFindMove(out Vector2Int from, out Vector2Int to) =>
            MoveFinder.TryFindMove(_grid.Width, _grid.Height, GetKey, out from, out to);

        /// <summary>
        /// Перемешивает подвижные фишки так, чтобы не было готовых совпадений и появился ход.
        /// Если перестановкой не получилось (мало разных фишек), меняет типы фишек.
        /// </summary>
        public async UniTask ShuffleAsync()
        {
            foreach (var pair in Rearrange())
            {
                _animation.MoveTile(pair.Key, _grid.GridToWorld(pair.Value.x, pair.Value.y), Ease.InOutQuad);
            }
            await UniTask.Delay(TimeSpan.FromSeconds(ShuffleDuration), cancellationToken: destroyCancellationToken)
                .SuppressCancellationThrow();
        }

        private void ArrangeWithoutAnimation()
        {
            foreach (var pair in Rearrange())
            {
                pair.Key.transform.localPosition = _grid.GridToWorld(pair.Value.x, pair.Value.y);
            }
        }

        // Переставляет фишки в сетке; возвращает фишки, сменившие клетку, и их новые клетки
        private Dictionary<Tile, Vector2Int> Rearrange()
        {
            var moved = new Dictionary<Tile, Vector2Int>();
            var map = MoveFinder.TryShuffle(_grid.Width, _grid.Height, GetKey, _random);
            if (map != null)
            {
                var snapshot = new Dictionary<Vector2Int, Tile>();
                foreach (var source in map.Values) snapshot[source] = _grid.GetValue(source.x, source.y);
                foreach (var pair in map)
                {
                    var tile = snapshot[pair.Value];
                    _grid.SetValue(pair.Key.x, pair.Key.y, tile);
                    if (pair.Key != pair.Value) moved[tile] = pair.Key;
                }
                return moved;
            }

            // Перестановок не хватило: перекрашиваем фишки без троек, пока не появится ход
            for (int attempt = 0; attempt < MaxBoardGenerations; attempt++)
            {
                for (int x = 0; x < _grid.Width; x++)
                for (int y = 0; y < _grid.Height; y++)
                {
                    var tile = _grid.GetValue(x, y);
                    if (MatchFinder.GetMatchKey(tile) == null) continue;
                    tile.SetTileConfig(_tilePool.GetRandomTileConfig());
                    AvoidLineWithPlaced(tile, x, y);
                }

                if (HasPossibleMove()) break;
            }
            return moved;
        }

        private void AvoidLineWithPlaced(Tile tile, int x, int y)
        {
            const int maxRerolls = 50;
            for (int i = 0; i < maxRerolls && MoveFinder.CreatesLineWithPlaced(GetKey, _grid.Width, _grid.Height, x, y, MatchFinder.GetMatchKey(tile)); i++)
            {
                tile.SetTileConfig(_tilePool.GetRandomTileConfig());
            }
        }

        private object GetKey(int x, int y) => MatchFinder.GetMatchKey(_grid.GetValue(x, y));

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
                        // Сразу подбираем тип без тройки с соседями слева и снизу:
                        // стартовое поле никогда не содержит готовых совпадений
                        AvoidLineWithPlaced(tile, x, y);
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