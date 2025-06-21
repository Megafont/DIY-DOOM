using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;


namespace DIY_DOOM
{
    public class PlayerControls : MonoBehaviour
    {
        [Header("Mouse Cursor Settings")]
        public bool CursorLocked = true;


        private PlayerInput _PlayerInput;
        private InputActionMap _InputActionMap;


        // Input Actions
        // ----------------------------------------------------------------------------------------------------
        private InputAction _UpAction;
        private InputAction _DownAction;
        private InputAction _LeftAction;
        private InputAction _RightAction;
        private InputAction _EscapeAction;
        private InputAction _ToggleCameraAction;
        private InputAction _CameraLookAction;
        // ----------------------------------------------------------------------------------------------------



        void Awake()
        {
            _PlayerInput = FindObjectOfType<PlayerInput>();
            _InputActionMap = _PlayerInput.actions.FindActionMap("Main", true);


            _UpAction = _InputActionMap["Up"];
            _DownAction = _InputActionMap["Down"];
            _LeftAction = _InputActionMap["Left"];
            _RightAction = _InputActionMap["Right"];

            _EscapeAction = _InputActionMap["Escape"];

            _ToggleCameraAction = _InputActionMap["ToggleCamera"];
            _CameraLookAction = _InputActionMap["CameraLook"];
        }

        void Update()
        {
            Up = _UpAction.IsPressed();
            Down = _DownAction.IsPressed();
            Left = _LeftAction.IsPressed();
            Right = _RightAction.IsPressed();

            Escape = _EscapeAction.WasPerformedThisFrame();
            
            ToggleCamera = _ToggleCameraAction.WasPressedThisFrame();
            CameraLook = _CameraLookAction.ReadValue<Vector2>();
        }

        public void ResetInputs()
        {
            Up = false;
            Down = false;
            Left = false;
            Right = false;

            Escape = false;

            ToggleCamera = false;
            CameraLook = Vector2.zero;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(CursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }



        public bool Up { get; private set; }
        public bool Down { get; private set; }
        public bool Left { get; private set; }
        public bool Right { get; private set; }
        
        public bool Escape { get; private set; }
        
        public bool ToggleCamera { get; private set; }
        
        public Vector2 CameraLook { get; private set; }
    }
}
