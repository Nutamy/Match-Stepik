using System;
using TMPro;
using UI.Menu.Levels;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace UI.Menu
{
    public class StartLevelButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Button _levelButton;
        public int LevelNumber { get; private set; }

        private StartGame _startGame;
        // start
        private SetupLevelSequence _setupLevelSequence;

        private void OnEnable()
        {
            _levelButton.onClick.AddListener(StartLevelButtonClick);
        }

        private void OnDisable()
        {
            _levelButton.onClick.RemoveListener(StartLevelButtonClick);
        }

        public void SetLevelNumber(int levelNumber)
        {
            LevelNumber = Mathf.Clamp(levelNumber, 1, 10);
        }

        public void SetLable()
        {
            _levelText.text = LevelNumber.ToString();
        }

        public void SetButtonInteractable(bool value)
        {
            _levelButton.interactable = value;
        }

        private void StartLevelButtonClick()
        {
            
            Debug.Log("StartLevelButtonClick ->" + (LevelNumber - 1) + "\nCount LevelSequence = " +  _setupLevelSequence.CurrentLevelSequence.LevelSequence.Count);
            // Находим индекс уровня в текущем наборе уровней
            int levelIndex = (LevelNumber - 1) % 5; // Для Levels1-5 или Levels6-10

            // Логгируем информацию
            Debug.Log("StartLevelButtonClick ->" + levelIndex + "\nCount LevelSequence = " + _setupLevelSequence.CurrentLevelSequence.LevelSequence.Count);

            // Загружаем уровень по индексу
            _startGame.Start(_setupLevelSequence.CurrentLevelSequence.LevelSequence[levelIndex]);
            
            //_startGame.Start(_setupLevelSequence.CurrentLevelSequence.LevelSequence[LevelNumber - 1]);
            Debug.Log($"{_setupLevelSequence.CurrentLevelSequence.LevelSequence[LevelNumber - 1]} level has been started");
        }

        [Inject]
        private void Construct(SetupLevelSequence setupLevelSequence, StartGame startGame)
        {
            _setupLevelSequence = setupLevelSequence;
            _startGame = startGame;
        }
    }
}