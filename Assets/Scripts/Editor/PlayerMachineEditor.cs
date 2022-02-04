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
        //attack definitions property
        private SerializedProperty basicAttacksProperty;

        //cached editor that will be an instance of the AttackDefinitionEditor.
        private Editor embeddedEditor;

        //other runtime settings.
        private bool foldoutAttacks = false;
        private PlayerMachine playerMachine;
        private SerializedProperty editingAttack = null;

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
            EditorGUILayout.PropertyField(basicAttacksProperty);

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
        /// Draw the basicAttacksProperty array with "EDIT" buttons on the side.
        /// </summary>
        private void DrawAttackInspector()
        {
            //Put everything in a box to at least somewhat differentiate it from everything around it.
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                int arrayLength = basicAttacksProperty.arraySize;
                if (arrayLength > 0)
                {
                    //Foldout for the attacks array, as to not make the editor too large.
                    foldoutAttacks = EditorGUILayout.Foldout(foldoutAttacks, "Attacks", true, EditorStyles.foldoutHeader);
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
                        editingAttack = arrayElement;
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

        private void DrawEmbeddedAttackDefinitionEditor()
        {
            EditorGUILayout.LabelField("---");
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
            //This is only for handles.
        }
    }
}