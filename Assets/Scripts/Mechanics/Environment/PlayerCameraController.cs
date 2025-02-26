using Managers;
using UnityEngine;
using static PlayerInput.KeyBinds;

namespace Mechanics.Environment {
    public class PlayerCameraController : MonoBehaviour {
        [SerializeField] private Vector2 cameraMove;
        private bool _anyKeyPressed;
        private Vector2 _modifiedOffset;

        private void Update() {
            ManageCameraInput();
        }

        private void ManageCameraInput() {
            Vector2 offset = Vector2.zero;
            bool isKeyPressed = false;

            if (GetCameraUpKey()) {
                offset.y += cameraMove.y;
                isKeyPressed = true;
            }
            if (GetCameraDownKey()) {
                offset.y -= cameraMove.y;
                isKeyPressed = true;
            }
            if (GetCameraLeftKey()) {
                offset.x -= cameraMove.x;
                isKeyPressed = true;
            }
            if (GetCameraRightKey()) {
                offset.x += cameraMove.x;
                isKeyPressed = true;
            }

            if (isKeyPressed) {
                CameraManager.Instance.SetOffset(offset);
                _modifiedOffset = offset;
                _anyKeyPressed = true;
            }
            else if (_anyKeyPressed) {
                CameraManager.Instance.SetOffset(-_modifiedOffset / 2);
                _anyKeyPressed = false;
            }
        }
    }
}