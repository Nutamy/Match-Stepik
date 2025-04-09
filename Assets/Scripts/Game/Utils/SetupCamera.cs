using UnityEngine;

namespace Game.Utils
{
    public class SetupCamera
    {
        private bool _isVertical;

        public void SetCamera(int width, int height, bool isVertical)
        {
            _isVertical = isVertical;
            var xPos = width / 2f - 0.5f;
            var yPos = height / 2f - 1.1f;
            Camera.main.gameObject.transform.position = new Vector3(xPos, yPos, -10f);
            Camera.main.orthographicSize = GetOrthoSize(width, height);
        }

        private float GetOrthoSize(int width, int height)
        {
            return _isVertical
                ? (width + 2f) * Screen.height / Screen.width * 0.5f
                : (height + 2f) * Screen.height / Screen.width;
        }
    }
}