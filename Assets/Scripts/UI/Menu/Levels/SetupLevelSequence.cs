using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace UI.Menu.Levels
{
    public class SetupLevelSequence
    {
        public LevelSequenceConfig CurrentLevelSequence { get; private set; }

        public async UniTask Setup(int currentLevel)
        {
            if (currentLevel <= 5)
            {
                Debug.Log("Load Levels1-5");
                await LoadLevels("Levels1-5");
            }
            else
            {
                Debug.Log("Load Levels6-10");
                await LoadLevels("Levels6-10");
            }
        }

        private async UniTask LoadLevels(string key)
        {
            AsyncOperationHandle<LevelSequenceConfig> levels = Addressables.LoadAssetAsync<LevelSequenceConfig>(key);
            await levels.ToUniTask();
            if (levels.Status == AsyncOperationStatus.Succeeded)
            {
                CurrentLevelSequence = levels.Result;
                Addressables.Release(levels);
            }
        }
    }
}