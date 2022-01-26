using Assets.Scripts;
using Assets.Scripts.Birds;
using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
[DefaultExecutionOrder(Global.BIRD_MANAGER_ORDER)]
public class BirdManager : BaseScript
{
    private static BirdManager _instance = null;
    public static BirdManager Instance => _instance;

    [SerializeField]
    private GameObject[] _birdPrefabs = null;
    [SerializeField]
    private TextMeshProUGUI _scoreTMP = null;

    public int DefaultBirdCount = 2;
    public ConcurrentDictionary<string, List<GameObject>> AllBirds = new ConcurrentDictionary<string, List<GameObject>>();

    private Dictionary<string, Transform> _boundaries;
    private Action<int> _spawnBird;
    private int _bird_last_index;

    private void Awake()
    {
        _instance = FindObjectOfType<BirdManager>();
        _boundaries = Global.Items[Global.BOUNDARIES_TAG] as Dictionary<string, Transform>;
        _spawnBird = GetSpawnBirdHandler();
        _bird_last_index = _birdPrefabs.Length - 1;
        Enumerable.Range(0, DefaultBirdCount)
            .ToList()
            .ForEach(num => SpawnBird(0));
    }

    private GameObject GetBirdAtIndex(int index)
    {
        if (index <= _bird_last_index)
        {
            return _birdPrefabs[index];
        }
        return _birdPrefabs[_bird_last_index];
    }
    public void SpawnBird(int index)
    {
        if (int.TryParse(_scoreTMP.text, out int score))
        {
            var bird = GetBirdAtIndex(index);
            var price = bird.GetComponent<BirdBase>().Price;
            if (score >= price)
            {
                _spawnBird?.Invoke(index);
                _scoreTMP.text = (score - price).ToString();
            }
        }
    }

    private Action<int> GetSpawnBirdHandler()
    {
        var offset = 2f;
        var horizontalMargin = new Vector3(offset, 0, 0);
        var verticalMargin = new Vector3(0, offset, 0);
        var left = _boundaries[Global.LEFT_BOUNDARY].position + horizontalMargin;
        var right = _boundaries[Global.RIGHT_BOUNDARY].position - horizontalMargin;
        var top = _boundaries[Global.TOP_BOUNDARY].position - verticalMargin;
        gameObject.transform.position = _boundaries[Global.TOP_BOUNDARY].position;

        return (int index) =>
        {
            if (_birdPrefabs != null)
            {
                var position = left.Range(right);
                position += top;
                var newBird = Instantiate(
                   original: GetBirdAtIndex(index),
                   position: position,
                   rotation: Quaternion.identity,
                   parent: gameObject.transform);

                if (index == 0)
                {
                    newBird.name = Pigeon.Name;
                }

                AllBirds.Add(newBird);

                newBird.GetComponent<Rigidbody2D>().AddForce(Vector2.down * UnityEngine.Random.Range(20f, 40f));
            }
        };
    }
}
