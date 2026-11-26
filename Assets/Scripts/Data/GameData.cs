using System;
using Levels;
using UnityEngine;

namespace Data
{
    public class GameData
    {
        // Ключ PlayerPrefs: номер последнего открытого уровня
        public const string UnlockedLevelKey = "Progress.UnlockedLevel";

        public LevelConfig CurrentLevel { get; private set; }
        public int CurrentLevellIndex { get; private set; }
        public bool IsEnabledSound { get; private set; }

        public GameData()
        {
            IsEnabledSound = true;
            CurrentLevellIndex = Mathf.Max(1, PlayerPrefs.GetInt(UnlockedLevelKey, 1));
        }

        public void SetCurrentLevelIndex(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            CurrentLevellIndex = index;
            SaveProgress();
        }

        public void OpenNextLevel()
        {
            CurrentLevellIndex++;
            SaveProgress();
        }

        private void SaveProgress()
        {
            PlayerPrefs.SetInt(UnlockedLevelKey, CurrentLevellIndex);
            PlayerPrefs.Save();
        }

        public void SetEnabledSound(bool value) => IsEnabledSound = value;

        public void SetCurrentLevelConfig(LevelConfig level)
        {
            if (level != null) CurrentLevel = level;
        }
    }
}