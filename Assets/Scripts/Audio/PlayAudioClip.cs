using Managers;
using UnityEngine;

namespace Audio {
    public class PlayAudioClip : StateMachineBehaviour {
        public float t = 0.5f;

        public float modulus;

        public AudioClip clip;

        private float _lastT = -1f;

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            float nt = stateInfo.normalizedTime;
            if (modulus > 0f) {
                nt %= modulus;
            }

            if (nt >= t && _lastT < t) {
                CharacterManager.Instance.currentPlayerController.audioSource.PlayOneShot(clip);
            }

            _lastT = nt;
        }
    }
}