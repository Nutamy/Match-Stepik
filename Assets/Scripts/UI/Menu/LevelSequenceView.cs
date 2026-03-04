using DG.Tweening; // Добавляем для DOKill()
using System.Collections.Generic;
using UI.Menu.Levels;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UI.Menu
{
    public class LevelSequenceView : MonoBehaviour
    {
        [SerializeField] private StartLevelButton _buttonPrefab;
        [SerializeField] private Transform _contentParent;

        private SetupLevelSequence _setupLevelSequence;
        private readonly List<StartLevelButton> _spawnedButtons = new List<StartLevelButton>();
        private IObjectResolver _resolver;

        [Inject]
        private void Construct(SetupLevelSequence setupLevelSequence, IObjectResolver resolver)
        {
            _setupLevelSequence = setupLevelSequence;
            _resolver = resolver;
        }

        public List<StartLevelButton> SetupButtonsView(int playerMaxLevel)
        {
            // 1. Очищаем старые кнопки и убиваем их анимации ПЕРЕД созданием новых
            foreach (var btn in _spawnedButtons)
            {
                if (btn != null)
                {
                    btn.transform.DOKill();
                    Destroy(btn.gameObject);
                }
            }
            _spawnedButtons.Clear();

            if (_setupLevelSequence.AllLevels == null)
            {
                Debug.LogError("[LevelSequenceView] AllLevels is null!");
                return _spawnedButtons;
            }

            var sequence = _setupLevelSequence.AllLevels.LevelSequence;

            // 2. Создаем новые кнопки из префаба
            foreach (var levelConfig in sequence)
            {
                // Создаем кнопку через VContainer
                StartLevelButton newButton = _resolver.Instantiate(_buttonPrefab, _contentParent);

                // СРАЗУ выключаем, чтобы она не мелькнула до начала анимации
                newButton.gameObject.SetActive(false);

                // Инициализируем данными
                newButton.Init(levelConfig, playerMaxLevel);

                _spawnedButtons.Add(newButton);
            }

            return _spawnedButtons; // Передаем список выключенных кнопок в MenuView
        }
    }
}