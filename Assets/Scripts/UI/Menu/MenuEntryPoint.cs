using Audio;
using Data;
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
        private GameData _gameData;

        public MenuEntryPoint(IAsyncSceneLoading sceneLoading, SetupLevelSequence setupLevelSequence, LevelSequenceView levelSequenceView, MenuView menuView, AudioManager audioManager, GameData gameData)
        {
            _sceneLoading = sceneLoading;
            _setupLevelSequence = setupLevelSequence;
            _levelSequenceView = levelSequenceView;
            _menuView = menuView;
            _audioManager = audioManager;
            _gameData = gameData;
        }

        public async void Initialize()
        {
            await _setupLevelSequence.Setup(_gameData.CurrentLevellIndex);
            _levelSequenceView.SetupButtonsView(_gameData.CurrentLevellIndex);
            _audioManager.PlayMenuMusic();
            _sceneLoading.LoadingDone(true);
            await _menuView.StartAnimation();
            
            
            
        }
    }
}