using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Random;
using Channel = System.Collections.Concurrent.ConcurrentQueue<(Assets.Scripts.Birds.BirdSignal signal, object data)>;

namespace Assets.Scripts.Birds
{
    public partial class BirdBase : BaseScript
    {
        public int Price = 100;
        public float EnergyConsumePerSecond = 10;
        public GameObject[] CashPrefabs;

        protected Rigidbody2D _rigidbody = null;
        protected SpriteRenderer _sprite = null;
        protected BirdContext _lifeCycle = null;
        protected Animator _animator = null;
        protected Collider2D _collider = null;
        protected string _foodTag = Global.FOOD_TAG;
        protected Channel _foodChannel = null;

        protected virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _collider = GetComponent<Collider2D>();
            StartLifeCycleOfBird();
        }

        protected virtual void Update()
        {
            _sprite.flipX = _rigidbody.velocity.x < 0;
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(_foodTag))
            {
                _foodChannel.Enqueue((
                    signal: BirdSignal.FoundFood,
                    data: collision));
            }
        }
        protected virtual void StartLifeCycleOfBird()
        {
            _lifeCycle = new BirdContext(this);
            _lifeCycle.Run(
                data: new BirdContextData(),
                state: GetAllBirdStates()
                    .ToList()
                    .First(s => s.ID == BirdEnum.Hunting));
            _foodChannel = _lifeCycle.Data.Channel;
        }

    }
}
