using Assets.Contracts;
using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Enemy
{
    public class Enemy : BaseScript
    {
        private IInputManager _input;
        private Rigidbody2D _rigidbody;
        private SpriteRenderer _sprite = null;
        private Animator _animator = null;
        private Collider2D _collider = null;
        private EnemyManager _enemyManager;
        private GameObject _cashManager;
        private const float castThickness = 0.4f;
        private bool destroyed = false;

        public float Health = 100;
        public GameObject Cash;

        private void Awake()
        {
            _cashManager = Global.GameObjects.GetGameObject(Global.CASH_MANAGER_TAG);

            _enemyManager = Global.GameObjects.GetGameObject(Global.ENEMY_MANAGER_TAG).GetComponent<EnemyManager>();
            _enemyManager.TouchEvent += TouchHandler;

            _rigidbody = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _collider = GetComponent<Collider2D>();

            StartFindingBirdCoroutine();
        }

        protected override void OnDestroy()
        {
            _enemyManager.TouchEvent -= TouchHandler;
            base.OnDestroy();
        }
        private void StartFindingBirdCoroutine()
        {
            StartCoroutine(FindBird());
        }
        private IEnumerator FindBird()
        {
            var timeStep = 1f;
            var timeStep2 = 0.1f;
            var sToHz = timeStep2.GetSToHzHandler();
            var hzOut = sToHz(timeStep);
            var (pidHandler, resetPid) = transform.GetPidHandler(30f, 3f, 21f, -7f, 7f, _rigidbody);
            var target = default(Collider2D);
            var birdMask = LayerMask.GetMask(Global.BIRDS_MARK_LAYER);
            var randomPath = new RandomPathGenerator() as ITargetFinder;

            while (true)
            {
                yield return new WaitForSeconds(timeStep);

                target = Physics2D.OverlapCircle(transform.position, 20f, birdMask);

                var hz = 0;

                while (true)
                {
                    yield return new WaitForSeconds(timeStep2);

                    var position = default(Vector3);

                    if (target != null)
                    {
                        position = target.transform.position;
                    }
                    else
                    {
                        (_, position) = randomPath.GetHighestPriorityTarget(transform);
                    }

                    pidHandler(position, timeStep2);

                    if (hz++ >= hzOut)
                    {
                        break;
                    }
                }
            }
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
                        _rigidbody.AddForce(direction * 5);

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

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision != null
                && collision.transform.CompareTag(Global.BIRD_TAG))
            {
                Destroy(collision.gameObject);
            }
        }
    }
}
