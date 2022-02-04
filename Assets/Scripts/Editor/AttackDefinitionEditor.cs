using UnityEditor;
using UnityEngine;

namespace Soulslike.EditingTools
{
    [CustomEditor(typeof(AttackDefinition))]
    public class AttackDefinitionEditor : Editor
    {
        //AttackDefinition properties:
        private SerializedProperty damageMultiplierProperty;
        private SerializedProperty hitVolumesProperty;
        //editor only property:
        private SerializedProperty associatedAnimationProperty;

        //properties of the hitVolumes
        private SerializedProperty shapeProperty; 
        private SerializedProperty relativePositionProperty;
        private SerializedProperty relativeRotationProperty;
        private SerializedProperty sizesProperty;

        //"runtime"
        private bool showGizmos = true;
        private float animationPlayback;
        private AttackDefinition attackDefinition;
        private int volumeIndex = 0;
        private bool hasContext = false;
        private GameObject contextObject;

        //Setup
        private void OnEnable()
        {
            if(serializedObject.context != null)
                hasContext = serializedObject.context is GameObject;
            if (hasContext)
                contextObject = serializedObject.context as GameObject;
            
            //Get properties.
            attackDefinition = target as AttackDefinition;
            damageMultiplierProperty    = serializedObject.FindProperty("damageMultiplier");
            hitVolumesProperty          = serializedObject.FindProperty("hitVolumes");
            associatedAnimationProperty = serializedObject.FindProperty("associatedAnimation");
            volumeIndex = 0;
        }
        public override void OnInspectorGUI()
        {
            if (hasContext)
            {
                //base.OnInspectorGUI();
                Undo.RecordObject(target, "Edit AttackDefinition");
                EditorGUILayout.PropertyField(damageMultiplierProperty);
                EditorGUILayout.PropertyField(hitVolumesProperty);
                EditorGUILayout.PropertyField(associatedAnimationProperty);
                serializedObject.ApplyModifiedProperties();
                Undo.FlushUndoRecordObjects();

                //preview for animation in the editor, which is pretty scuffed ngl.
                AnimationClip animation = attackDefinition.associatedAnimation;
                if (contextObject != null && animation != null)
                {
                    float previousAnimTime = animationPlayback;
                    animationPlayback = EditorGUILayout.Slider("Preview Animation Time", animationPlayback, 0, 1);
                    //detect changes to animation playback
                    if (previousAnimTime != animationPlayback)
                    {
                        float timeStamp = animation.length * animationPlayback;
                        animation.SampleAnimation(contextObject, timeStamp);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("AttackDefinition is better edited in context of an entity.", MessageType.Warning);
                Undo.RecordObject(target, "Edit AttackDefinition");
                EditorGUILayout.PropertyField(damageMultiplierProperty);
                EditorGUILayout.PropertyField(hitVolumesProperty);
                EditorGUILayout.PropertyField(associatedAnimationProperty);
                serializedObject.ApplyModifiedProperties();
                Undo.FlushUndoRecordObjects();
            }

        }

        //Use Handles in the SceneGUI to define all the stuff.
        private void OnSceneGUI()
        {
            if (!hasContext)
                return;
            //all of this sorta requires a previewObject to work.
            if (contextObject == null)
                return;
            Debug.Log("Scene GUI");
            Transform transform = contextObject.transform;
            transform.position = Handles.PositionHandle(transform.position, transform.rotation);
            Handles.color = Color.red;
            Handles.DrawLine(transform.position, Vector3.up);
            Handles.DrawWireCube(Vector3.zero, Vector3.one);
            //Draw Gizmos first
            for(int i = 0; i < attackDefinition.hitVolumes.Length; i++)
            {
                attackDefinition.hitVolumes[i].DrawGizmos(transform);
            }
            //now do the editing tools depending on the active tool.
            if(Tools.current == Tool.Move)
            {

            }
            else if( Tools.current == Tool.Rotate)
            {

            }
            else if(Tools.current == Tool.Scale)
            {

            }
        }
    }
}