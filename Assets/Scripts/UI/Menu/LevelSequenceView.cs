using System.Collections.Generic;
using UI.Menu.Levels;
using UnityEngine;
using VContainer;
using System;

namespace UI.Menu
{
    public class LevelSequenceView : MonoBehaviour
    {
        [SerializeField] private List<StartLevelButton> _levelButtons = new List<StartLevelButton>();
        private SetupLevelSequence _setupLevelSequence;
        
        private void OnValidate()
        {
            if (_levelButtons.Count != 5)
            {
                throw new ArgumentOutOfRangeException("Level buttons must contain 5 elements");
            }
        }

        public void SetupButtonsView(int currentLevel)
        {
            for (int i = 0; i < _levelButtons.Count; i++)
            {
                Debug.Log("[i] = " + i);
                Debug.Log("_levelButtons[i] = " + _levelButtons[i]);
                Debug.Log("_setupLevelSequence.CurrentLevelSequence.LevelSequence[i] = " + _setupLevelSequence.CurrentLevelSequence.LevelSequence[i]);
                
                _levelButtons[i].SetLevelNumber(_setupLevelSequence.CurrentLevelSequence.LevelSequence[i].LevelNumber);
                _levelButtons[i].SetLable();
                if (_levelButtons[i].LevelNumber > currentLevel)
                {
                    _levelButtons[i].SetButtonInteractable(false);
                }
            }
        }
        
        [Inject]
        private void Construct(SetupLevelSequence setupLevelSequence)
        {
            _setupLevelSequence = setupLevelSequence;
        }
    }
}