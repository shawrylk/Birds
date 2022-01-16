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
        public Transform[] Trees;
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
            print(_rigidbody.velocity.sqrMagnitude);
        }
        private void StartStateMachine()
        {
            _context = new BirdContext(this);
            _context.Run(
                data: new BirdContextData(),
                state: GetAllBirdStates()
                    .ToList()
                    .Skip(1)
                    .First());
        }
        private Func<ITargetFinder, Vector3> GetPositionResolverHandler()
        {
            return (targetFinder) =>
            {
                var targetPosition = targetFinder
                    .GetHighestPriorityTarget(transform)
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
            var timeStep = 5.0f;
            var sToHz = timeStep.GetSToHzHandler();
            var timeOutHz = sToHz(Range(65.0f, 70.0f));
            var timeOutHz2 = sToHz(Range(185.0f, 195.0f));
            var coinManager = Global.GameObjects.GetGameObject(Global.CASH_MANAGER_TAG);

            IEnumerator produceCash(CancellationToken token)
            {
                var silverCoin = CoinPrefabs[0];
                var goldCoin = CoinPrefabs[1];
                var currentCoin = default(GameObject);
                var time = 0;
                while (true)
                {
                    if (token.IsCancellationRequested) break;
                    yield return new WaitForSeconds(timeStep + Range(-2f, 2f));

                    if (currentCoin != null)
                    {
                        var coin = Instantiate(
                            original: currentCoin,
                            position: transform.position,
                            rotation: Quaternion.identity,
                            parent: coinManager.transform);
                    }

                    if (++time >= timeOutHz
                        && currentCoin == null
                        && _context.State.ID == BirdEnum.Idling)
                    {
                        _context.Data.Channel.Enqueue((BirdSignal.GrownStage1, null));
                        currentCoin = silverCoin;
                        EnergyConsumePerSecond *= 1.5f;
                    }
                    if (++time >= timeOutHz2
                        && currentCoin == silverCoin
                        && _context.State.ID == BirdEnum.Idling)
                    {
                        _context.Data.Channel.Enqueue((BirdSignal.GrownStage2, null));
                        currentCoin = goldCoin;
                        EnergyConsumePerSecond *= 1.5f;
                    }

                }
            }

            StartCoroutine(produceCash(_cancelSource.Token));
        }
    }
}
