using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Assets.Contracts;
using Assets.Scripts.Utilities;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{

    [DefaultExecutionOrder(Global.FOOD_MANAGER_ORDER)]
    public class FoodManager : MonoBehaviour
    {
        private IInputManager _input;
        private int _price = 0;
        private const int MAX_FOOD_COUNT = 10;
        public GameObject FoodPrefab;
        public TextMeshProUGUI ScoreTMP;

        private void Awake()
        {
            _input = InputManager.Instance;
            _input.OnStartTouch += Touch;
            _price = FoodPrefab.gameObject.GetComponent<Food>().Price;
        }

        private void OnDisable()
        {
            _input.OnStartTouch -= Touch;
        }
        private void Touch(InputContext input)
        {
            if (FoodPrefab != null)
            {
                if (int.TryParse(ScoreTMP.text, out int currentCash)
                    && currentCash >= _price
                    && transform.childCount < MAX_FOOD_COUNT)
                {
                    var position = input.Position.ToWorldCoord();
                    var food =
                        Instantiate(original: FoodPrefab,
                        position: position,
                        rotation: Quaternion.identity,
                        parent: transform);
                    ScoreTMP.text = (currentCash - _price).ToString();
                }
            }
        }
    }
}
