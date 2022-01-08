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

namespace Assets.Scripts.Birds
{
    public partial class Bird : BaseScript
    {
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
            GameObject foods = null;

            if (!Global.GameObjects.TryGetValue(Global.FOODS_TAG, out foods))
            {
                foods = GameObject
                    .FindGameObjectWithTag(Global.FOODS_TAG);
                Global.GameObjects.TryAdd(Global.FOODS_TAG, foods);
            }

            return (targetFinder) =>
            {
                var targets = foods
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
        private Action<Vector3, float> GetPidHandler()
        {
            const float MAX_MAGNITUDE = 2f;
            IPidController pidX = new PidController();
            IPidController pidY = new PidController();

            var p = 0.6f;
            var i = 0.1344f;
            var d = 1.586f;

            pidX.Ready(p, i, d);

            pidY.Ready(p, i, d);

            return (targetPosition, timeStep) =>
            {
                var distanceVector = targetPosition - transform.position;

                var magnitudeX = pidX.GetOutputValue(distanceVector.x, timeStep);
                var magnitudeY = pidY.GetOutputValue(distanceVector.y, timeStep);

                magnitudeX = Mathf.Clamp(magnitudeX, -MAX_MAGNITUDE, MAX_MAGNITUDE);
                magnitudeY = Mathf.Clamp(magnitudeY, -MAX_MAGNITUDE, MAX_MAGNITUDE);

                _rigidbody.AddRelativeForce(magnitudeX * Vector2.right);
                _rigidbody.AddRelativeForce(magnitudeY * Vector2.up);
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
    }
}
