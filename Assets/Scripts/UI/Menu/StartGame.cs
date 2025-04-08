﻿using Audio;
using Data;
using Levels;
using SceneLoading;

namespace UI.Menu
{
    public class StartGame
    {
        private GameData _gameData;
        private AudioManager _audioManager;
        private IAsyncSceneLoading _sceneLoading;

        public StartGame(GameData gameData, AudioManager audioManager, IAsyncSceneLoading sceneLoading)
        {
            _gameData = gameData;
            _audioManager = audioManager;
            _sceneLoading = sceneLoading;
        }

        public async void Start(LevelConfig levelConfig)
        {
            _gameData.SetCurrentLevelConfig(levelConfig);
            _audioManager.StopMusic();
            _audioManager.PlayStopMusic();
            await _sceneLoading.UnLoadAsync(Scenes.MENU);
            await _sceneLoading.LoadAsync(Scenes.GAME);
            _audioManager.PlayGameMusic();
        }
    }
}