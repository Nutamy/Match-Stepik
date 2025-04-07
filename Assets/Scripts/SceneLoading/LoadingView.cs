using UnityEngine;

namespace SceneLoading
{
    public class LoadingView : MonoBehaviour
    {
        [SerializeField] private GameObject _loadingScreen;

        public void SetActiveScreen(bool value)
        {
            _loadingScreen.SetActive(value);
        }
    }
}