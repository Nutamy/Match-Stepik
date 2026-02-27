using Game.Tiles;
using Levels;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ResourcesLoading
{
    public class GameResourcesLoader : MonoBehaviour
    {
        [SerializeField] private GameObject _tilePrefab;
        [SerializeField] private GameObject _tileBlank;
        [SerializeField] private TileConfig _blankConfig;
        [SerializeField] private TileSetConfig _tileSetConfig;
        [SerializeField] private GameObject _FXPrefab;

        [SerializeField] private List<TileSetMapping> _allTileSets;
        [Serializable]
        public class TileSetMapping
        {
            public TileSets Type;
            public TileSetConfig Config;
        }

        public TileSetConfig GetTileSet(TileSets type)
        {
            var mapping = _allTileSets.FirstOrDefault(m => m.Type == type);
            if (mapping != null)
            {
                return mapping.Config;
            }

            Debug.LogError($"[GameResourcesLoader] Набор {type} не найден в списке!");
            return _allTileSets.FirstOrDefault()?.Config;
        }

        public GameObject FXPrefab => _FXPrefab;

        public GameObject TilePrefab => _tilePrefab;
        public TileSetConfig TileSetConfig => _tileSetConfig;
        public GameObject TileBlank => _tileBlank;
        public TileConfig BlankConfig => _blankConfig;
    }
}