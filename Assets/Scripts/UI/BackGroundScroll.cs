using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(RawImage))]
    public class BackGroundScroll : MonoBehaviour
    {
        [SerializeField] private float _scrollSpeed = 0.007f;
        [SerializeField] private float _xDirection = 1f;
        [SerializeField] private float _yDirection = 0f;
        
        private RawImage _backGroundImage;
        private Vector2 _uvRectSize;

        private async void Awake()
        {
            _backGroundImage = GetComponent<RawImage>();
            _uvRectSize = _backGroundImage.uvRect.size;
            await Scroll().SuppressCancellationThrow();

        }

        private async UniTask Scroll()
        {
            while (destroyCancellationToken.IsCancellationRequested == false)
            {
                _backGroundImage.uvRect = new Rect(
                    _backGroundImage.uvRect.position + new Vector2(_xDirection * _scrollSpeed, _yDirection * _scrollSpeed) * Time.deltaTime, _uvRectSize 
                    );
                await UniTask.Yield(PlayerLoopTiming.Update, destroyCancellationToken);

            }
        }
    }
}