using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace UI {
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LinkOpener : MonoBehaviour, IPointerClickHandler {
        private TextMeshProUGUI _textMeshPro;

        private void Awake() {
            _textMeshPro = GetComponent<TextMeshProUGUI>();
        }

        public void OnPointerClick(PointerEventData eventData) {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(_textMeshPro, eventData.position, null);
            if (linkIndex != -1) {
                TMP_LinkInfo linkInfo = _textMeshPro.textInfo.linkInfo[linkIndex];
                OpenURL.OpenProvidedUrl(linkInfo.GetLinkID());
            }
        }
    }
}