using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UI.Menu.Levels
{
    public class SetupLevelSequence
    {
        public LevelSequenceConfig AllLevels { get; private set; }

        public async UniTask Setup()
        {
            // Грузим один конфиг, в котором лежат все 15 уровней
            AsyncOperationHandle<LevelSequenceConfig> handle = Addressables.LoadAssetAsync<LevelSequenceConfig>("AllLevels");
            await handle.ToUniTask();

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                AllLevels = handle.Result;
                Debug.Log($"[SetupLevelSequence] Загружено уровней: {AllLevels.LevelSequence.Count}");
            }
            else
            {
                Debug.LogError("[SetupLevelSequence] Не удалось загрузить AllLevels. Проверь Address в окне Addressables!");
            }
        }
    }
}