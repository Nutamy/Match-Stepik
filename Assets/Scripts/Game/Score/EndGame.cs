using Audio;
using Data;
using SceneLoading;

namespace Game.Score
{
    public class EndGame
    {
        private GameData _gameData;
        private AudioManager _audioManager;
        private IAsyncSceneLoading _sceneLoading;

        public EndGame(GameData gameData, AudioManager audioManager, IAsyncSceneLoading sceneLoading)
        {
            _gameData = gameData;
            _audioManager = audioManager;
            _sceneLoading = sceneLoading;
        }

        public async void End(bool success)
        {
            if (success && _gameData.CurrentLevel.LevelNumber == _gameData.CurrentLevellIndex)
            {
                _gameData.OpenNextLevel();
            }
            _audioManager.StopMusic();
            await _sceneLoading.UnLoadAsync(Scenes.GAME);
            await _sceneLoading.LoadAsync(Scenes.MENU);
            _audioManager.PlayMenuMusic();
            
        }
    }
}