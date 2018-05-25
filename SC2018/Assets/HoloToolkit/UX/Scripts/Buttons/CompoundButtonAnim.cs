// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System;
using HoloToolkit.Unity;
using System.Collections;

namespace HoloToolkit.Unity.Buttons
{
    /// <summary>
    /// Anim controller button offers as simple way to link button states to animation controller parameters
    /// </summary>
    [RequireComponent(typeof(CompoundButton))]
    public class CompoundButtonAnim : MonoBehaviour
    {

        public GameObject ObjToManip;

        public string direction;                        //used to differentiate which button is pressed
                   
        static bool singleInstance = false;             //makes sure, we only have one instance of the clone

        IEnumerator Waiting()                           //coroutine to stop the user from clicking the button too often and by that spawning too much clones
        {
            
            yield return new WaitForSeconds(1.2f);
            singleInstance = false;
            
        }



        [DropDownComponent]
        public Animator TargetAnimator;

        /// <summary>
        /// List of animation actions
        /// </summary>
        [HideInMRTKInspector]
        public AnimatorControllerAction[] AnimActions;

        private void Awake() {
            GetComponent<Button>().StateChange += StateChange;
            if (TargetAnimator == null) {
                TargetAnimator = GetComponent<Animator>();
            }
        }

        /// <summary>
        /// State change
        /// </summary>
        void StateChange(ButtonStateEnum newState) {
            if (TargetAnimator == null) {
                return;
            }

            if (AnimActions == null) {
                return;
            }

            if (!gameObject.activeSelf)
                return;



            //directly sets singleInstance to true, so no other clone can be spawned

            //Vector3 pos_xVec = GameObject.Find("TransCube").transform.position + new Vector3(0.2f, 0, 0);           //takes pos from current cube and adds another vector for new position
            //GameObject CloneCube = Instantiate(GameObject.Find("TransCube"), pos_xVec, Quaternion.identity);        //instantiates "CloneCube" and slightly different position 


            //Destroy(GameObject.Find("TransCube"), 0.0f);                                                            //destroy old cube
            //CloneCube.name = "TransCube";                                                                           //rename CloneCube so it can be used as reference in the next clone process
            //StartCoroutine(Waiting());                                                                        

            //WorldAnchorManager.Instance.RemoveAnchor(GameObject.Find("TransCube"));
            ObjToManip.transform.Translate(0.1f,0,0);
                    //WorldAnchorManager.Instance.AttachAnchor(GameObject.Find("TransCube"));
                    StartCoroutine(Waiting());

                
                



                /*switch (direction)
                {

                    case "pos x":
                        
                        Vector3 pos_xVec = GameObject.Find("TransCube").transform.position + new Vector3(0.1f, 0, 0);
                        GameObject CloneCube = Instantiate(GameObject.Find("TransCube"), pos_xVec, Quaternion.identity);
                        

                        Destroy(GameObject.Find("TransCube"), 0.0f);
                        CloneCube.name = "TransCube";
                        StartCoroutine(Waiting());

                        break;

                    case "neg x":

                        break;

                    case "pos y":

                        break;

                    case "neg y":

                        break;

                    case "pos z":

                        break;

                    case "neg z":

                        break;

            }*/





                //filter out which button is pressed and move in the right direction

                /*case "pos x":
                    WorldAnchorManager.Instance.RemoveAnchor(TransCube);
                    TransCube.transform.Translate(0.01f, 0, 0);
                    WorldAnchorManager.Instance.AttachAnchor(TransCube);
                    break;

                case "neg x":
                    WorldAnchorManager.Instance.RemoveAnchor(TransCube);
                    TransCube.transform.Translate(-0.01f, 0, 0);
                    WorldAnchorManager.Instance.AttachAnchor(TransCube);
                    break;

                case "pos y":
                    WorldAnchorManager.Instance.RemoveAnchor(TransCube);
                    TransCube.transform.Translate(0, 0.01f, 0);
                    WorldAnchorManager.Instance.AttachAnchor(TransCube);
                    break;

                case "neg y":
                    WorldAnchorManager.Instance.RemoveAnchor(TransCube);
                    TransCube.transform.Translate(0, -0.01f, 0);
                    WorldAnchorManager.Instance.AttachAnchor(TransCube);
                    break;

                case "pos z":                 
                    WorldAnchorManager.Instance.RemoveAnchor(TransCube);
                    TransCube.transform.Translate(0, 0, 0.01f);
                    WorldAnchorManager.Instance.AttachAnchor(TransCube);
                    break;

                case "neg z":
                    WorldAnchorManager.Instance.RemoveAnchor(TransCube);
                    TransCube.transform.Translate(0, 0, -0.01f);
                    WorldAnchorManager.Instance.AttachAnchor(TransCube);
                    break;*/

            
                
            
        
         

            for (int i = 0; i < AnimActions.Length; i++) {
                if (AnimActions[i].ButtonState == newState) {
                    if (!string.IsNullOrEmpty(AnimActions[i].ParamName)) {
                        switch (AnimActions[i].ParamType) {
                            case AnimatorControllerParameterType.Bool:
                                TargetAnimator.SetBool(AnimActions[i].ParamName, AnimActions[i].BoolValue);
                                break;

                            case AnimatorControllerParameterType.Float:
                                TargetAnimator.SetFloat(AnimActions[i].ParamName, AnimActions[i].FloatValue);
                                break;

                            case AnimatorControllerParameterType.Int:
                                TargetAnimator.SetInteger(AnimActions[i].ParamName, AnimActions[i].IntValue);
                                break;

                            case AnimatorControllerParameterType.Trigger:
                                TargetAnimator.SetTrigger(AnimActions[i].ParamName);
                                break;

                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    break;
                }
            }
        }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(CompoundButtonAnim))]
        public class CustomEditor : MRTKEditor
        {
            /// <summary>
            /// Draw a custom editor for AnimatorControllerActions to make them easier to edit
            /// </summary>
            protected override void DrawCustomFooter() {

                CompoundButtonAnim acb = (CompoundButtonAnim)target;
                Animator animator = acb.TargetAnimator;
                AnimatorControllerParameter[] animParams = null;

                // Validate the AnimButton controls - make sure there's one control for each button state
                ButtonStateEnum[] buttonStates = (ButtonStateEnum[])System.Enum.GetValues(typeof(ButtonStateEnum));
                if (acb.AnimActions == null || acb.AnimActions.Length != buttonStates.Length) {
                    acb.AnimActions = new AnimatorControllerAction[buttonStates.Length];
                }

                // Don't allow user to change setup during play mode
                if (!Application.isPlaying && !string.IsNullOrEmpty (acb.gameObject.scene.name)) {

                    // Get the available animation parameters
                    animParams = animator.parameters;

                    for (int i = 0; i < buttonStates.Length; i++) {
                        acb.AnimActions[i].ButtonState = buttonStates[i];
                    }

                    // Now make sure all animation parameters are found
                    for (int i = 0; i < acb.AnimActions.Length; i++) {
                        if (!string.IsNullOrEmpty(acb.AnimActions[i].ParamName)) {
                            bool invalidParam = true;
                            foreach (AnimatorControllerParameter animParam in animParams) {
                                if (acb.AnimActions[i].ParamName == animParam.name) {
                                    // Update the type while we're here
                                    invalidParam = false;
                                    acb.AnimActions[i].ParamType = animParam.type;
                                    break;
                                }
                            }

                            // If we didn't find it, mark it as invalid
                            acb.AnimActions[i].InvalidParam = invalidParam;
                        }
                    }
                }

                UnityEditor.EditorGUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
                UnityEditor.EditorGUILayout.LabelField("Animation states:", UnityEditor.EditorStyles.miniBoldLabel);

                // Draw the editor for all the animation actions
                for (int i = 0; i < acb.AnimActions.Length; i++) {
                    acb.AnimActions[i] = DrawAnimActionEditor(acb.AnimActions[i], animParams);
                }

                UnityEditor.EditorGUILayout.EndVertical();
            }

            AnimatorControllerAction DrawAnimActionEditor(AnimatorControllerAction action, AnimatorControllerParameter[] animParams) {
                bool actionIsEmpty = string.IsNullOrEmpty(action.ParamName);
                UnityEditor.EditorGUILayout.BeginHorizontal();
                UnityEditor.EditorGUILayout.LabelField(action.ButtonState.ToString(), GUILayout.MaxWidth(150f), GUILayout.MinWidth(150f));

                if (animParams != null && animParams.Length > 0) {
                    // Show a dropdown
                    string[] options = new string[animParams.Length + 1];
                    options[0] = "(None)";
                    int currentIndex = 0;
                    for (int i = 0; i < animParams.Length; i++) {
                        options[i + 1] = animParams[i].name;
                        if (animParams[i].name == action.ParamName) {
                            currentIndex = i + 1;
                        }
                    }
                    GUI.color = actionIsEmpty ? Color.gray : Color.white;
                    int newIndex = UnityEditor.EditorGUILayout.Popup(currentIndex, options, GUILayout.MinWidth(150f), GUILayout.MaxWidth(150f));
                    if (newIndex == 0) {
                        action.ParamName = string.Empty;
                    } else {
                        action.ParamName = animParams[newIndex - 1].name;
                        action.ParamType = animParams[newIndex - 1].type;
                    }
                } else {
                    // Just show a label
                    GUI.color = action.InvalidParam ? Color.yellow : Color.white;
                    UnityEditor.EditorGUILayout.LabelField(actionIsEmpty ? "(None)" : action.ParamName, GUILayout.MinWidth(75f), GUILayout.MaxWidth(75f));
                }

                GUI.color = Color.white;

                if (!actionIsEmpty) {
                    UnityEditor.EditorGUILayout.LabelField(action.ParamType.ToString(), UnityEditor.EditorStyles.miniLabel, GUILayout.MinWidth(75f), GUILayout.MaxWidth(75f));
                    switch (action.ParamType) {
                        case AnimatorControllerParameterType.Bool:
                            action.BoolValue = UnityEditor.EditorGUILayout.Toggle(action.BoolValue);
                            break;

                        case AnimatorControllerParameterType.Float:
                            action.FloatValue = UnityEditor.EditorGUILayout.FloatField(action.FloatValue);
                            break;

                        case AnimatorControllerParameterType.Int:
                            action.IntValue = UnityEditor.EditorGUILayout.IntField(action.IntValue);
                            break;

                        case AnimatorControllerParameterType.Trigger:
                            break;

                        default:
                            break;

                    }
                }

                UnityEditor.EditorGUILayout.EndHorizontal();

                return action;
            }
        }
#endif
    }
}