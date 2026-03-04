using Audio;
using Data;
using SceneLoading;
using UI.Menu.Levels;
using VContainer.Unity;
using System.Collections.Generic;

namespace UI.Menu
{
    public class MenuEntryPoint : IInitializable
    {
        private readonly IAsyncSceneLoading _sceneLoading;
        private readonly SetupLevelSequence _setupLevelSequence;
        private readonly LevelSequenceView _levelSequenceView;
        private readonly MenuView _menuView;
        private readonly AudioManager _audioManager;
        private readonly GameData _gameData;

        public MenuEntryPoint(
            IAsyncSceneLoading sceneLoading,
            SetupLevelSequence setupLevelSequence,
            LevelSequenceView levelSequenceView,
            MenuView menuView,
            AudioManager audioManager,
            GameData gameData)
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
            // 1. Загружаем конфиг (теперь без аргументов, так как грузим AllLevels)
            await _setupLevelSequence.Setup();

            // 2. Создаем кнопки и получаем их список для анимации
            // Используем индекс прогресса из GameData
            int currentProgress = _gameData.CurrentLevellIndex;
            List<StartLevelButton> buttons = _levelSequenceView.SetupButtonsView(currentProgress);

            // 3. Базовые настройки меню
            _audioManager.PlayMenuMusic();
            _sceneLoading.LoadingDone(true);

            // 4. Запускаем анимацию и ПЕРЕДАЕМ список кнопок (исправляет ошибку CS7036)
            await _menuView.StartAnimation(buttons);
        }
    }
}