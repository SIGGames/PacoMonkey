using Managers;
using UnityEngine;
using static PlayerInput.KeyBinds;

namespace Mechanics.Environment {
    public class PlayerCameraController : MonoBehaviour {
        [SerializeField] private Vector2 cameraMove;
        private bool _anyKeyPressed;

        private void Update() {
            ManageCameraInput();
        }

        private void ManageCameraInput() {
            Vector2 offset = Vector2.zero;
            bool isKeyPressed = false;

            if (GetCameraUpKey()) {
                offset = new Vector2(0f, cameraMove.y);
                isKeyPressed = true;
            } else if (GetCameraDownKey()) {
                offset = new Vector2(0f, -cameraMove.y);
                isKeyPressed = true;
            } else if (GetCameraLeftKey()) {
                offset = new Vector2(-cameraMove.x, 0f);
                isKeyPressed = true;
            } else if (GetCameraRightKey()) {
                offset = new Vector2(cameraMove.x, 0f);
                isKeyPressed = true;
            }

            if (isKeyPressed) {
                CameraManager.Instance.SetOffset(offset);
                _anyKeyPressed = true;
            } else if (_anyKeyPressed) {
                CameraManager.Instance.ResetCamera();
                _anyKeyPressed = false;
            }
        }
    }
}