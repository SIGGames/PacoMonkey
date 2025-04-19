using System.Collections.Generic;
using UnityEngine;

namespace View {
    [ExecuteInEditMode]
    public class ParallaxBackground : MonoBehaviour {
        public ParallaxCamera parallaxCamera;
        private readonly List<ParallaxLayer> _parallaxLayers = new();

        private void Start() {
            if (parallaxCamera == null)
                parallaxCamera = Camera.main.GetComponent<ParallaxCamera>();

            if (parallaxCamera != null)
                parallaxCamera.onCameraTranslate += Move;

            SetLayers();
        }

        private void SetLayers() {
            _parallaxLayers.Clear();

            for (int i = 0; i < transform.childCount; i++) {
                ParallaxLayer layerOld = transform.GetChild(i).GetComponent<ParallaxLayer>();

                if (layerOld != null) {
                    layerOld.name = "Layer-" + i;
                    _parallaxLayers.Add(layerOld);
                }
            }
        }

        private void Move(float delta) {
            foreach (ParallaxLayer layer in _parallaxLayers) {
                layer.Move(delta);
            }
        }
    }
}