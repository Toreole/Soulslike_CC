using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Soulslike.EditingTools
{
    [CustomEditor(typeof(PlayerMachine))]
    public class PlayerMachineEditor : Editor
    {
        //PlayerMachine baseline.
        private SerializedProperty animatorProperty;
        private SerializedProperty cameraControllerProperty;
        private SerializedProperty characterControllerProperty;
        //attack definitions property
        private SerializedProperty basicAttacksProperty;

        //movement settings
        private SerializedProperty walkSpeedProperty;
        private SerializedProperty runSpeedProperty;
        private SerializedProperty rollInputTimeFrame;

        //cached editor that will be an instance of the AttackDefinitionEditor.
        private Editor embeddedEditor;

        //relative properties of the attackdefinition we are currently editing with the SceneGUI.
        private SerializedProperty hitVolumesProperty;

        private SerializedProperty hitVolumeSizeProperty;
        private SerializedProperty hitVolumePositionProperty;
        private SerializedProperty hitVolumeRotationProperty;

        //other runtime settings.
        private bool foldoutAttacks = false;
        private PlayerMachine playerMachine;
        private SerializedProperty editingAttack = null;
        private bool foldoutMovement = false;

        //LAYOUT OPTIONS.
        static readonly GUILayoutOption smallButton = GUILayout.MaxWidth(45);
        private Texture2D darkGrayTexture;
        private GUIStyle foldoutStyle;


        //Initialize the basics for editing.
        private void OnEnable()
        {
            //setup the playerMachine for editing.
            playerMachine = target as PlayerMachine;
            playerMachine.selectedAttackIndex = 0;
            playerMachine.showAttackHitbox = false;
            //get properties.
            animatorProperty = serializedObject.FindProperty("animator");
            cameraControllerProperty = serializedObject.FindProperty("cameraController");
            basicAttacksProperty = serializedObject.FindProperty("basicAttacks");
            characterControllerProperty = serializedObject.FindProperty("characterController");
            //movement properties.
            walkSpeedProperty = serializedObject.FindProperty("walkSpeed");
            runSpeedProperty = serializedObject.FindProperty("runSpeed");
            rollInputTimeFrame = serializedObject.FindProperty("rollInputTimeFrame");
            editingAttack = null;
        }

        public override void OnInspectorGUI()
        {
            SetupStyles();
            Undo.RecordObject(target, "Edit PlayerMachine");
            //base.OnInspectorGUI();
            //The basics.
            EditorGUILayout.PropertyField(animatorProperty);
            EditorGUILayout.PropertyField(cameraControllerProperty);
            EditorGUILayout.PropertyField(characterControllerProperty);
            //EditorGUILayout.PropertyField(basicAttacksProperty);
            DrawMovementProperties();

            DrawAttackInspector();
            Undo.FlushUndoRecordObjects();
            serializedObject.ApplyModifiedProperties();
        }

        private void SetupStyles()
        {
            if (foldoutStyle != null)
                return;

            //foldoutStyle = new GUIStyle(EditorStyles.foldout);
            //foldoutStyle.onNormal = new GUIStyleState();
            //foldoutStyle.onNormal.background = Texture2D.blackTexture; //THIS CHANGES THE TEXT COLOUR??? Unity can seriously suck my nuts. fuck unity.
        }

        /// <summary>
        /// Draws the editor for everything connected to movement.
        /// </summary>
        private void DrawMovementProperties()
        {
            if(foldoutMovement = EditorGUILayout.Foldout(foldoutMovement, "Movement Variables", true))
            {
                using(new EditorGUI.IndentLevelScope(1))
                {
                    EditorGUILayout.PropertyField(walkSpeedProperty);
                    EditorGUILayout.PropertyField(runSpeedProperty);
                    EditorGUILayout.PropertyField(rollInputTimeFrame);
                }
            }
        }

        /// <summary>
        /// Draw the basicAttacksProperty array with "EDIT" buttons on the side.
        /// </summary>
        private void DrawAttackInspector()
        {
            //Put everything in a box to at least somewhat differentiate it from everything around it.
            using (new EditorGUILayout.VerticalScope())
            {
                int arrayLength = basicAttacksProperty.arraySize;
                if (arrayLength > 0)
                {
                    //Foldout for the attacks array, as to not make the editor too large.
                    foldoutAttacks = EditorGUILayout.Foldout(foldoutAttacks, "Attacks", true);
                    if (foldoutAttacks)
                    {
                        using (new EditorGUI.IndentLevelScope(1))
                        {
                            DrawAttackArray(arrayLength);
                        }
                        if (editingAttack != null)
                        {
                            DrawEmbeddedAttackDefinitionEditor();
                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Basic Attacks Array is empty.");
                }
            }
        }

        private void DrawAttackArray(int arrayLength)
        {
            for (int i = 0; i < arrayLength; i++)
            {
                //1. fetch the element.
                var arrayElement = basicAttacksProperty.GetArrayElementAtIndex(i);
                var rect = EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PropertyField(arrayElement);
                    //draw the button
                    if (GUILayout.Button("EDIT", smallButton))
                    {
                        StartEditingAttack(arrayElement);
                        playerMachine.selectedAttackIndex = i;
                        playerMachine.showAttackHitbox = true;
                    }
                    if(GUILayout.Button("DELETE", smallButton))
                    {
                        basicAttacksProperty.DeleteArrayElementAtIndex(i);
                        return;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            //Adding new Elements:
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Add Attack:");
            AttackDefinition addElement = EditorGUILayout.ObjectField(obj: null, typeof(AttackDefinition), false) as AttackDefinition;
            //check if we are adding something.
            if(addElement != null)
            {
                basicAttacksProperty.InsertArrayElementAtIndex(arrayLength);
                basicAttacksProperty.GetArrayElementAtIndex(arrayLength).objectReferenceValue = addElement;
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Takes a property (index) of basicAttacks and finds the related properties for editing.
        /// </summary>
        private void StartEditingAttack(SerializedProperty attack)
        {
            editingAttack = attack;
            if(editingAttack != null)
            {
                //this is an array.
                hitVolumesProperty = attack.FindPropertyRelative("hitVolumes");
            }
        }

        private void DrawEmbeddedAttackDefinitionEditor()
        {
            EditorGUILayout.LabelField(editingAttack.displayName);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            Editor.CreateCachedEditorWithContext(targetObject: editingAttack.objectReferenceValue, (target as MonoBehaviour).gameObject, typeof(AttackDefinitionEditor), ref embeddedEditor);
            embeddedEditor.OnInspectorGUI();

            EditorGUILayout.EndVertical();
        }

        private void OnDisable()
        {
            
        }

        private void OnSceneGUI()
        {
            if (embeddedEditor != null && editingAttack != null)
                (embeddedEditor as AttackDefinitionEditor).OnSceneGUI();
        }
    }
}