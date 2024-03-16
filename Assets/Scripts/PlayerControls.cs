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
        }

        void Update()
        {
            Up = _UpAction.WasPressedThisFrame();
            Down = _DownAction.WasPressedThisFrame();
            Left = _LeftAction.WasPressedThisFrame();
            Right = _RightAction.WasPressedThisFrame();

            Escape = _EscapeAction.WasPerformedThisFrame();
        }

        public void ResetInputs()
        {
            Up = false;
            Down = false;
            Left = false;
            Right = false;

            Escape = false;
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
    }
}
