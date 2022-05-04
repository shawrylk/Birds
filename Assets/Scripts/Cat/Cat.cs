using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Random;

namespace Assets.Scripts.Cat
{
    public class Cat : AnimalBase
    {
        [SerializeField] private CashManager _cashManager;

        protected override void Awake()
        {
            base.Awake();
            StartCoroutine(CollectCash());
        }
        private IEnumerator CollectCash()
        {
            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var timeOutHz = sToHz(Range(8, 10));
            var positionHandler = transform.GetPositionResolver();
            var (pidHandler, resetPid) = PidExtensions.GetPidHandler(options =>
            {
                options.Enable = (true, false);
                options.X = (29f, 29f, 0f);
                options.ClampX = (-4.4f, 4.4f);
                options.Transform = transform;
                options.Rigidbody2D = _rigidbody;
            });
            var targetFinder = new TargetFinder();


            _animator.enabled = true;

            var randomTargetGenerator = new RandomPathGenerator();
            randomTargetGenerator.UpdateTargets(null);
            var hzedOut = sToHz(Range(1.7f, 2.3f));
            var getRandomPosition = new Func<Vector3>(
                () => positionHandler(randomTargetGenerator))
                .SampleAt(hzedOut);

            while (true)
            {
                yield return new WaitForSeconds(timeStep);

                var targets = _cashManager
                    .GetComponentsInChildren<Transform>()
                    .Skip(Global.PARENT_TRANSFORM)
                    .ToList();

                if (targets.Count == 0)
                {
                    var position = getRandomPosition();
                    pidHandler(position, timeStep);
                }
                else
                {
                    targetFinder.UpdateTargets(targets);
                    var position = positionHandler(targetFinder);
                    pidHandler(position, timeStep);
                }

                //if (_rigidbody.velocity.sqrMagnitude > 3.0f)
                //{
                //    _animator.Play("bird_1_fly_fast");
                //}
                //else
                //{
                //    _animator.Play("fly");
                //}
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(Global.CASH_TAG))
            {
                collision.gameObject.GetComponent<Cash>().GetValue();
            }
        }
    }
}
