using Assets.Contracts;
using Assets.Scripts.Utilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class InputContext
{
    public InputAction.CallbackContext Context { get; set; }
    public TouchControls.TouchActions TouchActions { get; set; }
    public Vector2 Position { get => TouchActions.TouchPosition?.ReadValue<Vector2>() ?? new Vector2(); }
    public bool Handled { get; set; } = false;
}
[DefaultExecutionOrder(-1)]
public class InputManager : LifeTimeBase.SingletonScript<InputManager>, IInputManager
{
    private TouchControls _touchControls;
    private object _eventLock = new object();

    private event Action<InputContext> onStartTouch;
    public event Action<InputContext> OnStartTouch
    {
        add { lock (_eventLock) onStartTouch += value; }
        remove { lock (_eventLock) onStartTouch -= value; }
    }
    private event Action<InputContext> onEndTouch;
    public event Action<InputContext> OnEndTouch
    {
        add { lock (_eventLock) onEndTouch += value; }
        remove { lock (_eventLock) onEndTouch -= value; }
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
    }

    private void OnDisable()
    {
        _touchControls.Disable();
    }

    private (Action<InputAction.CallbackContext> startTouchHandler, Action<InputAction.CallbackContext> endTouchHandler) GetTouchHandlers()
    {
        bool firstTouch = true;

        void invokeHandler(InputAction.CallbackContext context, Action<InputContext> action)
        {
            var param = new InputContext
            {
                Context = context,
                TouchActions = _touchControls.Touch,
                Handled = false
            };

            lock (_eventLock)
            {
                var list = action?.GetInvocationList();
                if (list is null) return;
                foreach (Action<InputContext> handler in list)
                {
                    handler.Invoke(param);
                    if (param.Handled) break;
                }
            }
        }

        IEnumerator touchCoroutine(InputAction.CallbackContext context)
        {
            yield return new WaitForEndOfFrame();
            invokeHandler(context, onStartTouch);
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
                invokeHandler(context, onStartTouch);
            }
        };

        Action<InputAction.CallbackContext> endTouchHandler = context =>
        {
            invokeHandler(context, onEndTouch);
        };

        return (startTouchHandler, endTouchHandler);
    }
}
