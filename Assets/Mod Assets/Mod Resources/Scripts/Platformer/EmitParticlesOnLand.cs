using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Gameplay;

[RequireComponent(typeof(ParticleSystem))]
public class EmitParticlesOnLand : MonoBehaviour {
    public bool emitOnLand = true;
    public bool emitOnEnemyDeath = true;

    #if UNITY_TEMPLATE_PLATFORMER

    ParticleSystem p;

    void Start() {
        p = GetComponent<ParticleSystem>();

        if (emitOnLand) {
            PlayerLanded.OnExecute += PlayerLanded_OnExecute;

            void PlayerLanded_OnExecute(PlayerLanded obj) {
                p.Play();
            }
        }

        if (emitOnEnemyDeath) {
            Platformer.Gameplay.EnemyDeath.OnExecute += EnemyDeath_OnExecute;

            void EnemyDeath_OnExecute(Platformer.Gameplay.EnemyDeath obj) {
                p.Play();
            }
        }
    }

    #endif
}