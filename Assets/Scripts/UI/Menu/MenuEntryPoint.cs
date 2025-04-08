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
        private MenuView _menuView;

        public MenuEntryPoint(IAsyncSceneLoading sceneLoading, SetupLevelSequence setupLevelSequence, LevelSequenceView levelSequenceView, MenuView menuView)
        {
            _sceneLoading = sceneLoading;
            _setupLevelSequence = setupLevelSequence;
            _levelSequenceView = levelSequenceView;
            _menuView = menuView;
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
            await _menuView.StartAnimation();
            // buttons enable
        }
    }
}