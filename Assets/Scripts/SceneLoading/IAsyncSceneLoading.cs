using Cysharp.Threading.Tasks;

namespace SceneLoading
{
    public interface IAsyncSceneLoading
    {
        UniTask LoadAsync(string sceneName);
        UniTask UnLoadAsync(string sceneName);
        void LoadingDone(bool value);
    }
}