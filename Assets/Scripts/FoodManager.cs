using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Assets.Contracts;
using Assets.Scripts.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{

    [DefaultExecutionOrder(Global.FOOD_MANAGER_ORDER)]
    public class FoodManager : MonoBehaviour
    {
        private IInputManager _input;
        private int _price = 0;
        private const int MAX_FOOD_COUNT = 10;
        private int _foods_last_index = 0;
        public GameObject[] FoodPrefabs;

        public Button UpgradeFoodQualityButton;
        public Button UpgradeFoodCountButton;

        public TextMeshProUGUI UpgradeFoodQualityPriceTMP;
        public TextMeshProUGUI UpgradeFoodCountTMP;
        public TextMeshProUGUI ScoreTMP;

        public int FoodIndex = 0;
        public int FoodCount = 1;
        private void Awake()
        {
            _input = InputManager.Instance;
            _input.OnStartTouch += SpawnFood;
            _price = GetCurrentFood(FoodIndex).gameObject.GetComponent<Food>().Price;
            _foods_last_index = FoodPrefabs.Length - 1;
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
            if (int.TryParse(ScoreTMP.text, out int score))
            {
                var newFood = GetCurrentFood(FoodIndex + 1).GetComponent<Food>();
                var upgradePrice = newFood.UpgradePrice;
                _price = newFood.Price;
                if (score >= upgradePrice)
                {
                    FoodIndex++;
                    ScoreTMP.text = (score - upgradePrice).ToString();

                    newFood = GetCurrentFood(FoodIndex + 1).GetComponent<Food>();
                    upgradePrice = newFood.UpgradePrice;
                    UpgradeFoodQualityPriceTMP.text = $"Upgrade food\n{upgradePrice}";

                    if (FoodIndex == _foods_last_index)
                    {
                        UpgradeFoodQualityButton.enabled = false;
                        UpgradeFoodQualityButton.interactable = false;
                    }
                }
            }
        }
        public void UpgradeFoodCount()
        {
            if (FoodCount < MAX_FOOD_COUNT
                && int.TryParse(ScoreTMP.text, out int score)
                && score >= 300)
            {
                FoodCount++;
                ScoreTMP.text = (score - 300).ToString();
                UpgradeFoodCountTMP.text = $"Food Count: {FoodCount}\n Price: 300";


                if (FoodCount == MAX_FOOD_COUNT)
                {
                    UpgradeFoodCountButton.enabled = false;
                    UpgradeFoodCountButton.interactable = false;
                }
            }
        }
        private void OnDisable()
        {
            _input.OnStartTouch -= SpawnFood;
        }
        private void SpawnFood(InputContext input)
        {
            if (FoodPrefabs != null)
            {
                if (int.TryParse(ScoreTMP.text, out int currentCash)
                    && currentCash >= _price
                    && transform.childCount < FoodCount)
                {
                    var position = input.ScreenPosition.ToWorldCoord();
                    var food =
                        Instantiate(original: GetCurrentFood(FoodIndex),
                        position: position,
                        rotation: Quaternion.identity,
                        parent: transform);

                    food.name = Food.Name;

                    ScoreTMP.text = (currentCash - _price).ToString();
                }
            }
        }
    }
}
