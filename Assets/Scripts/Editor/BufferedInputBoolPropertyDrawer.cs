using System;
using UnityEngine;
using UnityEditor;

namespace Soulslike.EditingTools
{
    /// <summary>
    /// Super basic property drawer that essentially extracts the timeFrame property from the BufferedInputBool to show it directly.
    /// Much more space efficient than what the default inspector would do.
    /// </summary>
    [CustomPropertyDrawer(typeof(BufferedInputBool))]
    public class BufferedInputBoolPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //string addition ew i know.
            label.text = label.text + " Buffer Time";
            EditorGUI.PropertyField(position, property.FindPropertyRelative("timeFrame"), label);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
