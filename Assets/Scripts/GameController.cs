using System.Collections;
using System.Collections.Generic;
using DIY_DOOM.MeshGeneration;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace DIY_DOOM
{
    public class GameController : MonoBehaviour
    {
        [Header("References")]
        
        [SerializeField] 
        private GameObject _PlayerObject;
        [SerializeField]
        private Camera _MainCamera;
        [SerializeField]
        private Camera _AutoMapCamera;
        [SerializeField] 
        private Camera _AutoMapLinesCamera;
        [SerializeField]
        private PlayerControls _PlayerControls;
        [SerializeField] 
        private LevelGeometry _LevelGeometry;


        [Header("Player Settings")]
        
        [SerializeField]
        [Tooltip("The movement speed in units per second.")]
        [Range(0f, 20f)]
        private float _MoveSpeed = 5f;
        
        [SerializeField]
        [Tooltip("The rotation speed in degrees per second.")]
        [Range(0f, 360f)]
        private float _TurnSpeed = 90f;
        
        
        [Header("Main Camera Settings")] 
        
        [Tooltip("Sets the height of the main camera off the ground.")]
        [SerializeField]
        [Range(0f, 10f)]
        private float _MainCameraHeight = 1.5f;
        
        
        [FormerlySerializedAs("_AutoMapCameraSize")]
        [Header("Auto Map Camera Settings")]
        
        [Tooltip("This sets the size of the orthographic auto map camera, or in other words, the larger this value is the more zoomed out the auto map is.")]
        [SerializeField]
        [Range(1f, 100f)]
        private float _AutoMapOrthographicCameraSize = 50f;

        [Tooltip("Sets the height of the auto map camera off the ground.")]
        [SerializeField]
        [Range(0f, 100f)]
        private float _AutoMapCameraHeight = 30f;        

        private bool _IsInGameMode = true;
        
        
        
        void Awake()
        {
            Vector3 startPos = _PlayerObject.transform.position;
            UpdatePlayerPosition(startPos);

            _AutoMapCamera.orthographicSize = _AutoMapOrthographicCameraSize;
            _AutoMapLinesCamera.orthographicSize = _AutoMapOrthographicCameraSize;
        }
        
        // Start is called before the first frame update
        void Start()
        {
            _MainCamera.gameObject.SetActive(_IsInGameMode);
            _AutoMapCamera.gameObject.SetActive(!_IsInGameMode);
            
            //StartCoroutine(InitPlayerPosition());
            //_PlayerObject.transform.position = _LevelGeometry.Map.GetPlayerSpawn(0).Position / _LevelGeometry.Map.ScaleFactor;
        }

        private IEnumerator InitPlayerPosition()
        {
            yield return new WaitForSeconds(3f);
            UpdatePlayerPosition(_PlayerObject.transform.position = _LevelGeometry.Map.GetPlayerSpawn(0).Position / _LevelGeometry.Map.ScaleFactor);            
        }

        // Update is called once per frame
        void Update()
        {
            CheckInputs();
        }

        private void CheckInputs()
        {
            CheckMovementControls();
            CheckCameraLookControls();
            
            
            
            if (_PlayerControls.ToggleCamera)
            {
                _IsInGameMode = !_IsInGameMode;
                
                _MainCamera.gameObject.SetActive(_IsInGameMode);
                _AutoMapCamera.gameObject.SetActive(!_IsInGameMode);
            }
        }

        private void CheckMovementControls()
        {
            Vector3 curPlayerPos = _PlayerObject.transform.position;
            Vector3 movement = Vector3.zero;
            
            if (_PlayerControls.Up)
            {
                movement += _MainCamera.transform.forward * _MoveSpeed * Time.deltaTime;
            }

            if (_PlayerControls.Down)
            {
                movement += -_MainCamera.transform.forward * _MoveSpeed * Time.deltaTime;
            }

            if (_PlayerControls.Left)
            {
                movement += -_MainCamera.transform.right * _MoveSpeed * Time.deltaTime;
            }
            
            if (_PlayerControls.Right)
            {
                movement += _MainCamera.transform.right * _MoveSpeed * Time.deltaTime;
            }

            
            movement.y = 0f;

            
            if (movement != Vector3.zero)
            {
                UpdatePlayerPosition(curPlayerPos + movement);
            }
        }

        private void CheckCameraLookControls()
        {
            Vector3 rotation = _MainCamera.transform.eulerAngles;
            
            if (_PlayerControls.CameraLook.x < 0)
            {
                rotation.y += -_TurnSpeed * Time.deltaTime;
            }
            
            if (_PlayerControls.CameraLook.x > 0)
            {
                rotation.y += _TurnSpeed * Time.deltaTime;
            }

            if (_PlayerControls.CameraLook.y > 0)
            {
                rotation.x += -_TurnSpeed * Time.deltaTime;
            }

            if (_PlayerControls.CameraLook.y < 0)
            {
                rotation.x += _TurnSpeed * Time.deltaTime;
            }
            
            
            _MainCamera.transform.rotation = Quaternion.Euler(rotation);
        }

        public void UpdatePlayerPosition(Vector3 newPosition)
        {
            _PlayerObject.transform.position = newPosition;
            
            newPosition.y = _MainCameraHeight;
            _MainCamera.transform.position = newPosition;
            
            newPosition.y = _AutoMapCameraHeight;
            _AutoMapCamera.transform.position = newPosition;
        }
    }

}
