using Assets.Contracts;
using Assets.Scripts;
using Assets.Scripts.Birds;
using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Random;

namespace Assets.Scripts.Birds
{
    public partial class Bird : BaseScript
    {
        public GameObject[] CoinPrefabs;
        public int Price = 100;
        public float EnergyConsumePerSecond = 10;

        private Rigidbody2D _rigidbody = null;
        private SpriteRenderer _sprite = null;
        private BirdContext _context = null;
        private Animator _animator = null;
        private Collider2D _collider = null;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();

            _sprite = GetComponent<SpriteRenderer>();

            _animator = GetComponent<Animator>();

            _collider = GetComponent<Collider2D>();

            StartStateMachine();

            ProduceCash();
        }

        private void Update()
        {
            _sprite.flipX = _rigidbody.velocity.x < 0;
        }
        private void StartStateMachine()
        {
            _context = new BirdContext(this);
            _context.Run(
                data: new BirdContextData(),
                state: GetAllBirdStates()
                    .ToList()
                    .First());
        }
        private Func<ITargetFinder, Vector3> GetPositionResolverHandler()
        {
            var foodManager = Global.GameObjects.GetGameObject(Global.FOOD_MANAGER_TAG);

            return (targetFinder) =>
            {
                var targets = foodManager
                    .GetComponentsInChildren<Transform>()
                    .Skip(Global.PARENT_TRANSFORM)
                    .ToList();

                targetFinder.UpdateTargets(targets);

                var targetPosition = targetFinder
                    .GetHighestPriorityTarget(transform.position)
                    .position;

                return targetPosition;
            };
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(Global.FOOD_TAG))
            {
                _context.Data.Channel.Enqueue((
                    key: BirdSignal.FoundFood,
                    value: collision));
            }
        }

        private void ProduceCash()
        {
            var timeStep = 25.0f;
            var sToHz = timeStep.GetSToHzHandler();
            var timeOutHz = sToHz(Range(75.0f, 95.0f));
            var coinManager = Global.GameObjects.GetGameObject(Global.CASH_MANAGER_TAG);

            IEnumerator produceCash(CancellationToken token)
            {
                var silverCoin = CoinPrefabs[0];
                var goldCoin = CoinPrefabs[1];
                var currentCoin = silverCoin;
                var time = 0;
                while (true)
                {
                    if (token.IsCancellationRequested) break;
                    yield return new WaitForSeconds(timeStep + Range(-5f, 5f));

                    var coin = Instantiate(
                        original: currentCoin,
                        position: transform.position,
                        rotation: Quaternion.identity,
                        parent: coinManager.transform);

                    if (++time >= timeOutHz
                        && currentCoin == silverCoin
                        && _context.State.ID == BirdEnum.Idling)
                    {
                        _context.Data.Channel.Enqueue((BirdSignal.Grown, null));
                        currentCoin = goldCoin;
                        EnergyConsumePerSecond *= 1.5f;
                    }
                }
            }

            StartCoroutine(produceCash(_cancelSource.Token));
        }
    }
}
