using TMPro;
using Levels;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace UI.Menu
{
    public class StartLevelButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Button _levelButton;

        private LevelConfig _myLevelConfig;
        private StartGame _startGame;

        // Публичное свойство, если вдруг понадобится извне
        public int LevelNumber => _myLevelConfig != null ? _myLevelConfig.LevelNumber : 0;

        private void OnEnable()
        {
            _levelButton.onClick.AddListener(OnButtonClick);
        }

        private void OnDisable()
        {
            _levelButton.onClick.RemoveListener(OnButtonClick);
        }

        // Метод инициализации кнопки данными
        public void Init(LevelConfig config, int currentProgressLevel)
        {
            _myLevelConfig = config;

            if (_myLevelConfig == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            _levelText.text = _myLevelConfig.LevelNumber.ToString();

            // Кнопка активна, если номер уровня меньше или равен прогрессу игрока
            _levelButton.interactable = _myLevelConfig.LevelNumber <= currentProgressLevel;
        }

        private void OnButtonClick()
        {
            if (_myLevelConfig != null && _startGame != null)
            {
                Debug.Log($"[StartLevelButton] Starting level: {_myLevelConfig.LevelNumber}");
                _startGame.Start(_myLevelConfig);
            }
        }

        [Inject]
        private void Construct(StartGame startGame)
        {
            _startGame = startGame;
        }
    }
}