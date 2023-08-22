#if UNITY_EDITOR
using System.Linq;
using UnityEditor;

////TODO: support multi-object editing

namespace UnityEngine.InputSystem.Samples.RebindUI
{
    /// <summary>
    /// A custom inspector for <see cref="RebindActionUI"/> which provides a more convenient way for
    /// picking the binding which to rebind.
    /// </summary>
    [CustomEditor(typeof(RebindActionUI))]
    public class RebindActionUIEditor : UnityEditor.Editor
    {
        // The next line is CUSTOM CODE from this tutorial: https://www.youtube.com/watch?v=qXbjyzBlduY
        RebindActionUI m_RebindActionUI;


        protected void OnEnable()
        {
            // The next line is CUSTOM CODE from this tutorial: https://www.youtube.com/watch?v=qXbjyzBlduY
            m_RebindActionUI = (RebindActionUI) target;


            m_ActionProperty = serializedObject.FindProperty("m_Action");

            // CUSTOM CODE. I added these two lines.
            m_LinkedActionProperty = serializedObject.FindProperty("m_LinkedAction");
            m_LinkedAction2Property = serializedObject.FindProperty("m_LinkedAction2");

            m_BindingIdProperty = serializedObject.FindProperty("m_BindingId");
            m_ActionLabelProperty = serializedObject.FindProperty("m_ActionLabel");
            m_BindingTextProperty = serializedObject.FindProperty("m_BindingText");
            m_RebindOverlayProperty = serializedObject.FindProperty("m_RebindOverlay");
            m_RebindTextProperty = serializedObject.FindProperty("m_RebindText");
            m_UpdateBindingUIEventProperty = serializedObject.FindProperty("m_UpdateBindingUIEvent");
            m_RebindStartEventProperty = serializedObject.FindProperty("m_RebindStartEvent");
            m_RebindStopEventProperty = serializedObject.FindProperty("m_RebindStopEvent");
            m_DisplayStringOptionsProperty = serializedObject.FindProperty("m_DisplayStringOptions");


            // This block is CUSTOM CODE from this tutorial: https://www.youtube.com/watch?v=qXbjyzBlduY
            m_ActionOverrideProperty = serializedObject.FindProperty("m_OverrideActionLabel");
            m_ActionOverrideStringProperty = serializedObject.FindProperty("m_ActionLabelString");


            RefreshBindingOptions();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            // Binding section.
            EditorGUILayout.LabelField(m_BindingLabel, Styles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_ActionProperty);

                // CUSTOM CODE. I added this block and the following if statement. It shows the new linked action property, and validates it.
                InputActionReference actionRef = (InputActionReference) m_ActionProperty.objectReferenceValue;
                InputAction action = actionRef != null ? actionRef.action : null;

                EditorGUILayout.PropertyField(m_LinkedActionProperty);
                InputActionReference linkedActionRef = (InputActionReference) m_LinkedActionProperty.objectReferenceValue;
                InputAction linkedAction = linkedActionRef != null ? linkedActionRef.action : null;
                
                if (action != null && linkedAction != null)
                {
                    if (linkedAction.actionMap == action.actionMap)
                    {
                        Debug.LogError($"RebindActionUIEditor.cs: You cannot set the linked action for input action \"{action.name}\" to \"{linkedAction.name}\", because it is in the same action map as \"{action.name}\"!");

                        // Clear this property since the specified action is invalid.
                        m_LinkedActionProperty.objectReferenceValue = null;
                    }
                }


                // CUSTOM CODE. I added this block and the following if statement. It shows the new linked action 2 property, and validates it.
                EditorGUILayout.PropertyField(m_LinkedAction2Property);
                InputActionReference linkedActionRef2 = (InputActionReference) m_LinkedAction2Property.objectReferenceValue;
                InputAction linkedAction2 = linkedActionRef2 != null ? linkedActionRef2.action : null;

                if (action != null && linkedAction2 != null)
                {
                    if (linkedAction2.actionMap == action.actionMap)
                    {
                        Debug.LogError($"RebindActionUIEditor.cs: You cannot set the second linked action for input action \"{action.name}\" to \"{linkedAction2.name}\", because it is in the same action map as \"{action.name}\"!");

                        // Clear this property since the specified action is invalid.
                        m_LinkedAction2Property.objectReferenceValue = null;
                    }
                }


                var newSelectedBinding = EditorGUILayout.Popup(m_BindingLabel, m_SelectedBindingOption, m_BindingOptions);
                if (newSelectedBinding != m_SelectedBindingOption)
                {
                    var bindingId = m_BindingOptionValues[newSelectedBinding];
                    m_BindingIdProperty.stringValue = bindingId;
                    m_SelectedBindingOption = newSelectedBinding;
                }

                var optionsOld = (InputBinding.DisplayStringOptions)m_DisplayStringOptionsProperty.intValue;
                var optionsNew = (InputBinding.DisplayStringOptions)EditorGUILayout.EnumFlagsField(m_DisplayOptionsLabel, optionsOld);
                if (optionsOld != optionsNew)
                    m_DisplayStringOptionsProperty.intValue = (int)optionsNew;
            }

            // UI section.
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(m_UILabel, Styles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_ActionLabelProperty);
                EditorGUILayout.PropertyField(m_BindingTextProperty);
                EditorGUILayout.PropertyField(m_RebindOverlayProperty);
                EditorGUILayout.PropertyField(m_RebindTextProperty);
            }


            // Customize UI Section.
            // The block is CUSTOM CODE from this tutorial: https://www.youtube.com/watch?v=qXbjyzBlduY
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(m_CustomizeUILabel, Styles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_ActionOverrideProperty);
                if (m_RebindActionUI.m_OverrideActionLabel)
                    EditorGUILayout.PropertyField(m_ActionOverrideStringProperty);
            }


            // Events section.
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(m_EventsLabel, Styles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_RebindStartEventProperty);
                EditorGUILayout.PropertyField(m_RebindStopEventProperty);
                EditorGUILayout.PropertyField(m_UpdateBindingUIEventProperty);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                RefreshBindingOptions();
            }
        }

        protected void RefreshBindingOptions()
        {
            var actionReference = (InputActionReference)m_ActionProperty.objectReferenceValue;
            var action = actionReference?.action;

            if (action == null)
            {
                m_BindingOptions = new GUIContent[0];
                m_BindingOptionValues = new string[0];
                m_SelectedBindingOption = -1;
                return;
            }

            var bindings = action.bindings;
            var bindingCount = bindings.Count;

            m_BindingOptions = new GUIContent[bindingCount];
            m_BindingOptionValues = new string[bindingCount];
            m_SelectedBindingOption = -1;

            var currentBindingId = m_BindingIdProperty.stringValue;
            for (var i = 0; i < bindingCount; ++i)
            {
                var binding = bindings[i];
                var bindingId = binding.id.ToString();
                var haveBindingGroups = !string.IsNullOrEmpty(binding.groups);

                // If we don't have a binding groups (control schemes), show the device that if there are, for example,
                // there are two bindings with the display string "A", the user can see that one is for the keyboard
                // and the other for the gamepad.
                var displayOptions =
                    InputBinding.DisplayStringOptions.DontUseShortDisplayNames | InputBinding.DisplayStringOptions.IgnoreBindingOverrides;
                if (!haveBindingGroups)
                    displayOptions |= InputBinding.DisplayStringOptions.DontOmitDevice;

                // Create display string.
                var displayString = action.GetBindingDisplayString(i, displayOptions);

                // If binding is part of a composite, include the part name.
                if (binding.isPartOfComposite)
                    displayString = $"{ObjectNames.NicifyVariableName(binding.name)}: {displayString}";

                // Some composites use '/' as a separator. When used in popup, this will lead to to submenus. Prevent
                // by instead using a backlash.
                displayString = displayString.Replace('/', '\\');

                // If the binding is part of control schemes, mention them.
                if (haveBindingGroups)
                {
                    var asset = action.actionMap?.asset;
                    if (asset != null)
                    {
                        var controlSchemes = string.Join(", ",
                            binding.groups.Split(InputBinding.Separator)
                                .Select(x => asset.controlSchemes.FirstOrDefault(c => c.bindingGroup == x).name));

                        displayString = $"{displayString} ({controlSchemes})";
                    }
                }

                m_BindingOptions[i] = new GUIContent(displayString);
                m_BindingOptionValues[i] = bindingId;

                if (currentBindingId == bindingId)
                    m_SelectedBindingOption = i;
            }
        }

        private SerializedProperty m_ActionProperty;
        
        // CUSTOM CODE: I added these two fields.
        private SerializedProperty m_LinkedActionProperty;
        private SerializedProperty m_LinkedAction2Property;

        private SerializedProperty m_BindingIdProperty;
        private SerializedProperty m_ActionLabelProperty;
        private SerializedProperty m_BindingTextProperty;
        private SerializedProperty m_RebindOverlayProperty;
        private SerializedProperty m_RebindTextProperty;
        private SerializedProperty m_RebindStartEventProperty;
        private SerializedProperty m_RebindStopEventProperty;
        private SerializedProperty m_UpdateBindingUIEventProperty;
        private SerializedProperty m_DisplayStringOptionsProperty;

        // The next two lines are CUSTOM CODE from this tutorial: https://www.youtube.com/watch?v=qXbjyzBlduY
        private SerializedProperty m_ActionOverrideProperty;
        private SerializedProperty m_ActionOverrideStringProperty;

        private GUIContent m_BindingLabel = new GUIContent("Binding");
        private GUIContent m_DisplayOptionsLabel = new GUIContent("Display Options");

        // The next line is CUSTOM CODE from this tutorial: https://www.youtube.com/watch?v=qXbjyzBlduY
        private GUIContent m_CustomizeUILabel = new GUIContent("Customize UI");

        private GUIContent m_UILabel = new GUIContent("UI");
        private GUIContent m_EventsLabel = new GUIContent("Events");
        private GUIContent[] m_BindingOptions;
        private string[] m_BindingOptionValues;
        private int m_SelectedBindingOption;

        private static class Styles
        {
            public static GUIStyle boldLabel = new GUIStyle("MiniBoldLabel");
        }
    }
}
#endif
