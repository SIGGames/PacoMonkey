using Controllers;
using Mechanics.Movement;
using Platformer.Gameplay;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Zones {
    public class VictoryZone : MonoBehaviour {
        void OnTriggerEnter2D(Collider2D collider) {
            var p = collider.gameObject.GetComponent<PlayerController>();
            if (p != null) {
                var ev = Schedule<PlayerEnteredVictoryZone>();
                ev.victoryZone = this;
            }
        }
    }
}