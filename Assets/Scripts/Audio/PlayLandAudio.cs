using Controllers;
using Managers;
using UnityEngine;

namespace Audio {
    public class PlayLandAudio : StateMachineBehaviour {
        [SerializeField, Range(0f, 1f)] private float minAirTime = 0.1f;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            PlayerController player = CharacterManager.Instance.currentPlayerController;

            // Play landing audio
            if (player.airTime >= minAirTime && player.audioSource) {
                player.audioSource.PlayOneShot(AudioManager.GetRandomAudioClip(player.landAudios), player.landAudioVolume);
            }

            player.airTime = 0f;
        }
    }
}