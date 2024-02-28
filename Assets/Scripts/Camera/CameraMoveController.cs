using System;
using Player;
using Player.ActionHandlers;
using UnityEngine;
using Utils.Singleton;

namespace Camera
{
    public class CameraMoveController : DontDestroyMonoBehaviourSingleton<CameraMoveController>
    {
        [SerializeField, Range(0f, 1f)] private float _moveSmoothness = 0.1f;
        // можно вынести для каждой сцены
        [SerializeField] private Vector2 _moveBorderCenter = new(0, 0);
        [SerializeField] private Vector2 _moveBorderSize = new(30f, 20f);
        
        private ClickHandler _clickHandler;
        private bool _isDrag;
        private Vector3 _startDragPosition;
        private float _minXCameraPosition;
        private float _maxXCameraPosition;
        private float _minYCameraPosition;
        private float _maxYCameraPosition;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            
            Gizmos.DrawWireCube(_moveBorderCenter, _moveBorderSize);
        }

        private void Awake()
        {
            _clickHandler = ClickHandler.Instance;
            _clickHandler.AddDragEventHandlers(OnDragStart, OnDragEnd);
            _isDrag = false;
        }

        private void Start()
        {
            var orthographicSize = CameraHolder.Instance.MainCamera.orthographicSize;
            var aspect = CameraHolder.Instance.MainCamera.aspect;
            _minXCameraPosition = _moveBorderCenter.x - _moveBorderSize.x / 2 + orthographicSize * aspect;
            _maxXCameraPosition = _moveBorderCenter.x + _moveBorderSize.x / 2 - orthographicSize * aspect;
            _minYCameraPosition = _moveBorderCenter.y - _moveBorderSize.y / 2 + orthographicSize;
            _maxYCameraPosition = _moveBorderCenter.y + _moveBorderSize.y / 2 - orthographicSize;
        }

        private void LateUpdate()
        {
            if (!_isDrag)
                return;
            
            var mousePosition = CameraHolder.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            var myTransform = transform;
            var deltaDrag = mousePosition - myTransform.position;
            var targetPosition = _startDragPosition - deltaDrag;
            
            targetPosition.x = Mathf.Clamp(targetPosition.x, _minXCameraPosition, _maxXCameraPosition);
            targetPosition.y = Mathf.Clamp(targetPosition.y, _minYCameraPosition, _maxYCameraPosition);
            
            myTransform.position = Vector3.Lerp(myTransform.position, targetPosition, _moveSmoothness);
        }

        private void OnDragEnd(Vector3 obj)
        {
            if (PlayerController.PlayerState == PlayerState.Connecting)
                return;

            _isDrag = false;
        }

        private void OnDragStart(Vector3 obj)
        {
            if (PlayerController.PlayerState == PlayerState.Connecting)
                return;

            _startDragPosition = CameraHolder.Instance.MainCamera.ScreenToWorldPoint(Input.mousePosition);
            _isDrag = true;
        }

        private void OnDestroy()
        {
            _clickHandler.RemoveDragEventHandlers(OnDragStart, OnDragEnd);
        }
    }
}