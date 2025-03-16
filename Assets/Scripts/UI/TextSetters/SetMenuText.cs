using System;
using TMPro;
using UnityEngine;

namespace UI.TextSetters {
    public class SetMenuText : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI textComponent;
        [SerializeField] private TitleTextType textType;

        private string _text;

        private void Start() {
            _text = textType switch {
                TitleTextType.GameName => Application.companyName,
                TitleTextType.CompanyName => Application.productName,
                TitleTextType.Version => Application.version,
                _ => throw new ArgumentOutOfRangeException()
            };

            textComponent.text = _text;
        }
    }

    public enum TitleTextType {
        GameName,
        CompanyName,
        Version
    }
}