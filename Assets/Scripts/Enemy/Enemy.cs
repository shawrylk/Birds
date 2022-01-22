using Assets.Contracts;
using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Random;

namespace Assets.Scripts.Enemy
{
    public class Enemy : AnimalBase
    {
        private IInputManager _input;
        private EnemyManager _enemyManager;
        private GameObject _cashManager;
        private const float castThickness = 1f;
        private bool destroyed = false;

        public float Health = 100;
        public GameObject Cash;

        protected override void Awake()
        {
            _cashManager = Global.GameObjects.GetGameObject(Global.CASH_MANAGER_TAG);

            _enemyManager = Global.GameObjects.GetGameObject(Global.ENEMY_MANAGER_TAG).GetComponent<EnemyManager>();
            _enemyManager.TouchEvent += TouchHandler;
            FindBird();
            base.Awake();
        }

        protected override void OnDestroy()
        {
            _enemyManager.TouchEvent -= TouchHandler;
            base.OnDestroy();
        }
        private void FindBird()
        {
            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var timeOutHz = sToHz(Range(7, 10));
            var positionHandler = transform.GetPositionResolverHandler();
            var (pidHandler, resetPid) = PidExtensions.GetPidHandler(options =>
            {
                options.X = (20f, 10f, 21f);
                options.Y = (20f, 10f, 21f);
                options.ClampX = (-10f, 10f);
                options.ClampY = (-10f, 10f);
                options.Transform = transform;
                options.Rigidbody2D = _rigidbody;
            }); var targetFinder = new TargetFinder();


            IEnumerator huntingHandler()
            {
                while (true)
                {
                    yield return new WaitForSeconds(timeStep);

                    var foodManager = Global.GameObjects.GetGameObject(Global.BIRD_MANAGER_TAG);

                    var targets = foodManager
                        .GetComponentsInChildren<Transform>()
                        .Skip(Global.PARENT_TRANSFORM)
                        .ToList();

                    targetFinder.UpdateTargets(targets);
                    var position = positionHandler(targetFinder);
                    pidHandler(position, timeStep);

                }
            }

            StartCoroutine(huntingHandler());
        }

        private void TouchHandler(InputContext input)
        {
            if (destroyed) return;

            var position = input.ScreenPosition.ToWorldCoord();
            var hits = Physics2D.CircleCastAll(position, castThickness, Vector2.zero);
            if (hits != null)
            {
                foreach (var hit in hits)
                {
                    if (hit.transform != null
                        && hit.transform.CompareTag(Global.ENEMY_TAG)
                        && hit.transform == transform)
                    {
                        var direction = ((Vector2)transform.position - hit.point).normalized;
                        _rigidbody.AddForce(direction * Range(10f, 20f));

                        Health -= 3f;

                        if (Health <= 0)
                        {
                            destroyed = true;
                            DestroySelf();
                        }

                        input.Handled = true;

                        break;
                    }
                }
            }
        }

        private void DestroySelf()
        {
            var timeStep = 0.1f;
            var fadeOut = 1.0f;
            var fadeOutStep = 0.1f;

            IEnumerator deathHandler()
            {
                _animator.enabled = false;
                _collider.enabled = false;
                _rigidbody.gravityScale = 1;

                while (fadeOut > 0)
                {
                    _sprite.color = new Color(
                        _sprite.color.r,
                        _sprite.color.g,
                        _sprite.color.b,
                        fadeOut -= fadeOutStep);

                    yield return new WaitForSeconds(timeStep);
                }

                Destroy(gameObject);
            }

            StartCoroutine(deathHandler());

            Enumerable.Range(0, 1)
                .ToList()
                .ForEach(num => GiveCash());

        }

        private void GiveCash()
        {
            var offset = new Vector3(
               UnityEngine.Random.Range(-0.3f, 0.3f),
               UnityEngine.Random.Range(0.3f, 0.8f),
               0);

            Instantiate(
                Cash,
                transform.position + offset,
                Quaternion.identity,
                _cashManager.transform);
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision != null
                && collision.transform.CompareTag(Global.BIRD_TAG))
            {
                Destroy(collision.gameObject);
            }
            else
            {
                base.OnTriggerEnter2D(collision);
            }
        }
    }
}
