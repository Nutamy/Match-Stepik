using SceneLoading;
using UI.Menu.Levels;
using VContainer.Unity;

namespace UI.Menu
{
    public class MenuEntryPoint : IInitializable
    {
        private IAsyncSceneLoading _sceneLoading;
        private SetupLevelSequence _setupLevelSequence;
        private LevelSequenceView _levelSequenceView;

        public MenuEntryPoint(IAsyncSceneLoading sceneLoading, SetupLevelSequence setupLevelSequence, LevelSequenceView levelSequenceView)
        {
            _sceneLoading = sceneLoading;
            _setupLevelSequence = setupLevelSequence;
            _levelSequenceView = levelSequenceView;
        }

        public async void Initialize()
        {
            // setup level sequence
            await _setupLevelSequence.Setup(1);
            // music menu
            _levelSequenceView.SetupButtonsView(1);
            // loading is done
            _sceneLoading.LoadingDone(true);
            // await animation
            // buttons enable
        }
    }
}