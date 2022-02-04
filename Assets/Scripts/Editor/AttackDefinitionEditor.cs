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
        private GameObject previewObject;
        private AttackDefinition attackDefinition;
        private int volumeIndex = 0;

        //Setup
        private void OnEnable()
        {
            //Get properties.
            attackDefinition = target as AttackDefinition;
            damageMultiplierProperty    = serializedObject.FindProperty("damageMultiplier");
            hitVolumesProperty          = serializedObject.FindProperty("hitVolumes");
            associatedAnimationProperty = serializedObject.FindProperty("associatedAnimation");
            volumeIndex = 0;
            previewObject = null;
        }
        /**
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            Undo.RecordObject(target, "Edit AttackDefinition");
            EditorGUILayout.PropertyField(damageMultiplierProperty);
            EditorGUILayout.PropertyField(hitVolumesProperty);
            EditorGUILayout.PropertyField(associatedAnimationProperty);
            serializedObject.ApplyModifiedProperties();
            Undo.FlushUndoRecordObjects();

            //the "runtime" settings.
            previewObject = EditorGUILayout.ObjectField(previewObject, typeof(GameObject), allowSceneObjects: true) as GameObject;
            //preview for animation in the editor, which is pretty scuffed ngl.
            AnimationClip animation = attackDefinition.associatedAnimation;
            if (previewObject != null && animation != null)
            {
                float previousAnimTime = animationPlayback;
                animationPlayback = EditorGUILayout.Slider("Preview Animation Time", animationPlayback, 0, 1);
                //detect changes to animation playback
                if(previousAnimTime != animationPlayback)
                {
                    float timeStamp = animation.length * animationPlayback;
                    animation.SampleAnimation(previewObject, timeStamp);
                }
            }

        }

        //Use Handles in the SceneGUI to define all the stuff.
        private void OnSceneGUI()
        {
            //all of this sorta requires a previewObject to work.
            if (previewObject == null)
                return;
            Debug.Log("Scene GUI");
            Transform transform = previewObject.transform;
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
        }*/
    }
}