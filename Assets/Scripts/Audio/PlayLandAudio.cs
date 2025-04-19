
using Controllers;
using Managers;
using UnityEngine;

namespace Audio {
    public class PlayLandAudio : StateMachineBehaviour {
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            PlayerController player = CharacterManager.Instance.currentPlayerController;

            // Play landing audio
            if (player.audioSource) {
                player.audioSource.PlayOneShot(AudioManager.GetRandomAudioClip(player.landAudios), player.landAudioVolume);
            }
        }
    }
}