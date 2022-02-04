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

            Undo.undoRedoPerformed += this.Repaint;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= this.Repaint;
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
        internal void OnSceneGUI()
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

            //start recording the changes made to the attack.
            Undo.RecordObject(target, "Edit AttackDefinition from PlayerMachine Context");
            for (int i = 0; i < hitVolumesProperty.arraySize; i++)
            {
                var volume = hitVolumesProperty.GetArrayElementAtIndex(i);
                if (volume == null)
                    continue;
                //Find related properties
                relativePositionProperty = volume.FindPropertyRelative("relativePosition");
                relativeRotationProperty = volume.FindPropertyRelative("relativeRotation");
                sizesProperty = volume.FindPropertyRelative("sizes");
                //get the values.
                Vector3 relativePosition = relativePositionProperty.vector3Value;
                Quaternion relativeRotation = relativeRotationProperty.quaternionValue;
                Vector3 sizes = sizesProperty.vector3Value;
                //processing:
                //relative to worldspace.
                Vector3 worldPosition = transform.TransformPoint(relativePosition);
                Quaternion worldRotation = transform.rotation * relativeRotation;

                if (Tools.current == Tool.Move)
                {
                    Vector3 changedPosition = Handles.PositionHandle(worldPosition, worldRotation);
                    //set the properties value to the position relative to the player.
                    relativePositionProperty.vector3Value = transform.InverseTransformPoint(changedPosition);
                }
                else if (Tools.current == Tool.Rotate)
                {
                    Quaternion changedRotation = Handles.RotationHandle(relativeRotation, worldPosition);
                    relativeRotationProperty.quaternionValue = changedRotation;
                }
                else if (Tools.current == Tool.Scale)
                {
                    Vector3 changedSize = Handles.ScaleHandle(sizes, worldPosition, worldRotation);
                    sizesProperty.vector3Value = changedSize;
                }
            }

            serializedObject.ApplyModifiedProperties();
            Undo.FlushUndoRecordObjects();
        }
    }
}