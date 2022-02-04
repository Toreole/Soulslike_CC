using UnityEditor;
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



        private void OnEnable()
        {

        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }

        private void OnSceneGUI()
        {
            //This is only for handles.
        }
    }
}