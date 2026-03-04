using System;
using System.Collections.Generic;
using Levels;
using UnityEngine;

namespace UI.Menu.Levels
{
    [CreateAssetMenu(fileName = "LevelSequenceConfigs", menuName = "Configs/LevelSequenceConfigs")]
    public class LevelSequenceConfig : ScriptableObject
    {
        [SerializeField] private List<LevelConfig> _levelSequence = new List<LevelConfig>();

        public List<LevelConfig> LevelSequence => _levelSequence;

        private void OnValidate()
        {
            // Убираем жесткую проверку на 5 элементов, 
            // так как теперь у нас 15 уровней в AllLevels.
            if (_levelSequence == null || _levelSequence.Count == 0)
            {
                Debug.LogWarning("Level sequence is empty!");
            }
        }
    }
}