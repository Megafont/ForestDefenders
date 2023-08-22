using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

////TODO: localization support

////TODO: deal with composites that have parts bound in different control schemes

namespace UnityEngine.InputSystem.Samples.RebindUI
{
    /// <summary>
    /// A reusable component with a self-contained UI for rebinding a single action.
    /// </summary>
    public class RebindActionUI : MonoBehaviour
    {
        /// <summary>
        /// Reference to the action that is to be rebound.
        /// </summary>
        public InputActionReference actionReference
        {
            get => m_Action;
            set
            {
                m_Action = value;
                UpdateActionLabel();
                UpdateBindingDisplay();
            }
        }

        /// <summary>
        /// ID (in string form) of the binding that is to be rebound on the action.
        /// </summary>
        /// <seealso cref="InputBinding.id"/>
        public string bindingId
        {
            get => m_BindingId;
            set
            {
                m_BindingId = value;
                UpdateBindingDisplay();
            }
        }

        public InputBinding.DisplayStringOptions displayStringOptions
        {
            get => m_DisplayStringOptions;
            set
            {
                m_DisplayStringOptions = value;
                UpdateBindingDisplay();
            }
        }

        /// <summary>
        /// Text component that receives the name of the action. Optional.
        /// </summary>
        public TextMeshProUGUI actionLabel
        {
            get => m_ActionLabel;
            set
            {
                m_ActionLabel = value;
                UpdateActionLabel();
            }
        }

        /// <summary>
        /// Text component that receives the display string of the binding. Can be <c>null</c> in which
        /// case the component entirely relies on <see cref="updateBindingUIEvent"/>.
        /// </summary>
        public TextMeshProUGUI bindingText
        {
            get => m_BindingText;
            set
            {
                m_BindingText = value;
                UpdateBindingDisplay();
            }
        }

        /// <summary>
        /// Optional text component that receives a text prompt when waiting for a control to be actuated.
        /// </summary>
        /// <seealso cref="startRebindEvent"/>
        /// <seealso cref="rebindOverlay"/>
        public TextMeshProUGUI rebindPrompt
        {
            get => m_RebindText;
            set => m_RebindText = value;
        }

        /// <summary>
        /// Optional UI that is activated when an interactive rebind is started and deactivated when the rebind
        /// is finished. This is normally used to display an overlay over the current UI while the system is
        /// waiting for a control to be actuated.
        /// </summary>
        /// <remarks>
        /// If neither <see cref="rebindPrompt"/> nor <c>rebindOverlay</c> is set, the component will temporarily
        /// replaced the <see cref="bindingText"/> (if not <c>null</c>) with <c>"Waiting..."</c>.
        /// </remarks>
        /// <seealso cref="startRebindEvent"/>
        /// <seealso cref="rebindPrompt"/>
        public GameObject rebindOverlay
        {
            get => m_RebindOverlay;
            set => m_RebindOverlay = value;
        }

        /// <summary>
        /// Event that is triggered every time the UI updates to reflect the current binding.
        /// This can be used to tie custom visualizations to bindings.
        /// </summary>
        public UpdateBindingUIEvent updateBindingUIEvent
        {
            get
            {
                if (m_UpdateBindingUIEvent == null)
                    m_UpdateBindingUIEvent = new UpdateBindingUIEvent();
                return m_UpdateBindingUIEvent;
            }
        }

        /// <summary>
        /// Event that is triggered when an interactive rebind is started on the action.
        /// </summary>
        public InteractiveRebindEvent startRebindEvent
        {
            get
            {
                if (m_RebindStartEvent == null)
                    m_RebindStartEvent = new InteractiveRebindEvent();
                return m_RebindStartEvent;
            }
        }

        /// <summary>
        /// Event that is triggered when an interactive rebind has been completed or canceled.
        /// </summary>
        public InteractiveRebindEvent stopRebindEvent
        {
            get
            {
                if (m_RebindStopEvent == null)
                    m_RebindStopEvent = new InteractiveRebindEvent();
                return m_RebindStopEvent;
            }
        }

        /// <summary>
        /// When an interactive rebind is in progress, this is the rebind operation controller.
        /// Otherwise, it is <c>null</c>.
        /// </summary>
        public InputActionRebindingExtensions.RebindingOperation ongoingRebind => m_RebindOperation;

        /// <summary>
        /// Return the action and binding index for the binding that is targeted by the component
        /// according to
        /// </summary>
        /// <remarks>CUSTOM CODE: I modified this function to take in an InputActionReference so it can be used for any one, rather than only working for the one this RebindActionUI is set to.</remarks>
        /// <param name="action"></param>
        /// <param name="bindingIndex"></param>
        /// <returns></returns>
        public bool ResolveActionAndBinding(InputActionReference actionRef, out InputAction action, out int bindingIndex)
        {
            bindingIndex = -1;

            action = actionRef?.action;
            if (action == null)
                return false;

            if (string.IsNullOrEmpty(m_BindingId))
                return false;

            // Look up binding index.
            var bindingId = new Guid(m_BindingId);
            bindingIndex = action.bindings.IndexOf(x => x.id == bindingId);
            if (bindingIndex == -1)
            {
                Debug.LogError($"Cannot find binding with ID '{bindingId}' on '{action}'", this);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Trigger a refresh of the currently displayed binding.
        /// </summary>
        public void UpdateBindingDisplay()
        {
            var displayString = string.Empty;
            var deviceLayoutName = default(string);
            var controlPath = default(string);

            // Get display string from action.
            var action = m_Action?.action;
            if (action != null)
            {
                var bindingIndex = action.bindings.IndexOf(x => x.id.ToString() == m_BindingId);
                if (bindingIndex != -1)
                    displayString = action.GetBindingDisplayString(bindingIndex, out deviceLayoutName, out controlPath, displayStringOptions);
            }

            // Set on label (if any).
            if (m_BindingText != null)
                m_BindingText.text = displayString;

            // Give listeners a chance to configure UI in response.
            m_UpdateBindingUIEvent?.Invoke(this, displayString, deviceLayoutName, controlPath);
        }

        /// <summary>
        /// Remove currently applied binding overrides.
        /// </summary>
        public void ResetToDefault()
        {
            // CUSTOM CODE: I modified this function call to take a new first pararmeter. See the comments on the ResolveActionAndBinding() function.
            if (!ResolveActionAndBinding(m_Action, out var action, out var bindingIndex))
                return;


            // I added this code snippet.
            bool actionIsEnabled = action.enabled;
            if (actionIsEnabled)
                action.Disable();


            // CUSTOM CODE: I added this line.
            ResetLinkedActionBindings();

            // CUSTOM CODE. This line is from a tutorial (https://www.youtube.com/watch?v=qXbjyzBlduY). This tutorial also had me comment out the block of code below.
            // Check if another input is bound to the default value of this one before we reset it.            
            ResetBinding(action, bindingIndex);

            
            if (action.bindings[bindingIndex].isComposite)
            {
                // It's a composite. Remove overrides from part bindings.
                for (var i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                    action.RemoveBindingOverride(i);
            }
            else
            {
                action.RemoveBindingOverride(bindingIndex);
            }


            // I added this code. If the action was enabled when this function started, then re-enable it now.
            if (actionIsEnabled)
                action.Enable();


            UpdateBindingDisplay();
        }

        /// <summary>
        /// This function is CUSTOM CODE from the tutorial here: https://www.youtube.com/watch?v=qXbjyzBlduY
        /// It checks if another input is already bound to the default binding of the action we are reseting.
        /// If so, it sets that binding to whatever this action is currently set to, and then sets this action
        /// back to its default key/button/input.
        /// </summary>
        /// <remarks>NOTE: This function has been modified according to the notes in his pinned post under the video description to fix a bug.</remarks>
        private void ResetBinding(InputAction action, int bindingIndex)
        {
            InputBinding newBinding = action.bindings[bindingIndex];
            string oldOverridePath = newBinding.overridePath;

            action.RemoveBindingOverride(bindingIndex);
            int currentIndex = -1;

            foreach(InputAction otherAction in action.actionMap.actions)
            {
                currentIndex++;
                InputBinding currentBinding = action.actionMap.bindings[currentIndex];


                // I added this code snippet.
                bool otherActionIsEnabled = otherAction.enabled;
                if (otherActionIsEnabled)
                    otherAction.Disable();


                if (otherAction == action)
                {
                    if (newBinding.isPartOfComposite)
                    {
                        if (currentBinding.overridePath == newBinding.path)
                        {
                            otherAction.ApplyBindingOverride(currentIndex, oldOverridePath);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                for (int i = 0; i < otherAction.bindings.Count; i++)
                {
                    InputBinding binding = otherAction.bindings[i];
                    if (binding.overridePath == newBinding.path)
                    {
                        otherAction.ApplyBindingOverride(i, oldOverridePath);
                    }
                }


                // I added this code. If otherAction was enabled when this function started, then re-enable it now.
                if (otherActionIsEnabled)
                    otherAction.Enable();
            }
             
        }

        /// <summary>
        /// This function is CUSTOM CODE.
        /// If there is an action in another action map that is linked to this one, then this function will reset that action.
        /// </summary>
        private void ResetLinkedActionBindings()
        {
            // CUSTOM CODE: I modified this function call to take a new first pararmeter. See the comments on the ResolveActionAndBinding() function.
            if (!ResolveActionAndBinding(m_Action, out var action, out var bindingIndex))
                return;


            InputAction linkedAction = m_LinkedAction != null ? m_LinkedAction.action : null;
            InputAction linkedAction2 = m_LinkedAction2 != null ? m_LinkedAction2.action : null;

            ResetLinkedAction(action, bindingIndex, linkedAction);
            ResetLinkedAction(action, bindingIndex, linkedAction2);
        }

        /// <summary>
        /// This function first checks if a linked action is specified. If so, it validates it
        /// by checking that it is not in the same action map as the action this RebindActionUI is set to.
        /// If so, then the linked action is then reset.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="bindingIndex"></param>
        /// <param name="linkedAction"></param>
        private void ResetLinkedAction(InputAction action, int bindingIndex, InputAction linkedAction)
        {
            bool linkedActionIsEnabled = linkedAction.enabled;
            linkedAction.Disable();

            InputBinding bindingToCopy = action.bindings[bindingIndex];
            InputBinding bindingMask = new InputBinding()
            {
                groups = bindingToCopy.groups,
                overridePath = bindingToCopy.overridePath
            };

            int index = linkedAction.GetBindingIndex(bindingMask);
            ResetBinding(m_LinkedAction.action, index);

            if (linkedActionIsEnabled)
                m_LinkedAction.action.Enable();
        }

        /// <summary>
        /// Initiate an interactive rebind that lets the player actuate a control to choose a new binding
        /// for the action.
        /// </summary>
        public void StartInteractiveRebind()
        {
            // CUSTOM CODE: I modified this function call to take a new first pararmeter. See the comments on the ResolveActionAndBinding() function.
            if (!ResolveActionAndBinding(m_Action,out var action, out var bindingIndex))
                return;

            // If the binding is a composite, we need to rebind each part in turn.
            if (action.bindings[bindingIndex].isComposite)
            {
                var firstPartIndex = bindingIndex + 1;
                if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                    PerformInteractiveRebind(action, firstPartIndex, allCompositeParts: true);
            }
            else
            {
                PerformInteractiveRebind(action, bindingIndex);
            }
        }

        private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
        {
            m_RebindOperation?.Cancel(); // Will null out m_RebindOperation.

            void CleanUp()
            {
                m_RebindOperation?.Dispose();
                m_RebindOperation = null;
            }


            // CUSTOM CODE. This line is from a tutorial (https://www.youtube.com/watch?v=qXbjyzBlduY), and I added the second line.
            // Disable the action before rebinding it to prevent errors.
            action.Disable();
            bool actionIsEnabled = action.enabled;
            

            // Configure the rebind.
            m_RebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .OnCancel(
                    operation =>
                    {
                        // CUSTOM CODE. This line is from a tutorial (https://www.youtube.com/watch?v=qXbjyzBlduY). I added the if statement.
                        // Re-enable the action if it was enabled before we rebound it.
                        if (actionIsEnabled)
                            action.Enable();


                        m_RebindStopEvent?.Invoke(this, operation);
                        m_RebindOverlay?.SetActive(false);
                        UpdateBindingDisplay();
                        CleanUp();
                    })
                .OnComplete(
                    operation =>
                    {
                        // CUSTOM CODE. This line is from a tutorial (https://www.youtube.com/watch?v=qXbjyzBlduY). I added the if statement.
                        // Re-enable the action if it was enabled before we rebound it.
                        if (actionIsEnabled)
                            action.Enable();


                        m_RebindOverlay?.SetActive(false);
                        m_RebindStopEvent?.Invoke(this, operation);


                        // CUSTOM CODE. This if block is from a tutorial (https://www.youtube.com/watch?v=qXbjyzBlduY). I added the if statement.
                        // It checks for duplicate bindings in compisite iputs (like up/up/up/up instead of up/down/left/right).
                        if (CheckForDuplicateBindings(action, bindingIndex, allCompositeParts))
                        {
                            action.RemoveBindingOverride(bindingIndex);
                            CleanUp();
                            PerformInteractiveRebind(action, bindingIndex, allCompositeParts);
                            return;
                        }


                        // CUSTOM CODE. I added these lines.
                        ApplyOverrideToLinkedAction(action, bindingIndex, m_LinkedAction);
                        ApplyOverrideToLinkedAction(action, bindingIndex, m_LinkedAction2);


                        UpdateBindingDisplay();
                        CleanUp();

                        // If there's more composite parts we should bind, initiate a rebind
                        // for the next part.
                        if (allCompositeParts)
                        {
                            var nextBindingIndex = bindingIndex + 1;
                            if (nextBindingIndex < action.bindings.Count && action.bindings[nextBindingIndex].isPartOfComposite)
                                PerformInteractiveRebind(action, nextBindingIndex, true);
                        }
                    });

            // If it's a part binding, show the name of the part in the UI.
            var partName = default(string);
            if (action.bindings[bindingIndex].isPartOfComposite)
                partName = $"Binding '{action.bindings[bindingIndex].name}'. ";

            // Bring up rebind overlay, if we have one.
            m_RebindOverlay?.SetActive(true);
            if (m_RebindText != null)
            {
                var text = !string.IsNullOrEmpty(m_RebindOperation.expectedControlType)
                    ? $"{partName}Waiting for {m_RebindOperation.expectedControlType} input..."
                    : $"{partName}Waiting for input...";
                m_RebindText.text = text;
            }

            // If we have no rebind overlay and no callback but we have a binding text label,
            // temporarily set the binding text label to "<Waiting>".
            if (m_RebindOverlay == null && m_RebindText == null && m_RebindStartEvent == null && m_BindingText != null)
                m_BindingText.text = "<Waiting...>";

            // Give listeners a chance to act on the rebind starting.
            m_RebindStartEvent?.Invoke(this, m_RebindOperation);

            m_RebindOperation.Start();
        }

        /// <summary>
        /// This function is CUSTOM CODE.
        /// If there is an action in another action map that is linked to this one, then this function will apply
        /// an input binding from this action to it.
        /// </summary>
        /// <remarks>
        /// For example, Forest Defenders uses this function to make it so the Exit Build Mode action in the
        /// Build Mode input action map always has the same key assigned to it as the Enter Build Mode action in
        /// the Player input action map.
        /// </remarks>
        private void ApplyOverrideToLinkedAction(InputAction action, int bindingIndex, InputActionReference linkedAction)
        {
            // This code first checks if a linked action is specified. 
            // If so, then the linked action is bound to the same key as this action.
            if (linkedAction != null)
            {
                bool linkedActionIsEnabled = linkedAction.action.enabled;

                linkedAction.action.Disable();

                InputBinding bindingToCopy = action.bindings[bindingIndex];
                InputBinding newBinding = new InputBinding()
                {
                    groups = bindingToCopy.groups,
                    overridePath = bindingToCopy.overridePath
                };

                linkedAction.action.ApplyBindingOverride(newBinding);

                if (linkedActionIsEnabled)
                    linkedAction.action.Enable();

            }

        }


        /// <summary>
        /// This function is CUSTOM CODE from the tutorial here: https://www.youtube.com/watch?v=qXbjyzBlduY
        /// It prevents us from being able to assign duplicate inputs to a compisite binding (like up/up/up/up instead of up/down/left/right).
        /// </summary>
        /// <remarks>NOTE: This function has been modified according to the notes in his pinned post under the video description to fix a bug.</remarks>
        /// <param name="action"></param>
        /// <param name="bindingIndex"></param>
        /// <param name="allCompositeParts"></param>
        /// <returns></returns>
        private bool CheckForDuplicateBindings(InputAction action, int bindingIndex, bool allCompositeParts = false)
        {
            InputBinding newBinding = action.bindings[bindingIndex];
            int currentIndex = -1;

            foreach (InputBinding binding in action.actionMap.bindings)
            {
                currentIndex++;

                //Debug.Log($"{currentIndex}  {binding.name}  {binding.path}  {binding.effectivePath}  {binding.overridePath}");

                if (binding.action == newBinding.action)
                {
                    // I had to change this if statement to get it to stop causing false positives. It used to be "if (binding.isPartOfComposite && currentIndex != bindingIndex)"                    
                    if (binding.isPartOfComposite && binding.id != newBinding.id)
                    {
                        if (binding.effectivePath == newBinding.effectivePath)
                        {                            
                            //Debug.Log($"{bindingIndex}  {binding.name}  {binding.path}  {binding.effectivePath}  {binding.overridePath}   |   {currentIndex}  {newBinding.name}  {newBinding.path}  {newBinding.effectivePath}  {newBinding.overridePath}");
                            LogDuplicateBindingWarning(action, newBinding);
                            return true;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                if (binding.effectivePath == newBinding.effectivePath)
                {
                    LogDuplicateBindingWarning(action, newBinding);
                    return true;
                }
            }


            if (allCompositeParts)
            {
                for (int i = 1; i < bindingIndex; i++)
                {
                    if (action.bindings[i].effectivePath == newBinding.overridePath)
                    {
                        LogDuplicateBindingWarning(action, newBinding);
                        return true;
                    }
                }
            }


            return false;
        }

        /// <summary>
        /// This is a CUSTOM FUNCTION I wrote to remove duplicate code.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="newBinding"></param>
        private void LogDuplicateBindingWarning(InputAction action, InputBinding newBinding)
        {
            Debug.LogWarning($"Duplicate key binding found while binding action {action.name}: {newBinding.effectivePath}");
        }

        protected void OnEnable()
        {
            if (s_RebindActionUIs == null)
                s_RebindActionUIs = new List<RebindActionUI>();
            s_RebindActionUIs.Add(this);
            if (s_RebindActionUIs.Count == 1)
                InputSystem.onActionChange += OnActionChange;

            // CUSTOM CODE: I added this line to make sure the binding display gets updated whenever this UI element gets re-enabled (in other words when the controls dialog is opened).
            UpdateBindingDisplay();
        }

        protected void OnDisable()
        {
            m_RebindOperation?.Dispose();
            m_RebindOperation = null;

            s_RebindActionUIs.Remove(this);
            if (s_RebindActionUIs.Count == 0)
            {
                s_RebindActionUIs = null;
                InputSystem.onActionChange -= OnActionChange;
            }
        }

        // When the action system re-resolves bindings, we want to update our UI in response. While this will
        // also trigger from changes we made ourselves, it ensures that we react to changes made elsewhere. If
        // the user changes keyboard layout, for example, we will get a BoundControlsChanged notification and
        // will update our UI to reflect the current keyboard layout.
        private static void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.BoundControlsChanged)
                return;

            var action = obj as InputAction;
            var actionMap = action?.actionMap ?? obj as InputActionMap;
            var actionAsset = actionMap?.asset ?? obj as InputActionAsset;

            for (var i = 0; i < s_RebindActionUIs.Count; ++i)
            {
                var component = s_RebindActionUIs[i];
                var referencedAction = component.actionReference?.action;
                if (referencedAction == null)
                    continue;

                if (referencedAction == action ||
                    referencedAction.actionMap == actionMap ||
                    referencedAction.actionMap?.asset == actionAsset)
                    component.UpdateBindingDisplay();
            }
        }

        [Tooltip("Reference to action that is to be rebound from the UI.")]
        [SerializeField]
        private InputActionReference m_Action;

        // CUSTOM CODE. I added this field.
        [Tooltip("A reference to an action in another action map that is linked to this one. In other words, when this action is rebound, the linked action will be rebound to the same key.")]
        [SerializeField]
        private InputActionReference m_LinkedAction;
        [Tooltip("A reference to an action in another action map that is linked to this one. In other words, when this action is rebound, the linked action will be rebound to the same key.")]
        [SerializeField]
        private InputActionReference m_LinkedAction2;

        [SerializeField]
        private string m_BindingId;

        [SerializeField]
        private InputBinding.DisplayStringOptions m_DisplayStringOptions;

        [Tooltip("Text label that will receive the name of the action. Optional. Set to None to have the "
            + "rebind UI not show a label for the action.")]
        [SerializeField]
        private TextMeshProUGUI m_ActionLabel;

        [Tooltip("Text label that will receive the current, formatted binding string.")]
        [SerializeField]
        private TextMeshProUGUI m_BindingText;

        [Tooltip("Optional UI that will be shown while a rebind is in progress.")]
        [SerializeField]
        private GameObject m_RebindOverlay;

        [Tooltip("Optional text label that will be updated with prompt for user input.")]
        [SerializeField]
        private TextMeshProUGUI m_RebindText;


        // The next two fields are CUSTOM CODE from this tutorial: https://www.youtube.com/watch?v=qXbjyzBlduY
        [Tooltip("Optional bool field which allows you to override the action label with your own text.")]
        public bool m_OverrideActionLabel;

        [Tooltip("The text that will be displayed for the action label if OverideActionLabel is enabled.")]
        [SerializeField]
        private string m_ActionLabelString;


        [Tooltip("Event that is triggered when the way the binding is display should be updated. This allows displaying "
            + "bindings in custom ways, e.g. using images instead of text.")]
        [SerializeField]
        private UpdateBindingUIEvent m_UpdateBindingUIEvent;

        [Tooltip("Event that is triggered when an interactive rebind is being initiated. This can be used, for example, "
            + "to implement custom UI behavior while a rebind is in progress. It can also be used to further "
            + "customize the rebind.")]
        [SerializeField]
        private InteractiveRebindEvent m_RebindStartEvent;

        [Tooltip("Event that is triggered when an interactive rebind is complete or has been aborted.")]
        [SerializeField]
        private InteractiveRebindEvent m_RebindStopEvent;

        private InputActionRebindingExtensions.RebindingOperation m_RebindOperation;

        private static List<RebindActionUI> s_RebindActionUIs;

        // We want the label for the action name to update in edit mode, too, so
        // we kick that off from here.
        #if UNITY_EDITOR
        protected void OnValidate()
        {
            UpdateActionLabel();
            UpdateBindingDisplay();
        }

        #endif

        private void UpdateActionLabel()
        {
            if (m_ActionLabel != null)
            {
                var action = m_Action?.action;


                // This block is CUSTOM CODE from this tutorial: https://www.youtube.com/watch?v=qXbjyzBlduY
                if (m_OverrideActionLabel)
                {
                    m_ActionLabel.text = m_ActionLabelString;
                }
                else
                {
                    m_ActionLabelString = string.Empty;

                    // This line used to be the only thing here instead of this if block.
                    m_ActionLabel.text = action != null ? action.name : string.Empty;
                }
            }
        }

        [Serializable]
        public class UpdateBindingUIEvent : UnityEvent<RebindActionUI, string, string, string>
        {
        }

        [Serializable]
        public class InteractiveRebindEvent : UnityEvent<RebindActionUI, InputActionRebindingExtensions.RebindingOperation>
        {
        }
    }
}
