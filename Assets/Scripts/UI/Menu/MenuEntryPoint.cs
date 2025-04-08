using Audio;
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
        private AudioManager _audioManager;

        public MenuEntryPoint(IAsyncSceneLoading sceneLoading, SetupLevelSequence setupLevelSequence, LevelSequenceView levelSequenceView, MenuView menuView, AudioManager audioManager)
        {
            _sceneLoading = sceneLoading;
            _setupLevelSequence = setupLevelSequence;
            _levelSequenceView = levelSequenceView;
            _menuView = menuView;
            _audioManager = audioManager;
        }

        public async void Initialize()
        {
            await _setupLevelSequence.Setup(1);
            _audioManager.PlayMenuMusic();
            _sceneLoading.LoadingDone(true);
            await _menuView.StartAnimation();
            _levelSequenceView.SetupButtonsView(1);
            
            
        }
    }
}