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
        private (Action<Vector3, float> pid, Action reset) GetPidHandler(float p, float i, float d, float minClamp = 3f, float maxClamp = 3f)
        {
            IPidController pidX = new PidController();
            IPidController pidY = new PidController();

            pidX.Ready(p, i, d);
            pidY.Ready(p, i, d);

            return ((targetPosition, timeStep) =>
            {
                var distanceVector = targetPosition - transform.position;

                var magnitudeX = pidX.GetOutputValue(distanceVector.x, timeStep);
                var magnitudeY = pidY.GetOutputValue(distanceVector.y, timeStep);

                magnitudeX = Mathf.Clamp(magnitudeX, minClamp, maxClamp);
                magnitudeY = Mathf.Clamp(magnitudeY, minClamp, maxClamp);

                _rigidbody.AddRelativeForce(magnitudeX * Vector2.right);
                _rigidbody.AddRelativeForce(magnitudeY * Vector2.up);
            },
            () =>
            {
                pidX.Ready(p, i, d);
                pidY.Ready(p, i, d);
            }
            );
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
            var timeStep = 30.0f;
            var sToHz = timeStep.GetSToHzHandler();
            var timeOutHz = sToHz(Range(55.0f, 65.0f));
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
                    yield return new WaitForSeconds(timeStep);

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
                        timeStep = 45.0f;
                    }
                }
            }

            StartCoroutine(produceCash(_cancelSource.Token));
        }
    }
}
