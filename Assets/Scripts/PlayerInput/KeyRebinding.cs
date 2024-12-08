using UnityEngine;
using UnityEngine.InputSystem;

public class KeyRebinding : MonoBehaviour {
    private PlayerInputActions _inputActions;

    private void Awake() {
        _inputActions = new PlayerInputActions();
        _inputActions.Enable();
    }

    public void StartRebind() {
        var action = _inputActions.PlayerControls.Jump;

        action.PerformInteractiveRebinding()
            .WithControlsExcluding("Mouse")
            .OnComplete(operation => {
                Debug.Log($"Nova tecla assignada: {action.bindings[0].effectivePath}");
                operation.Dispose();
            })
            .Start();
    }
}