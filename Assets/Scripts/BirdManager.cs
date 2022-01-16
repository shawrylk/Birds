using Assets.Scripts;
using Assets.Scripts.Birds;
using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
[DefaultExecutionOrder(Global.BIRD_MANAGER_ORDER)]
public class BirdManager : BaseScript
{
    public GameObject[] BirdPrefabs;
    public TextMeshProUGUI ScoreTMP;
    public int DefaultBirdCount = 2;
    private Dictionary<string, Transform> _boundaries;
    private Action<int> _spawnBird;
    private int _bird_last_index;
    public void Awake()
    {
        _boundaries = Global.Items[Global.BOUNDARIES_TAG] as Dictionary<string, Transform>;
        _spawnBird = GetSpawnBirdHandler();
        _bird_last_index = BirdPrefabs.Length - 1;
        Enumerable.Range(0, DefaultBirdCount)
            .ToList()
            .ForEach(num => SpawnBird(0));
    }

    private GameObject GetBirdAtIndex(int index)
    {
        if (index <= _bird_last_index)
        {
            return BirdPrefabs[index];
        }
        return BirdPrefabs[_bird_last_index];
    }
    public void SpawnBird(int index)
    {
        if (int.TryParse(ScoreTMP.text, out int score))
        {
            var bird = GetBirdAtIndex(index);
            var price = bird.GetComponent<Bird>().Price;
            if (score >= price)
            {
                _spawnBird?.Invoke(index);
                ScoreTMP.text = (score - price).ToString();
            }
        }
    }

    private Action<int> GetSpawnBirdHandler()
    {
        var offset = 0.6f;
        var horizontalMargin = new Vector3(offset, 0, 0);
        var verticalMargin = new Vector3(0, offset, 0);
        var left = _boundaries[Global.LEFT_BOUNDARY].position + horizontalMargin;
        var right = _boundaries[Global.RIGHT_BOUNDARY].position - horizontalMargin;
        var top = _boundaries[Global.TOP_BOUNDARY].position - verticalMargin;
        gameObject.transform.position = _boundaries[Global.TOP_BOUNDARY].position;

        return (int index) =>
        {
            if (BirdPrefabs != null)
            {
                var position = left.Range(right);
                position += top;
                var newBird = Instantiate(
                   original: GetBirdAtIndex(index),
                   position: position,
                   rotation: Quaternion.identity,
                   parent: gameObject.transform);

                newBird.GetComponent<Rigidbody2D>().AddForce(Vector2.down * UnityEngine.Random.Range(20f, 40f));
            }
        };
    }
}
