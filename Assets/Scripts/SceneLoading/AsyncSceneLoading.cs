using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace SceneLoading
{
    public class AsyncSceneLoading : IAsyncSceneLoading
    {
        private readonly Dictionary<string, SceneInstance> _loadedScenes = new Dictionary<string, SceneInstance>();

        private LoadingView _loadingView;
        private CancellationTokenSource _cts;

        public AsyncSceneLoading(LoadingView loadingView)
        {
            _loadingView = loadingView;
        }


        public async UniTask LoadAsync(string sceneName)
        {
            _cts = new CancellationTokenSource();
            LoadingDone(false);
            
            // For show LoadScreen Could be Commented
            await UniTask.Delay(TimeSpan.FromSeconds(2f), _cts.IsCancellationRequested);
            var loadedScene = await Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Additive).WithCancellation(_cts.Token);
            SceneManager.SetActiveScene(loadedScene.Scene);
            if (_loadedScenes.ContainsKey(sceneName) == false)
            {
                _loadedScenes.Add(sceneName, loadedScene);
            }
            _cts?.Cancel();
        }

        public async UniTask UnLoadAsync(string sceneName)
        {
            _cts = new CancellationTokenSource();
            var sceneInstance = _loadedScenes[sceneName];
            await Addressables.UnloadSceneAsync(sceneInstance).WithCancellation(_cts.Token).AsUniTask();
            _loadedScenes.Remove(sceneName);
            _cts?.Cancel();
        }

        public void LoadingDone(bool value)
        {
            _loadingView.SetActiveScreen(value != true);
        }
    }
}