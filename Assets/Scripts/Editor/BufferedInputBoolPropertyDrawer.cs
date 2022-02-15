using System;
using UnityEngine;
using UnityEditor;

namespace Soulslike.EditingTools
{
    [CustomPropertyDrawer(typeof(BufferedInputBool))]
    public class BufferedInputBoolPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label.text = label.text + " Buffer Time";
            EditorGUI.PropertyField(position, property.FindPropertyRelative("timeFrame"), label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
