using System;
using UnityEditor;
using UnityEngine;

namespace Assets.Contracts
{
    public interface IInputManager
    {
        public event Action<InputContext> OnStartTouch;
        public event Action<InputContext> OnEndTouch;
    }
}