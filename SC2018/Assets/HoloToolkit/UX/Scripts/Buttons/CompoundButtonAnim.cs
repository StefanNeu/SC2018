// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System;
using HoloToolkit.Unity;
using System.Collections;
using UnityEngine.SceneManagement;

namespace HoloToolkit.Unity.Buttons
{
    /// <summary>
    /// Anim controller button offers as simple way to link button states to animation controller parameters
    /// </summary>
    [RequireComponent(typeof(CompoundButton))]
    public class CompoundButtonAnim : MonoBehaviour
    {
        public GameObject ButtonFamily;
        public GameObject ObjToManip;                   //object that gets moved
        public GameObject MoveHandle;                   //used to deactivate MoveUI and therefore activate RotateUI (otherwise both UIs overlap)
        public GameObject RotateHandle;
        public TextMesh TutorialText;                   //the tutorial-text above the cube
        public TextMesh TransformMode_Text;             //the text on the "fineness"-button, which we want to change according to the mode
       
        

        public static int tutorialStep = 0;                    //0 = first text,      1 = alligning the corner of the cube,      2 = rotate the cube
        public int t_step = tutorialStep;
        static float move_factor = 0.01f;                     //0.001m = 1mm
        static float rotate_factor = 5.0f;                    //1 degree
        static float ui_scale_factor = 1.1f;                 //10%

        public string button;                           //used to differentiate which button is pressed
                   
        static bool processActive = false;              //makes sure, we cant press button too often

        IEnumerator Waiting()                           //coroutine to stop the user from clicking the button too often and movingthe button too fast 
        {                                               
            
            yield return new WaitForSeconds(0.8f);      //for faster movement, decrease time
            processActive = false;
            
        }

        IEnumerator FirstTutorialText()
        {
            TutorialText.text = "How To Align" + Environment.NewLine + "The Coordinate System";
            yield return new WaitForSeconds(6.0f);
            TutorialText.text = "This tutorial shows, how to set" + Environment.NewLine + "up the HoloLens Coordinate System.";
            yield return new WaitForSeconds(6.0f);
            TutorialText.text = "Please always press the tutorial" + Environment.NewLine + "buttton when you want to go" + Environment.NewLine + "over to the next step!";
        }               //following three coroutintes manage the tutorial-text above the cube

        IEnumerator SecondTutorialText()
        {
            TutorialText.text = "Please move the cube, so" + Environment.NewLine + "that the marked corner, alligns" + Environment.NewLine + "with the marked point in reality!";
            yield return new WaitForSeconds(6.0f);
            TutorialText.text = "";
        }

        IEnumerator ThirdTutorialText()
        {
            
            TutorialText.text = "Now use (any) of the rotation buttons" + Environment.NewLine + "to rotate the cube around the y-axis" + Environment.NewLine + "until it aligns with the other points!";
            yield return new WaitForSeconds(6.0f);
            TutorialText.text = "When you are done, press the" + Environment.NewLine + "tutorial-button to end the process.";
            yield return new WaitForSeconds(5.0f);
            TutorialText.text = "";
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

            GameObject RotaHandleButtons = RotateHandle.transform.GetChild(0).gameObject;
            RotaHandleButtons.SetActive(false);                                     //get buttons of rotatehandle and deactivate them at start of application

            if (ObjToManip.transform.childCount >= 2)
                ObjToManip.transform.GetChild(1).gameObject.SetActive(false);           //deactivate mesh of RotateCube when app starts

            if (name == "Pos_UI_Scale" || name == "Neg_UI_Scale" || name == "TransformMode (rough or fine)")                   //ugly hotfix since backplates of UI_Scale buttons disappeared at app_start
                if (transform.GetChild(0).gameObject.activeSelf == false)
                    transform.GetChild(0).gameObject.SetActive(true);
        }


        //----------------- METHODS USED FOR MOVING/ROTATING/SCALING-----------------------------
        void Scale_UI(float scale_factor, ButtonStateEnum newState)
        {

            if ((processActive == false) && (newState == ButtonStateEnum.Pressed))
            {
                processActive = true;

                ButtonFamily.transform.localScale *= scale_factor;
                StartCoroutine(Waiting());

            }
        }                           //method to scale the UI

        void MoveObj(float x, float y, float z, ButtonStateEnum newState)
        {
            if ((processActive == false) && (newState == ButtonStateEnum.Pressed))
            {
                processActive = true;
                ObjToManip.transform.Translate(x,y,z);
                StartCoroutine(Waiting());

            }
        }                     //method to move object

        void RotateObj(float x, float y, float z, ButtonStateEnum newState)
        {
            if ((processActive == false) && (newState == ButtonStateEnum.Pressed))
            {
                processActive = true;

                if ((tutorialStep == 2) || (tutorialStep == 3))                                 //when in tutorialStep 2 or 3, rotate around "RotatingPoint"
                {
                    GameObject rotatePoint = GameObject.Find("RotatingPoint");                                          //rotate around the y-axis of this empty

                    ObjToManip.transform.RotateAround(rotatePoint.transform.position, Vector3.up, (x+y+z));      //just adding up x,y and z, since two of them are always 
                                                                                                                 //zero and then we get the right sign (+ or -)
                } else
                {
                    ObjToManip.transform.Rotate(x, y, z);
                }

                StartCoroutine(Waiting());

            }
        }                   //method to rotate object in euler-angles

        void Change_UI(ButtonStateEnum newState, int mode)
        {
            GameObject MovHandleButtons = MoveHandle.transform.GetChild(0).gameObject;          //get the childs (the buttons) of the handles, too deactivate them
            GameObject RotaHandleButtons = RotateHandle.transform.GetChild(0).gameObject;

            GameObject RotaCubeMesh = ObjToManip.transform.GetChild(1).gameObject;
            GameObject AxisCubeMesh = ObjToManip.transform.GetChild(0).gameObject;


            if (newState == ButtonStateEnum.Pressed)
            {
                switch (mode)
                {
                    case 1:                                                    //change UI to rotate-mode 

                        if (RotaHandleButtons.activeSelf == false)
                        {
                            RotaHandleButtons.SetActive(true);                  //two times because unity doesn't show it at the first (absolutely no clue why)
                            RotaHandleButtons.SetActive(true);
                        }

                        if (MovHandleButtons.activeSelf == true)
                            MovHandleButtons.SetActive(false);

                        if (RotaCubeMesh.activeSelf == false)                //see above case "move"
                            RotaCubeMesh.SetActive(true);

                        if (AxisCubeMesh.activeSelf == true)
                            AxisCubeMesh.SetActive(false);
                        break;
                            
                    case 2:                                                 //change ui to move-mode
                        if (RotaHandleButtons.activeSelf == true)
                            RotaHandleButtons.SetActive(false);

                        if (MovHandleButtons.activeSelf == false)
                            MovHandleButtons.SetActive(true);

                        if (RotaCubeMesh.activeSelf == true)                //since we change to the move-ui, the rotacubemesh (in form of child object) should be deactiv.
                            RotaCubeMesh.SetActive(false);

                        if (AxisCubeMesh.activeSelf == false)               //and axiscubemesh gets activated
                            AxisCubeMesh.SetActive(true);
                        break;

                    default:
                        Debug.Log("Invalid mode!");
                        break;
                            
                        
                }
                
            }
                  
        }                                                                                                                                                                             
                                                                                                 //method to change UI into rotate(1) or move(2)-mode

        //---------------------------------------------------------------------------------------
        

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
            
            GameObject PointingArrow = ObjToManip.transform.GetChild(2).gameObject;                 //get the pointing arrow as gameobject
                
            switch (button)
            {

                //---------------------------- CASES FOR UI-CHANGE ------------------------------------

                case "move":                                                //enables move-UI, disables rotate-UI

                    Change_UI(newState, 2);
                    break;

                case "rotate":                                              //enables rotate-UI, disables move-UI

                    Change_UI(newState, 1);
                    break;

                case "pos scale":                                               //scales UI in all positive in all directions

                    Scale_UI(ui_scale_factor, newState);
                    break;

                case "neg scale":

                    Scale_UI(2.0f - ui_scale_factor, newState);
                    break;

                //-------------------- OTHER CASES --------------------------------

                case "how to align":
                    if ((processActive == false) && (newState == ButtonStateEnum.Pressed))
                    {
                        processActive = true;

                        switch (tutorialStep)
                        {
                            case 0:                                             //first text of the tutorial
                                
                                StartCoroutine(FirstTutorialText());
                                tutorialStep++;
                                break;

                            case 1:
                                
                                if (PointingArrow.activeSelf == false)          //user has to allign the cube corner with the point in reality
                                    PointingArrow.SetActive(true);
                                StartCoroutine(SecondTutorialText());
                                tutorialStep++;
                                break;

                            case 2:
                                
                                if (PointingArrow.activeSelf == true)           //user has to turn the cube around the y-axis of the corner to allign the cube 
                                    PointingArrow.SetActive(false);             //with the other points
                                
                                StartCoroutine(ThirdTutorialText());
                                tutorialStep++;
                                break;

                            case 3:                                             //set tutorialStep to 0 to end tutorial
                                tutorialStep = 0;
                                break;
                          

                        }

                        StartCoroutine(Waiting());
                    }
                    break;

                case "transform mode":                                                          //change the factor of movement and rotation
                    if((processActive == false) && (newState == ButtonStateEnum.Pressed))
                    {
                        processActive = true;

                        if (TransformMode_Text.text == "Fine")
                        {
                            TransformMode_Text.text = "Rough";
                            move_factor = 0.01f;
                            rotate_factor = 10.0f;

                        } else if(TransformMode_Text.text == "Rough")
                        {
                            TransformMode_Text.text = "Fine";
                            move_factor = 0.001f;
                            rotate_factor = 1.0f;
                            
                        }

                        StartCoroutine(Waiting());

                    }
                    break;
             
                //------------------------- CASES FOR MOVEMENT --------------------------------------
                case "mov pos x":                                   //moves ObjToManip depending on case (direction)

                    MoveObj(move_factor, 0, 0, newState);

                    break;

                case "mov neg x":

                    MoveObj(-move_factor, 0, 0, newState);
                    break;

                case "mov pos y":

                    MoveObj(0, move_factor, 0, newState);
                    break;

                case "mov neg y":

                    MoveObj(0, -move_factor, 0, newState);
                    break;

                case "mov pos z":

                    MoveObj(0, 0, move_factor, newState);
                    break;

                case "mov neg z":

                    MoveObj(0, 0, -move_factor, newState);
                    break;

                //------------------------------- CASES FOR ROTATION -----------------------------------
                case "rot pos x":

                    RotateObj(rotate_factor, 0, 0, newState);
                    break;

                case "rot neg x":

                    RotateObj(-rotate_factor, 0, 0, newState);
                    break;

                case "rot pos y":

                    RotateObj(0, rotate_factor, 0, newState);
                    break;

                case "rot neg y":

                    RotateObj(0, -rotate_factor, 0, newState);
                    break;

                case "rot pos z":

                    RotateObj(0, 0, rotate_factor, newState);
                    break;

                case "rot neg z":

                    RotateObj(0, 0, -rotate_factor, newState);
                    break;



            }


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