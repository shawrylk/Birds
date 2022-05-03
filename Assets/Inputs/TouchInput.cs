using Assets.Scripts;
using Assets.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Inputs
{
    public interface IInput
    {
        event Func<Vector2, Vector2, Task<bool>> SwipeHandler;
        event Func<Vector2, Task<bool>> TapHandler;
        event Func<Vector2, Task<bool>> DragStartHandler;
        event Func<Vector2, Task<bool>> DragEndHandler;

    }
    [DefaultExecutionOrder(Global.INPUT_SCRIPT)]
    public class TouchInput : MonoBehaviour, IInput
    {
        private Vector2 _fingerDownPosition;
        private Vector2 _fingerUpPosition;
        private Func<bool> _isTapOnUI = null;
        private DateTime _startTime;
        private DateTime _dragStartTime;
        private bool isMoving = false;
        //[SerializeField] private bool _detectSwipeOnlyAfterRelease = false;
        [SerializeField] private float _minDistanceForSwipe = 0.5f;
        [SerializeField] private float _maxTimeForSwipe = 0.7f;
        [SerializeField] private float _initialDelayTimeForDragDetection = 0.05f; 

        public event Func<Vector2, Vector2, Task<bool>> SwipeHandler
        {
            add => _swipeHandler += value;
            remove => _swipeHandler -= value;
        }
        private event Func<Vector2, Vector2, Task<bool>> _swipeHandler;

        public event Func<Vector2, Task<bool>> TapHandler
        {
            add => _tapHandler += value;
            remove => _tapHandler -= value;
        }
        private event Func<Vector2, Task<bool>> _tapHandler;

        public event Func<Vector2, Task<bool>> DragStartHandler
        {
            add => _dragStartHandler += value;
            remove => _dragStartHandler -= value;
        }
        private event Func<Vector2, Task<bool>> _dragStartHandler;

        public event Func<Vector2, Task<bool>> DragEndHandler
        {
            add => _dragEndHandler += value;
            remove => _dragEndHandler -= value;
        }
        private event Func<Vector2, Task<bool>> _dragEndHandler;
        private void Awake()
        {
            Global.Items.TryAdd(Global.INPUT, this);
            var results = new List<RaycastResult>();
            var e = new PointerEventData(EventSystem.current);
            _isTapOnUI = () =>
            {
                e.position = _fingerDownPosition;
                EventSystem.current.RaycastAll(e, results);
                return results.Count > 0;
            };
        }
        private async void Update()
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began)
                {
                    _fingerUpPosition = touch.position;
                    _fingerDownPosition = touch.position;
                    _startTime = DateTime.Now;
                    _dragStartTime = DateTime.Now;
                }

                if (touch.phase == TouchPhase.Moved 
                    && (DateTime.Now - _dragStartTime).TotalSeconds >= _initialDelayTimeForDragDetection)
                {
                    _fingerDownPosition = touch.position;
                    isMoving = true;
                    await InvokeHandler(_dragStartHandler, _fingerDownPosition.ToWorldCoord());
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    _fingerDownPosition = touch.position;
                    if (isMoving)
                    {
                        isMoving = false;
                        await InvokeHandler(_dragEndHandler, _fingerDownPosition.ToWorldCoord());
                    }
                    var isTap = !await DetectSwipe();
                    if (isTap && !(_isTapOnUI?.Invoke() ?? false))
                    {
                        //Debug.Log($"{_fingerDownPosition} - {_fingerDownPosition.ToWorldCoord()}");
                        await InvokeHandler(_tapHandler, _fingerDownPosition.ToWorldCoord());
                    }
                }
            }
        }

        private async Task<bool> DetectSwipe()
        {
            if (SwipeDistanceCheckMet())
            {
                if ((DateTime.Now - _startTime).TotalSeconds <= _maxTimeForSwipe)
                {
                    await InvokeHandler(_swipeHandler, _fingerDownPosition.ToWorldCoord(), _fingerUpPosition.ToWorldCoord());
                    _fingerUpPosition = _fingerDownPosition;
                    return true;
                }
            }
            return false;
        }

        //private bool IsVerticalSwipe()
        //{
        //    return VerticalMovementDistance() > HorizontalMovementDistance();
        //}

        private bool SwipeDistanceCheckMet()
        {
            return VerticalMovementDistance() > _minDistanceForSwipe || HorizontalMovementDistance() > _minDistanceForSwipe;
        }

        private float VerticalMovementDistance()
        {
            return Mathf.Abs(_fingerDownPosition.y - _fingerUpPosition.y);
        }

        private float HorizontalMovementDistance()
        {
            return Mathf.Abs(_fingerDownPosition.x - _fingerUpPosition.x);
        }


        private async Task InvokeHandler(Func<Vector2, Task<bool>> funcs, Vector2 tapPoint)
        {
            var list = funcs?.GetInvocationList();
            if (list is null) return;
            foreach (Func<Vector2, Task<bool>> func in list)
            {
                var ret = await (func?.Invoke(tapPoint) ?? Task.FromResult(false));
                if (ret) break;
            }
        }

        private async Task InvokeHandler(Func<Vector2, Vector2, Task<bool>> funcs, Vector2 startPoint, Vector2 endPoint)
        {
            var list = funcs?.GetInvocationList();
            if (list is null) return;
            foreach (Func<Vector2, Vector2, Task<bool>> func in list)
            {
                var ret = await (func?.Invoke(startPoint, endPoint) ?? Task.FromResult(false));
                if (ret) break;
            }
        }
    }
}
