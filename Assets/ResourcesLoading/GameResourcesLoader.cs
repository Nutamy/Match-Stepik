using Cysharp.Threading.Tasks;
using Data;
using Game.Tiles;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ResourcesLoading
{
    // Обычный класс, больше не висит на сцене!
    public class GameResourcesLoader : System.IDisposable
    {
        private const string TILE_PREFAB_KEY = "TilePrefab";
        private const string BLANK_PREFAB_KEY = "TileBlank";
        private const string FX_PREFAB_KEY = "FXPrefab";
        private const string BLANK_CONFIG_KEY = "BlankTile";

        public GameObject TilePrefab { get; private set; }
        public GameObject TileBlank { get; private set; }
        public GameObject FXPrefab { get; private set; }
        public TileConfig BlankConfig { get; private set; }

        public List<TileConfig> CurrentTileSet { get; private set; }
        public TileSetConfig LoadedConfig { get; private set; }

        private readonly GameData _gameData;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        // Обычный конструктор — VContainer сам прокинет сюда GameData
        public GameResourcesLoader(GameData gameData)
        {
            _gameData = gameData;
        }

        public async UniTask Load()
        {
            if (_gameData?.CurrentLevel == null)
            {
                Debug.LogError("[GameResourcesLoader] GameData или CurrentLevel пустые!");
                return;
            }

            // Загружаем префабы и текущий сет параллельно
            await UniTask.WhenAll(LoadBaseResources(), LoadCurrentSet());
        }

        private async UniTask LoadBaseResources()
        {
            var tileTask = Addressables.LoadAssetAsync<GameObject>(TILE_PREFAB_KEY).ToUniTask(cancellationToken: _cts.Token);
            var blankTask = Addressables.LoadAssetAsync<GameObject>(BLANK_PREFAB_KEY).ToUniTask(cancellationToken: _cts.Token);
            var fxTask = Addressables.LoadAssetAsync<GameObject>(FX_PREFAB_KEY).ToUniTask(cancellationToken: _cts.Token);
            var configTask = Addressables.LoadAssetAsync<TileConfig>(BLANK_CONFIG_KEY).ToUniTask(cancellationToken: _cts.Token);

            var (t, b, f, c) = await UniTask.WhenAll(tileTask, blankTask, fxTask, configTask);

            TilePrefab = t;
            TileBlank = b;
            FXPrefab = f;
            BlankConfig = c;
        }

        private async UniTask LoadCurrentSet()
        {
            string setKey = _gameData.CurrentLevel.TileSets.ToString();
            var handle = Addressables.LoadAssetAsync<TileSetConfig>(setKey);

            await handle.ToUniTask(cancellationToken: _cts.Token);

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                LoadedConfig = handle.Result;
                CurrentTileSet = handle.Result.Set.ToList();
            }
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            // Здесь же можно выгружать ассеты из памяти, если нужно:
            // Addressables.Release(LoadedConfig);
        }
    }
}