using SceneLoading;
using UI.Menu.Levels;
using VContainer.Unity;

namespace UI.Menu
{
    public class MenuEntryPoint : IInitializable
    {
        private IAsyncSceneLoading _sceneLoading;
        private SetupLevelSequence _setupLevelSequence;

        public MenuEntryPoint(IAsyncSceneLoading sceneLoading, SetupLevelSequence setupLevelSequence)
        {
            _sceneLoading = sceneLoading;
            _setupLevelSequence = setupLevelSequence;
        }

        public async void Initialize()
        {
            // setup level sequence
            await _setupLevelSequence.Setup(1);
            // music menu
            // loading is done
            _sceneLoading.LoadingDone(true);
            // await animation
            // buttons enable
        }
    }
}