using Platformer.Core;
using UnityEngine;

namespace Controllers {
    public class GameController : MonoBehaviour {
        public static GameController Instance { get; private set; }

        private void OnEnable() {
            Instance = this;
        }

        private void OnDisable() {
            if (Instance == this) Instance = null;
        }

        private void Update() {
            if (Instance == this) Simulation.Tick();
        }
    }
}