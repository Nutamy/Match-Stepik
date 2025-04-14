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
            //if (_levelSequence.Count != 5)
            //{
                //throw new ArgumentOutOfRangeException("Levels sequence must contain 5 elements");
            //}
            Debug.Log("OnValidate +-+-+-+-+-+-+-");
        }
    }
}