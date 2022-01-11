using Assets.Scripts.Utilities;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts
{
    [DefaultExecutionOrder(Global.LEVEL_MANAGER_ORDER)]
    public class LevelManager : BaseScript
    {
        public GameObject CashManager;
        public GameObject FoodManager;
        public GameObject BirdManager;
        public GameObject Boundaries;

        private void Awake()
        {
        }
    }
}