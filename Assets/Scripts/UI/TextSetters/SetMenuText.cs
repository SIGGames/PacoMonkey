using System;
using TMPro;
using UnityEngine;

namespace UI.TextSetters {
    public class SetMenuText : MonoBehaviour {
        [SerializeField] private TitleTextType textType;

        private TextMeshProUGUI _textComponent;
        private string _text;

        private void Awake() {
            _textComponent = GetComponent<TextMeshProUGUI>();
        }

        private void Start() {
            _text = textType switch {
                TitleTextType.GameName => Application.companyName,
                TitleTextType.CompanyName => Application.productName,
                TitleTextType.Version => Application.version,
                _ => throw new ArgumentOutOfRangeException()
            };

            _textComponent.text = _text;
        }
    }

    public enum TitleTextType {
        GameName,
        CompanyName,
        Version
    }
}