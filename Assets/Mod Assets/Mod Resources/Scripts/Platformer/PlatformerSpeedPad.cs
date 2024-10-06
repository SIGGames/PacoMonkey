using System.Collections;
using Mechanics;
using Mechanics.Movement;
using UnityEngine;
using Platformer.Mechanics;

public class PlatformerSpeedPad : MonoBehaviour {
    public float maxSpeed;

    [Range(0, 5)] public float duration = 1f;

    public float accelerationMultiplier = 1f;

    void OnTriggerEnter2D(Collider2D other) {
        var rb = other.attachedRigidbody;
        if (rb == null) return;
        var player = rb.GetComponent<PlayerController>();
        if (player == null) return;
        player.StartCoroutine(PlayerModifier(player, duration));
    }

    IEnumerator PlayerModifier(PlayerController player, float lifetime) {
        var initialSpeed = player.maxRunSpeed;
        var initialAcceleration = player.runAcceleration;
        var initialDeceleration = player.runDeceleration;

        player.maxRunSpeed = maxSpeed;
        player.runAcceleration *= accelerationMultiplier;
        player.runDeceleration *= accelerationMultiplier;

        yield return new WaitForSeconds(lifetime);

        player.maxRunSpeed = initialSpeed;
        player.runAcceleration = initialAcceleration;
        player.runDeceleration = initialDeceleration;
    }
}
