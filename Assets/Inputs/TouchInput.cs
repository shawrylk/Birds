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
        event Func<Vector2, Task<bool>> SwipeHandler;
        event Func<Vector2, Task<bool>> TapHandler;
    }
    [DefaultExecutionOrder(Global.INPUT_SCRIPT)]
    public class TouchInput : MonoBehaviour, IInput
    {
        private Vector2 fingerDownPosition;
        private Vector2 fingerUpPosition;
        private Func<bool> _isTapOnUI = null;

        [SerializeField] private bool detectSwipeOnlyAfterRelease = false;

        [SerializeField] private float minDistanceForSwipe = 0.5f;

        public event Func<Vector2, Task<bool>> SwipeHandler
        {
            add => _swipeHandler += value;
            remove => _swipeHandler -= value;
        }
        private event Func<Vector2, Task<bool>> _swipeHandler;
        public event Func<Vector2, Task<bool>> TapHandler
        {
            add => _tapHandler += value;
            remove => _tapHandler -= value;
        }
        private event Func<Vector2, Task<bool>> _tapHandler;

        private void Awake()
        {
            Global.Items.TryAdd(Global.INPUT, this);
            var results = new List<RaycastResult>();
            var e = new PointerEventData(EventSystem.current);
            _isTapOnUI = () =>
            {
                e.position = fingerDownPosition;
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
                    fingerUpPosition = touch.position;
                    fingerDownPosition = touch.position;
                }

                if (!detectSwipeOnlyAfterRelease && touch.phase == TouchPhase.Moved)
                {
                    fingerDownPosition = touch.position;
                    DetectSwipe();
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    fingerDownPosition = touch.position;
                    var isTap = !DetectSwipe();
                    if (isTap && !(_isTapOnUI?.Invoke() ?? false))
                    {
                        await InvokeHandler(_tapHandler, fingerDownPosition.ToWorldCoord());

                    }
                }
            }
        }

        private bool DetectSwipe()
        {
            if (SwipeDistanceCheckMet())
            {
                /* 4 axis */
                //if (IsVerticalSwipe())
                //{
                //    var direction = fingerDownPosition.y - fingerUpPosition.y > 0 ? Vector2.up : Vector2.down;
                //    SendSwipe(direction);
                //}
                //else
                //{
                //    var direction = fingerDownPosition.x - fingerUpPosition.x > 0 ? Vector2.right : Vector2.left;
                //    SendSwipe(direction);
                //}

                /* Free axis */
                var direction = fingerDownPosition.ToWorldCoord() - fingerUpPosition.ToWorldCoord();
                SendSwipe(direction);

                fingerUpPosition = fingerDownPosition;
                return true;
            }
            return false;
        }

        private bool IsVerticalSwipe()
        {
            return VerticalMovementDistance() > HorizontalMovementDistance();
        }

        private bool SwipeDistanceCheckMet()
        {
            return VerticalMovementDistance() > minDistanceForSwipe || HorizontalMovementDistance() > minDistanceForSwipe;
        }

        private float VerticalMovementDistance()
        {
            return Mathf.Abs(fingerDownPosition.y - fingerUpPosition.y);
        }

        private float HorizontalMovementDistance()
        {
            return Mathf.Abs(fingerDownPosition.x - fingerUpPosition.x);
        }

        private async void SendSwipe(Vector2 direction)
        {
            //SwipeData swipeData = new SwipeData()
            //{
            //    Direction = direction,
            //    StartPosition = fingerDownPosition,
            //    EndPosition = fingerUpPosition
            //};
            await InvokeHandler(_swipeHandler, direction);
        }

        private async Task InvokeHandler(Func<Vector2, Task<bool>> funcs, Vector2 direction)
        {
            var list = funcs.GetInvocationList();
            foreach (Func<Vector2, Task<bool>> func in list)
            {
                var ret = await (func?.Invoke(direction) ?? Task.FromResult(false));
                if (ret) break;
            }
        }

        public Vector2 UserInput()
        {
            throw new NotImplementedException();
        }
    }
    //public struct SwipeData
    //{
    //    public Vector2 StartPosition;
    //    public Vector2 EndPosition;
    //    public Vector2 Direction;
    //}
}
