using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Assets.Contracts;
using Assets.Inputs;
using Assets.Scripts.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{

    [DefaultExecutionOrder(Global.FOOD_MANAGER_ORDER)]
    public class FoodManager : MonoBehaviour
    {
        private IInput _input;
        private int _price = 0;
        private const int MAX_FOOD_COUNT = 10;
        private int _foods_last_index = 0;
        public GameObject[] FoodPrefabs;
        private CashManager _cashManager;

        public static FoodManager Instance { get; private set; }

        public Button UpgradeFoodQualityButton;
        public Button UpgradeFoodCountButton;

        public TextMeshProUGUI UpgradeFoodQualityPriceTMP;
        public TextMeshProUGUI UpgradeFoodCountTMP;

        public int FoodIndex = 0;
        public int FoodCount = 1;
        private void Awake()
        {
            //_input = InputManager.Instance;
            //_input.OnStartTouch += SpawnFood;
            _price = GetCurrentFood(FoodIndex).gameObject.GetComponent<Food>().Price;
            _foods_last_index = FoodPrefabs.Length - 1;
            _cashManager = CashManager.Instance;
            Instance = this;
        }

        private GameObject GetCurrentFood(int index)
        {
            if (index <= _foods_last_index)
            {
                return FoodPrefabs[index];
            }
            return FoodPrefabs[_foods_last_index];
        }

        public void UpgradeFoodQuality()
        {
            var newFood = GetCurrentFood(FoodIndex + 1).GetComponent<Food>();
            var upgradePrice = newFood.UpgradePrice;
            var (isDone, currentCash) = _cashManager.Minus(upgradePrice);
            if (isDone)
            {
                FoodIndex++;
                newFood = GetCurrentFood(FoodIndex + 1).GetComponent<Food>();
                upgradePrice = newFood.UpgradePrice;
                UpgradeFoodQualityPriceTMP.text = $"Food\n{upgradePrice}";

                if (FoodIndex == _foods_last_index)
                {
                    UpgradeFoodQualityButton.enabled = false;
                    UpgradeFoodQualityButton.interactable = false;
                }
            }
        }
        public void UpgradeFoodCount()
        {
            const int FOOD_PRICE = 300;
            if (FoodCount < MAX_FOOD_COUNT)
            {
                var (isDone, currentCash) = _cashManager.Minus(FOOD_PRICE);
                if (isDone)
                {
                    FoodCount++;
                    UpgradeFoodCountTMP.text = $"Count: {FoodCount}\n300";


                    if (FoodCount == MAX_FOOD_COUNT)
                    {
                        UpgradeFoodCountButton.enabled = false;
                        UpgradeFoodCountButton.interactable = false;
                    }
                }
            }
        }
        //private void OnDisable()
        //{
        //    _input.OnStartTouch -= SpawnFood;
        //}
        public void SpawnFood(Vector3 position)
        {
            if (FoodPrefabs != null)
            {
                if (transform.childCount < FoodCount)
                {
                    var (isDone, currentCash) = _cashManager.Minus(_price);
                    if (isDone)
                    {
                        //var position = input.ScreenPosition.ToWorldCoord();
                        var food =
                            Instantiate(original: GetCurrentFood(FoodIndex),
                            position: position,
                            rotation: Quaternion.identity,
                            parent: transform);

                        food.name = Food.Name;

                    }
                }
            }
        }
    }
}
