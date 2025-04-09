using Game.Tiles;
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

        public GameObject FXPrefab => _FXPrefab;

        public GameObject TilePrefab => _tilePrefab;
        public TileSetConfig TileSetConfig => _tileSetConfig;
        public GameObject TileBlank => _tileBlank;
        public TileConfig BlankConfig => _blankConfig;
    }
}