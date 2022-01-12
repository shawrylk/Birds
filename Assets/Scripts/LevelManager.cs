using Assets.Contracts;
using Assets.Scripts.Utilities;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts
{
    [DefaultExecutionOrder(Global.LEVEL_MANAGER_ORDER)]
    public class LevelManager : BaseScript
    {
        public GameObject CashManager;
        public GameObject FoodManager;
        public GameObject BirdManager;
        public GameObject Boundaries;
        public GameObject UI;

        private IInputManager _input;
        private void Awake()
        {
            _input = InputManager.Instance;
            _input.OnStartTouch += AvoidClickThroughUI;
        }

        private void AvoidClickThroughUI(InputContext input)
        {
        }

        private void OnDisable()
        {
            _input.OnStartTouch -= AvoidClickThroughUI;
        }
    }
}