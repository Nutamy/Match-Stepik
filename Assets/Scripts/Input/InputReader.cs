using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    public class InputReader : IDisposable
    {
        // Нажатие и отпускание пальца/кнопки мыши, движение указателя
        public event Action PointerDown;
        public event Action PointerUp;
        public event Action PointerMoved;

        private readonly Inputs _inputs;

        public InputReader()
        {
            _inputs = new Inputs();
            _inputs.Player.Fire.performed += OnFirePerformed;
            _inputs.Player.Fire.canceled += OnFireCanceled;
            _inputs.Player.Select.performed += OnSelectPerformed;
        }

        public void EnableInputs(bool value)
        {
            if (value)
            {
                _inputs.Enable();
            }
            else
            {
                _inputs.Disable();
            }
        }

        public Vector2 Position() => _inputs.Player.Select.ReadValue<Vector2>();

        private void OnFirePerformed(InputAction.CallbackContext context) => PointerDown?.Invoke();

        private void OnFireCanceled(InputAction.CallbackContext context) => PointerUp?.Invoke();

        private void OnSelectPerformed(InputAction.CallbackContext context) => PointerMoved?.Invoke();

        public void Dispose()
        {
            _inputs.Player.Fire.performed -= OnFirePerformed;
            _inputs.Player.Fire.canceled -= OnFireCanceled;
            _inputs.Player.Select.performed -= OnSelectPerformed;
            _inputs.Dispose();
        }
    }
}
