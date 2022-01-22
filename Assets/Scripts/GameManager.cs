using Assets.Scripts.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public GameObject FoodsManager { get; set; }
        public GameObject CashManager { get; set; }
        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
        }

        private void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            var gameObjects = scene.GetRootGameObjects().ToList();
            gameObjects.ForEach(obj =>
            {
                if (obj.transform.CompareTag(Global.FOOD_MANAGER_TAG))
                {
                    FoodsManager = obj;
                }
                else if (obj.transform.CompareTag(Global.CASH_MANAGER_TAG))
                {
                    CashManager = obj;
                }
            });
            SetUpManagers();
        }

        private void SetUpManagers()
        {
            var script = FoodsManager.GetComponent<AnimalBase>();
        }
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
        }
    }
}
