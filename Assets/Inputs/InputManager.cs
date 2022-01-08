using Assets.Scripts.Utilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

[DefaultExecutionOrder(-1)]
public class InputManager : LifeTimeBase.SingletonScript<InputManager>
{
    private TouchControls _touchControls;

    private event Action<Vector2, double> onStartTouch;
    public event Action<Vector2, double> OnStartTouch
    {
        add => onStartTouch += value;
        remove => onStartTouch -= value;
    }
    private event Action<Vector2, double> onEndTouch;
    public event Action<Vector2, double> OnEndTouch
    {
        add => onEndTouch += value;
        remove => onEndTouch -= value;
    }
    private void Awake()
    {
        _touchControls = new TouchControls();
    }
    private void Start()
    {
        var (startTouchHandler, endTouchHandler) = GetTouchHandlers();
        _touchControls.Touch.TouchPress.started += startTouchHandler;
        _touchControls.Touch.TouchPress.canceled += endTouchHandler;
    }

    private void OnEnable()
    {
        _touchControls.Enable();
        //TouchSimulation.Enable();
    }

    private void OnDisable()
    {
        _touchControls.Disable();
        //TouchSimulation.Disable();
    }

    private (Action<InputAction.CallbackContext> startTouchHandler, Action<InputAction.CallbackContext> endTouchHandler) GetTouchHandlers()
    {
        bool firstTouch = true;

        IEnumerator touchCoroutine(InputAction.CallbackContext context)
        {
            yield return new WaitForEndOfFrame();
            onStartTouch?.Invoke(_touchControls.Touch.TouchPosition.ReadValue<Vector2>(), context.startTime);
        }

        Action<InputAction.CallbackContext> startTouchHandler = context =>
        {
            if (firstTouch)
            {
                firstTouch = false;
                StartCoroutine(touchCoroutine(context));
            }
            else
            {
                onStartTouch?.Invoke(_touchControls.Touch.TouchPosition.ReadValue<Vector2>(), context.startTime);
            }
        };

        Action<InputAction.CallbackContext> endTouchHandler = context =>
        {
            onEndTouch?.Invoke(_touchControls.Touch.TouchPosition.ReadValue<Vector2>(), context.time);
        };

        return (startTouchHandler, endTouchHandler);
    }
}
