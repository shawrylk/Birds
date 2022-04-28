//using Assets.Contracts;
//using Assets.Scripts.Utilities;
//using System;
//using System.Collections;
//using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.InputSystem;
//using UnityEngine.InputSystem.EnhancedTouch;

//public class InputContext
//{
//    //public InputAction.CallbackContext Context { get; set; }
//    //public TouchControls.TouchActions TouchActions { get; set; }
//    public Vector2 ScreenPosition { get; set; }
//    public bool Handled { get; set; } = false;
//}
//[DefaultExecutionOrder(-1)]
//public class InputManager : LifeTimeBase.SingletonScript<InputManager>, IInputManager
//{
//    private Action<Finger> startTouchHandler;
//    private Action<Finger> endTouchHandler;
//    private TouchControls _touchControls;
//    private object _eventLock = new object();

//    private event Action<InputContext> onStartTouch;
//    public event Action<InputContext> OnStartTouch
//    {
//        add { lock (_eventLock) onStartTouch += value; }
//        remove { lock (_eventLock) onStartTouch -= value; }
//    }
//    private event Action<InputContext> onEndTouch;
//    public event Action<InputContext> OnEndTouch
//    {
//        add { lock (_eventLock) onEndTouch += value; }
//        remove { lock (_eventLock) onEndTouch -= value; }
//    }
//    private void Awake()
//    {
//        _touchControls = new TouchControls();
//    }
//    private void Start()
//    {
//        (startTouchHandler, endTouchHandler) = GetTouchHandlers();
//        //_touchControls.Touch.TouchPress.started += startTouchHandler;
//        //_touchControls.Touch.TouchPress.canceled += endTouchHandler;

//    }
//    private bool _isPointerOverUI = false;

//    private void Update()
//    {
//        foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
//        {
//            _isPointerOverUI = EventSystem.current.IsPointerOverGameObject();

//            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
//            {
//                startTouchHandler?.Invoke(touch.finger);
//            }
//            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
//            {
//                endTouchHandler?.Invoke(touch.finger);
//            }
//        }
//    }

//    private void OnEnable()
//    {
//        EnhancedTouchSupport.Enable();
//        _touchControls.Enable();
//        //UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += startTouchHandler;
//        //UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += endTouchHandler;
//    }

//    private void OnDisable()
//    {
//        EnhancedTouchSupport.Disable();
//        _touchControls.Disable();
//        //UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= startTouchHandler;
//        //UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= endTouchHandler;
//    }

//    private (Action<Finger> startTouchHandler, Action<Finger> endTouchHandler) GetTouchHandlers()
//    {
//        bool firstTouch = true;

//        void invokeHandler(Finger finger, Action<InputContext> action)
//        {
//            var param = new InputContext
//            {
//                //Context = context,
//                //TouchActions = _touchControls.Touch,
//                ScreenPosition = finger.screenPosition,
//                Handled = false
//            };

//            lock (_eventLock)
//            {
//                var list = action?.GetInvocationList();
//                if (list is null
//                    || _isPointerOverUI) return;
//                foreach (Action<InputContext> handler in list)
//                {
//                    handler.Invoke(param);
//                    if (param.Handled) break;
//                }
//            }
//        }

//        IEnumerator touchCoroutine(Finger context)
//        {
//            yield return new WaitForEndOfFrame();
//            invokeHandler(context, onStartTouch);
//        }

//        Action<Finger> startTouchHandler = context =>
//        {
//            if (firstTouch)
//            {
//                firstTouch = false;
//                StartCoroutine(touchCoroutine(context));
//            }
//            else
//            {
//                invokeHandler(context, onStartTouch);
//            }
//        };

//        Action<Finger> endTouchHandler = context =>
//        {
//            invokeHandler(context, onEndTouch);
//        };

//        return (startTouchHandler, endTouchHandler);
//    }
//}
