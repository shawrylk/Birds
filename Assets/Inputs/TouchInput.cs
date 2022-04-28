using Assets.Scripts;
using Assets.Scripts.Utilities;
using System;
using System.Threading.Tasks;
using UnityEngine;
namespace Assets.Inputs
{
    public interface IInput
    {
        event Func<Vector2, Task> SwipeHandler;
        event Func<Vector2, Task> TapHandler;
    }
    [DefaultExecutionOrder(Global.INPUT_SCRIPT)]
    public class TouchInput : MonoBehaviour, IInput
    {
        private Vector2 fingerDownPosition;
        private Vector2 fingerUpPosition;

        [SerializeField] private bool detectSwipeOnlyAfterRelease = false;

        [SerializeField] private float minDistanceForSwipe = 0.5f;

        public event Func<Vector2, Task> SwipeHandler
        {
            add => _swipeHandler += value;
            remove => _swipeHandler -= value;
        }
        private event Func<Vector2, Task> _swipeHandler;
        public event Func<Vector2, Task> TapHandler
        {
            add => _tapHandler += value;
            remove => _tapHandler -= value;
        }
        private event Func<Vector2, Task> _tapHandler;

        private void Awake()
        {
            Global.Items.TryAdd(Global.INPUT, this);
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
                    if (isTap)
                    {
                        await (_tapHandler?.Invoke(fingerDownPosition.ToWorldCoord()) ?? Task.CompletedTask);
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
                var direction = fingerDownPosition - fingerUpPosition;
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
            await (_swipeHandler?.Invoke(direction) ?? Task.CompletedTask);
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
