using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using Localization;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using static Utils.PlayerPrefsKeys;
using PlayerInputManager = PlayerInput.PlayerInputManager;

// ReSharper disable Unity.NoNullPropagation
namespace UI.Rebinding_UI {
    /// <summary>
    /// A reusable component with a self-contained UI for rebinding a single action.
    /// </summary>
    public class RebindActionUI : MonoBehaviour {
        /// <summary>
        /// Reference to the action that is to be rebound.
        /// </summary>
        public InputActionReference ActionReference {
            get => m_Action;
            set {
                m_Action = value;
                UpdateActionLabel();
                UpdateBindingDisplay();
            }
        }

        [SerializeField]
        private InputDeviceType bindsInputType;

        private GameObject _resetToDefaultButton;

        private InputSystemUIInputModule _uiInputModule;

        /// <summary>
        /// ID (in string form) of the binding that is to be rebound on the action.
        /// </summary>
        /// <seealso cref="InputBinding.id"/>
        public string BindingId {
            get => m_BindingId;
            set {
                m_BindingId = value;
                UpdateBindingDisplay();
            }
        }

        public InputBinding.DisplayStringOptions DisplayStringOptions {
            get => m_DisplayStringOptions;
            set {
                m_DisplayStringOptions = value;
                UpdateBindingDisplay();
            }
        }

        /// <summary>
        /// Text component that receives the name of the action. Optional.
        /// </summary>
        public TextMeshProUGUI ActionLabel {
            get => m_ActionLabel;
            set {
                m_ActionLabel = value;
                UpdateActionLabel();
            }
        }

        /// <summary>
        /// Text component that receives the display string of the binding. Can be <c>null</c> in which
        /// case the component entirely relies on <see cref="updateBindingUIEvent"/>.
        /// </summary>
        public TextMeshProUGUI BindingText {
            get => m_BindingText;
            set {
                m_BindingText = value;
                UpdateBindingDisplay();
            }
        }

        /// <summary>
        /// Optional text component that receives a text prompt when waiting for a control to be actuated.
        /// </summary>
        /// <seealso cref="startRebindEvent"/>
        /// <seealso cref="RebindOverlay"/>
        public TextMeshProUGUI RebindPrompt {
            get => m_RebindText;
            set => m_RebindText = value;
        }

        /// <summary>
        /// Optional UI that is activated when an interactive rebind is started and deactivated when the rebind
        /// is finished. This is normally used to display an overlay over the current UI while the system is
        /// waiting for a control to be actuated.
        /// </summary>
        /// <remarks>
        /// If neither <see cref="RebindPrompt"/> nor <c>rebindOverlay</c> is set, the component will temporarily
        /// replaced the <see cref="BindingText"/> (if not <c>null</c>) with <c>"Waiting..."</c>.
        /// </remarks>
        /// <seealso cref="startRebindEvent"/>
        /// <seealso cref="RebindPrompt"/>
        public GameObject RebindOverlay {
            get => m_RebindOverlay;
            set => m_RebindOverlay = value;
        }

        /// <summary>
        /// Event that is triggered every time the UI updates to reflect the current binding.
        /// This can be used to tie custom visualizations to bindings.
        /// </summary>
        public UpdateBindingUIEvent updateBindingUIEvent => m_UpdateBindingUIEvent ??= new UpdateBindingUIEvent();

        /// <summary>
        /// Event that is triggered when an interactive rebind is started on the action.
        /// </summary>
        public InteractiveRebindEvent startRebindEvent => m_RebindStartEvent ??= new InteractiveRebindEvent();

        /// <summary>
        /// Event that is triggered when an interactive rebind has been completed or canceled.
        /// </summary>
        public InteractiveRebindEvent stopRebindEvent => m_RebindStopEvent ??= new InteractiveRebindEvent();

        /// <summary>
        /// When an interactive rebind is in progress, this is the rebind operation controller.
        /// Otherwise, it is <c>null</c>.
        /// </summary>
        public InputActionRebindingExtensions.RebindingOperation ongoingRebind => m_RebindOperation;

        /// <summary>
        /// Return the action and binding index for the binding that is targeted by the component
        /// according to
        /// </summary>
        /// <param name="action"></param>
        /// <param name="bindingIndex"></param>
        /// <returns></returns>
        public bool ResolveActionAndBinding(out InputAction action, out int bindingIndex) {
            bindingIndex = -1;

            action = m_Action?.action;

            if (action == null)
                return false;

            if (string.IsNullOrEmpty(m_BindingId))
                return false;

            // Look up binding index.
            var bindingId = new Guid(m_BindingId);
            bindingIndex = action.bindings.IndexOf(x => x.id == bindingId);
            if (bindingIndex == -1) {
                Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Trigger a refresh of the currently displayed binding.
        /// </summary>
        public void UpdateBindingDisplay() {
            string displayString = string.Empty;
            string deviceLayoutName = default;
            string controlPath = default;

            // Get display string from action.
            var action = m_Action?.action;
            if (action != null) {
                int bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == m_BindingId);
                if (bindingIndex != -1) {
                    displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath,
                        DisplayStringOptions);

                    // If we have no display string, try to get the effective path
                    if (string.IsNullOrEmpty(controlPath)) {
                        controlPath = action.bindings[bindingIndex].effectivePath;
                    }

                    if (string.IsNullOrEmpty(controlPath)) {
                        controlPath = action.bindings[bindingIndex].path;
                    }
                }
            }

            // Set on label (if any).
            if (m_BindingText != null)
                m_BindingText.text = displayString;

            // Give listeners a chance to configure UI in response.
            m_UpdateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);

            UpdateResetButton();
        }

        /// <summary>
        /// Remove currently applied binding overrides.
        /// </summary>
        public void ResetToDefault() {
            if (!ResolveActionAndBinding(out var action, out int bindingIndex)) {
                return;
            }

            // If its composite all parts needs to be reset
            if (action.bindings[bindingIndex].isComposite) {
                for (int i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++) {
                    action.RemoveBindingOverride(i);
                }
            } else {
                ResetBinding(action, bindingIndex);
            }
            /*
            if (action.bindings[bindingIndex].isComposite) {
                // It's a composite. Remove overrides from part bindings.
                for (int i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                    action.RemoveBindingOverride(i);
            } else {
                action.RemoveBindingOverride(bindingIndex);
            }
            */

            SaveControlSettings();
            UpdateBindingDisplay();
        }

        private static void ResetBinding(InputAction action, int bindingIndex) {
            InputBinding newBinding = action.bindings[bindingIndex];
            string oldOverridePath = newBinding.overridePath;

            action.RemoveBindingOverride(bindingIndex);

            foreach (var otherAction in action.actionMap.actions.Where(otherAction => otherAction != action)) {
                for (int i = 0; i < otherAction.bindings.Count; i++) {
                    if (otherAction.bindings[i].overridePath == newBinding.path) {
                        otherAction.ApplyBindingOverride(i, oldOverridePath);
                    }
                }
            }
        }

        /// <summary>
        /// Initiate an interactive rebind that lets the player actuate a control to choose a new binding
        /// for the action.
        /// </summary>
        public void StartInteractiveRebind() {
            if (!ResolveActionAndBinding(out var action, out int bindingIndex))
                return;

            // If the binding is a composite, we need to rebind each part in turn.
            if (action.bindings[bindingIndex].isComposite) {
                int firstPartIndex = bindingIndex + 1;
                if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite) {
                    PerformInteractiveRebind(action, firstPartIndex, allCompositeParts: true);
                }
            } else {
                PerformInteractiveRebind(action, bindingIndex);
            }
        }

        private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false,
            bool keyHasBeenUsed = false) {
            m_RebindOperation?.Cancel(); // Will null out m_RebindOperation.

            action.Disable();

            // Configure the rebind.
            m_RebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("<Mouse>/leftButton")
                .WithControlsExcluding("<Mouse>/press")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnCancel(
                    operation => {
                        EnableUINavigation();
                        action.Enable();
                        m_RebindStopEvent?.Invoke(this, operation);
                        m_RebindOverlay?.SetActive(false);
                        UpdateBindingDisplay();
                        CleanUp();
                    })
                .OnComplete(
                    operation => {
                        EnableUINavigation();
                        action.Enable();
                        m_RebindOverlay?.SetActive(false);
                        m_RebindStopEvent?.Invoke(this, operation);

                        if (CheckDuplicateBindings(action, bindingIndex, allCompositeParts)) {
                            action.RemoveBindingOverride(bindingIndex);
                            CleanUp();
                            PerformInteractiveRebind(action, bindingIndex, allCompositeParts, true);
                            return;
                        }

                        UpdateBindingDisplay();
                        CleanUp();

                        // If there's more composite parts we should bind, initiate a rebind
                        // for the next part.
                        if (allCompositeParts) {
                            int nextBindingIndex = bindingIndex + 1;
                            if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
                                PerformInteractiveRebind(action, nextBindingIndex, true);
                        }

                        SaveControlSettings();
                    });

            // If it's a part binding, show the name of the part in the UI.
            string bindingName = action.bindings[bindingIndex].isPartOfComposite
                ? $"'{action.bindings[bindingIndex].name}'"
                : $"'{m_Action?.action.name}'";

            // Translate the binding name if possible
            string formattedBindingName = bindingName.ToLower().Replace("'", "");
            if (LocalizationManager.Instance != null && LocalizationManager.Instance.HasKey(formattedBindingName)) {
                bindingName = "'" + LocalizationManager.Instance.GetLocalizedText(formattedBindingName) + "'";
            }

            // Bring up rebind overlay, if we have one.
            m_RebindOverlay?.SetActive(true);
            if (m_RebindText != null) {
                string text = LocalizationManager.Instance.GetLocalizedText(
                    !keyHasBeenUsed ? "waiting-for-input" : "key-already-used");

                if (!string.IsNullOrEmpty(m_RebindOperation.expectedControlType) && !keyHasBeenUsed) {
                    text += $" {bindingName}";
                }

                m_RebindText.text = text;
            }

            // If we have no rebind overlay and no callback but we have a binding text label,
            // temporarily set the binding text label to "<Waiting>".
            if (m_RebindOverlay == null && m_RebindText == null && m_RebindStartEvent == null && m_BindingText != null)
                m_BindingText.text = "<Waiting...>";

            // Give listeners a chance to act on the rebind starting.
            m_RebindStartEvent?.Invoke(this, m_RebindOperation);

            // Enable bind only for those binds that match the input device
            switch (bindsInputType) {
                case InputDeviceType.Controller:
                    m_RebindOperation.WithControlsExcluding("<Keyboard>").WithCancelingThrough("<Keyboard>/escape");
                    break;
                case InputDeviceType.Keyboard:
                    m_RebindOperation.WithControlsExcluding("<Gamepad>").WithCancelingThrough("<Keyboard>/escape");
                    break;
                case InputDeviceType.Unknown:
                default:
                    break;
            }

            DisableUINavigation();
            m_RebindOperation.Start();
            return;

            void CleanUp() {
                m_RebindOperation?.Dispose();
                m_RebindOperation = null;
            }
        }

        private static bool CheckDuplicateBindings(InputAction action, int bindingIndex, bool allCompositeParts = false) {
            InputBinding newBinding = action.bindings[bindingIndex];
            foreach (InputBinding binding in action.actionMap.bindings) {
                if (binding.action == newBinding.action) {
                    continue;
                }

                if (binding.effectivePath == newBinding.effectivePath) {
                    return true;
                }
            }

            if (allCompositeParts) {
                for (int i = 0; i < bindingIndex; i++) {
                    if (action.bindings[i].effectivePath == newBinding.overridePath) {
                        return true;
                    }
                }
            }

            return false;
        }

        private void SaveControlSettings() {
            string overrides = m_Action.action.actionMap.asset.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString(BindingOverridesKey, overrides);
            PlayerPrefs.Save();

            PlayerInputManager.Instance.UpdateBindingKeys();
            PlayerInputManager.Instance.OnInputTypeChange();
        }

        private void DisableUINavigation() {
            if (EventSystem.current != null) {
                EventSystem.current.sendNavigationEvents = false;
            }

            _uiInputModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
            if (_uiInputModule != null) {
                _uiInputModule.enabled = false;
            }
        }

        private void EnableUINavigation() {
            if (EventSystem.current != null) {
                EventSystem.current.sendNavigationEvents = true;
            }

            if (_uiInputModule != null) {
                _uiInputModule.enabled = true;
            }
        }

        protected void OnEnable() {
            s_RebindActionUIs ??= new List<RebindActionUI>();
            s_RebindActionUIs.Add(this);
            if (s_RebindActionUIs.Count == 1) {
                InputSystem.onActionChange += OnActionChange;
            }

            // Load binding overrides from player prefs
            if (PlayerInputManager.Instance != null && PlayerInputManager.Instance.InputActions != null) {
                string overrides = PlayerInputManager.Instance.InputActions.SaveBindingOverridesAsJson();
                m_Action.action.actionMap.asset.LoadBindingOverridesFromJson(overrides);
            }

            UpdateBindingDisplay();

            // On language change event update the action labels
            if (LocalizationManager.Instance != null) {
                LocalizationManager.Instance.OnLanguageChanged += UpdateActionLabel;
            }
        }

        protected void OnDisable() {
            m_RebindOperation?.Dispose();
            m_RebindOperation = null;

            s_RebindActionUIs.Remove(this);
            if (s_RebindActionUIs.Count == 0) {
                s_RebindActionUIs = null;
                InputSystem.onActionChange -= OnActionChange;
            }

            // Do not unsubscribe from language change event since this will be done while this is disabled
        }

        // When the action system re-resolves bindings, we want to update our UI in response. While this will
        // also trigger from changes we made ourselves, it ensures that we react to changes made elsewhere. If
        // the user changes keyboard layout, for example, we will get a BoundControlsChanged notification and
        // will update our UI to reflect the current keyboard layout.
        private static void OnActionChange(object obj, InputActionChange change) {
            if (change != InputActionChange.BoundControlsChanged)
                return;

            var action = obj as InputAction;
            var actionMap = action?.actionMap ?? obj as InputActionMap;
            var actionAsset = actionMap?.asset ?? obj as InputActionAsset;

            foreach (var component in s_RebindActionUIs) {
                var referencedAction = component.ActionReference?.action;
                if (referencedAction == null)
                    continue;

                if (referencedAction == action ||
                    referencedAction.actionMap == actionMap ||
                    referencedAction.actionMap?.asset == actionAsset)
                    component.UpdateBindingDisplay();
            }
        }

        [Tooltip("Reference to action that is to be rebound from the UI.")]
        [SerializeField]
        private InputActionReference m_Action;

        [SerializeField]
        private string m_BindingId;

        [SerializeField]
        private InputBinding.DisplayStringOptions m_DisplayStringOptions;

        [Tooltip("Text label that will receive the name of the action. Optional. Set to None to have the "
                 + "rebind UI not show a label for the action.")]
        [SerializeField]
        private TextMeshProUGUI m_ActionLabel;

        [Tooltip("Text label that will receive the current, formatted binding string.")]
        [SerializeField]
        private TextMeshProUGUI m_BindingText;

        [Tooltip("Optional UI that will be shown while a rebind is in progress.")]
        [SerializeField]
        private GameObject m_RebindOverlay;

        [Tooltip("Optional text label that will be updated with prompt for user input.")]
        [SerializeField]
        private TextMeshProUGUI m_RebindText;

        public bool overrideActionLabel;

        [SerializeField]
        private string actionLabelString;

        [Tooltip("Event that is triggered when the way the binding is display should be updated. This allows displaying "
                 + "bindings in custom ways, e.g. using images instead of text.")]
        [SerializeField]
        private UpdateBindingUIEvent m_UpdateBindingUIEvent;

        [Tooltip("Event that is triggered when an interactive rebind is being initiated. This can be used, for example, "
                 + "to implement custom UI behavior while a rebind is in progress. It can also be used to further "
                 + "customize the rebind.")]
        [SerializeField]
        private InteractiveRebindEvent m_RebindStartEvent;

        [Tooltip("Event that is triggered when an interactive rebind is complete or has been aborted.")]
        [SerializeField]
        private InteractiveRebindEvent m_RebindStopEvent;

        private InputActionRebindingExtensions.RebindingOperation m_RebindOperation;

        private static List<RebindActionUI> s_RebindActionUIs;

        // We want the label for the action name to update in edit mode, too, so
        // we kick that off from here.
        #if UNITY_EDITOR
        protected void OnValidate() {
            UpdateActionLabel();
            UpdateBindingDisplay();
        }
        #endif

        private void Awake() {
            _resetToDefaultButton = transform.Find("ResetToDefaultButton")?.GetComponent<Button>()?.gameObject;
        }

        private void Start() {
            UpdateActionLabel();
            UpdateBindingDisplay();
        }

        private void UpdateResetButton() {
            if (_resetToDefaultButton == null) {
                return;
            }

            if (!ResolveActionAndBinding(out InputAction action, out int bindingIndex)) {
                _resetToDefaultButton.GetComponent<Button>().interactable = false;
                return;
            }

            bool isChanged = false;
            // If its a composite all parts needs to be checked
            if (action.bindings[bindingIndex].isComposite) {
                for (int i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; i++) {
                    if (!string.IsNullOrEmpty(action.bindings[i].overridePath) &&
                        action.bindings[i].overridePath != action.bindings[i].path) {
                        isChanged = true;
                        break;
                    }
                }
            } else {
                isChanged = action.bindings[bindingIndex].effectivePath != action.bindings[bindingIndex].path;
            }

            _resetToDefaultButton.GetComponent<Button>().interactable = isChanged;
        }

        private void UpdateActionLabel() {
            if (m_ActionLabel != null) {
                var action = m_Action?.action;

                string text;
                if (overrideActionLabel) {
                    text = actionLabelString;
                } else {
                    text = action != null ? action.name : string.Empty;
                }

                if (text != null) {
                    // Formatting text for translations
                    string formattedText = text.ToLower().Replace(" ", "-");
                    if (LocalizationManager.Instance != null) {
                        text = LocalizationManager.Instance.GetLocalizedText(formattedText);
                    }
                }

                m_ActionLabel.text = text;
            }
        }

        [Serializable]
        public class UpdateBindingUIEvent : UnityEvent<RebindActionUI, string, string, string> {
        }

        [Serializable]
        public class InteractiveRebindEvent : UnityEvent<RebindActionUI, InputActionRebindingExtensions.RebindingOperation> {
        }
    }
}