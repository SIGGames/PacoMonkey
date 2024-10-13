#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor {
    [CustomPropertyDrawer(typeof(TagSelectorAttribute))]
    public class TagSelectorPropertyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            TagSelectorAttribute tagSelector = (TagSelectorAttribute)attribute;

            if (property.propertyType == SerializedPropertyType.String) {
                if (tagSelector.UseDefaultTagFieldDrawer) {
                    property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
                }
                else {
                    EditorGUI.BeginProperty(position, label, property);

                    // Create the list of tags
                    List<string> tagList = new List<string>();
                    tagList.Add("<NoTag>");
                    tagList.AddRange(UnityEditorInternal.InternalEditorUtility.tags);
                    string propertyString = property.stringValue;
                    int index = -1;

                    // If the tag is empty, select the first one
                    if (propertyString == "") {
                        index = 0;
                    }
                    else {
                        // Search the index of the current tag
                        for (int i = 1; i < tagList.Count; i++) {
                            if (tagList[i] == propertyString) {
                                index = i;
                                break;
                            }
                        }
                    }

                    int newIndex = EditorGUI.Popup(position, label.text, index, tagList.ToArray());

                    if (newIndex > 0) {
                        property.stringValue = tagList[newIndex];
                    }
                    else {
                        property.stringValue = "";
                    }

                    EditorGUI.EndProperty();
                }
            }
            else {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}
#endif