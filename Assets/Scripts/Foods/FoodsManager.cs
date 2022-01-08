using Assets.Scripts.Utilities;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FoodsManager : LifeTimeBase.SingletonScript<FoodsManager>
{
    private InputManager _input;
    public GameObject FoodPrefab;
    private void Awake()
    {
        _input = InputManager.Instance;
    }
    private void OnEnable()
    {
        _input.OnStartTouch += Touch;
    }
    private void OnDisable()
    {
        _input.OnEndTouch -= Touch;
    }
    private void Touch(Vector2 position, double time)
    {
        if (FoodPrefab != null)
        {
            position = position.ToWorldCoord();

            var food = Instantiate(
                original: FoodPrefab,
                position: position,
                rotation: Quaternion.identity,
                parent: transform);
        }
    }
}
