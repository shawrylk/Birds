using Assets.Scripts.Fishes;
using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class FishManager : BaseScript
    {
        private static FishManager _instance = null;
        public static FishManager Instance => _instance;

        [SerializeField]
        private GameObject[] _fishPrefabs = null;

        private CashManager _cashManager = null;
        public int DefaultFishCount = 1;
        public ConcurrentDictionary<string, List<GameObject>> AllFishes = new ConcurrentDictionary<string, List<GameObject>>();

        private Dictionary<string, Transform> _boundaries;
        private Action<int> _spawnFish;
        private int _fish_last_index;

        private void Awake()
        {
            _instance = FindObjectOfType<FishManager>();
            _boundaries = Global.Items[Global.BOUNDARIES_TAG] as Dictionary<string, Transform>;
            _spawnFish = GetSpawnBirdHandler();
            _fish_last_index = _fishPrefabs.Length - 1;
            _cashManager = CashManager.Instance;
            Enumerable.Range(0, DefaultFishCount)
                .ToList()
                .ForEach(num => SpawnFish(0));
            StartCoroutine(SpawnFishCoroutine());
        }

        private IEnumerator SpawnFishCoroutine()
        {
            yield return new WaitForSeconds(60f);
            var yieldTimeStep = new WaitForSeconds(15f);
            while (true)
            {
                SpawnFish(0);
                yield return yieldTimeStep;
            }
        }

        private GameObject GetFishAtIndex(int index)
        {
            if (index <= _fish_last_index)
            {
                return _fishPrefabs[index];
            }
            return _fishPrefabs[_fish_last_index];
        }
        public void SpawnFish(int index)
        {
            var bird = GetFishAtIndex(index);
            _spawnFish?.Invoke(index);
        }

        private Action<int> GetSpawnBirdHandler()
        {
            var offset = 2f;
            var horizontalMargin = new Vector3(offset, 0, 0);
            var verticalMargin = new Vector3(0, offset, 0);
            var left = _boundaries[Global.LEFT_BOUNDARY].position + horizontalMargin;
            var right = _boundaries[Global.RIGHT_BOUNDARY].position - horizontalMargin;
            var bottom = _boundaries[Global.BOTTOM_BOUNDARY].position + verticalMargin;
            gameObject.transform.position = _boundaries[Global.TOP_BOUNDARY].position;

            return (int index) =>
            {
                if (_fishPrefabs != null)
                {
                    var position = left.Range(right);
                    position += bottom;

                    var newFish = Instantiate(
                       original: GetFishAtIndex(index),
                       position: position,
                       rotation: Quaternion.identity,
                       parent: gameObject.transform);

                    if (index == 0)
                    {
                        newFish.name = FishBase.Name;
                    }

                    AllFishes.Add(newFish);

                    newFish.GetComponent<Rigidbody2D>().AddForce(Vector2.up * UnityEngine.Random.Range(20f, 40f));
                }
            };
        }
    }
}
