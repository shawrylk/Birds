using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Random;

namespace Assets.Scripts.Birds
{
    public class Ostrich : BirdBase
    {
        private BirdManager _birdManager;
        private float _floorY;
        protected override void Awake()
        {
            base.Awake();
            //var birdMask = LayerMask.GetMask(Global.BIRDS_MARK_LAYER);
            //Physics2D.IgnoreLayerCollision(birdMask, birdMask, false);
            var floorY = (Global.Items[Global.BOUNDARIES_TAG] as Dictionary<string, Transform>)
                [Global.BOTTOM_BOUNDARY]
                .position.y;

            _floorY = floorY + _collider.bounds.size.y * transform.localScale.y;
            print($"Floor = {floorY}");
            print($"Floor offset = {_floorY}");
            _birdManager = BirdManager.Instance;
            ProduceCash();
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            base.OnTriggerEnter2D(collision);
        }
        private void ProduceCash()
        {
            var coroutine = this.GetProduceCashCoroutine(options =>
            {
                options.CoroutineTime = (timeStep: 10.0f, variationRange: 1f);
                options.CashInfo = new List<(float timeOut, float variationRange, GameObject cash)>
                {
                    (timeOut: 0f, variationRange: 5f, cash: CashPrefabs[0]),
                };
                options.HzedOutHandler = null;
                options.InitialVelocity = new Vector2(0, 1f);
            });

            StartCoroutine(coroutine?.Invoke(_cancelSource.Token));
        }
        protected override Func<Context, IEnumerator> HandleIdlingState()
        {
            var positionHandler = transform.GetPositionResolverHandler();
            var (pidHandler, resetPid) = PidExtensions.GetPidHandler(options =>
            {
                options.X = (10f, 10f, 11f);
                options.Y = (5f, 0f, 1f);
                options.ClampX = (-7f, 7f);
                options.ClampY = (-7f, 7f);
                options.Transform = transform;
                options.Rigidbody2D = _rigidbody;
            });
            var randomTargetGenerator = new RandomPathGenerator();
            randomTargetGenerator.UpdateTargets(null);

            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var timeOutHz = sToHz(Range(5, 7));

            IEnumerator idlingHandler(Context context)
            {
                var birdContext = context as BirdContext;
                if (birdContext is null) yield return null;

                var time = 0;

                Func<Vector3> getRandomPositionHandler(float timeOut)
                {
                    var hzOut = sToHz(timeOut);
                    var ret = default(Vector3);
                    var first = true;
                    IEnumerable<int> wait()
                    {
                        while (true)
                        {
                            var i = 0;
                            while (i < hzOut)
                            {
                                yield return i++;
                            }
                            yield break;
                        }
                    }
                    var it = wait().GetEnumerator();
                    return () =>
                    {
                        if (!it.MoveNext() || first)
                        {
                            first = false;
                            it = wait().GetEnumerator();
                            ret = positionHandler(randomTargetGenerator);
                        }
                        return ret;
                    };
                }

                var getRandomPosition = getRandomPositionHandler(Range(1.7f, 2.3f));

                while (true)
                {
                    if (birdContext.Data.Channel.TryDequeue(out var result))
                    {
                        if (result.key == BirdSignal.EnergyRegen)
                        {
                            var Energytime = Convert.ToInt32(result.value) / EnergyConsumePerSecond;
                            timeOutHz = sToHz(Energytime);
                        }
                    }

                    var position = getRandomPosition();
                    position.y = _floorY;
                    pidHandler(position, timeStep);

                    if (++time >= timeOutHz)
                    {
                        birdContext.State = huntingState;
                        break;
                    }

                    yield return new WaitForSeconds(timeStep);
                }
            }
            return idlingHandler;
        }

        protected override Func<Context, IEnumerator> HandleHuntingState()
        {
            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var timeOutHz = sToHz(Range(7, 10));
            var positionHandler = transform.GetPositionResolverHandler();
            var (pidHandler, resetPid) = PidExtensions.GetPidHandler(options =>
            {
                options.X = (10f, 10f, 11f);
                options.Y = (5f, 0f, 0f);
                options.ClampX = (-7f, 7f);
                options.ClampY = (-7f, 7f);
                options.Transform = transform;
                options.Rigidbody2D = _rigidbody;
            });
            var targetFinder = new TargetFinder();


            IEnumerator huntingHandler(Context context)
            {
                var birdContext = context as BirdContext;
                if (birdContext is null) yield return null;
                var time = 0;

                _collider.isTrigger = true;
                _animator.enabled = true;

                while (true)
                {
                    yield return new WaitForSeconds(timeStep);

                    if (birdContext.Data.Channel.TryDequeue(out var result))
                    {
                        if (result.key == BirdSignal.FoundFood
                            && result.value is Collider2D c
                            && c != null)
                        {
                            var energy = c.gameObject.GetComponent<Food>().Energy;
                            _lifeCycle.Data.Channel.Enqueue((BirdSignal.EnergyRegen, energy));

                            birdContext.State = idlingState;
                            _sprite.material.SetFloat("_GrayscaleAmount", 0.0f);

                            Destroy(c.gameObject, 0.1f);
                            _collider.isTrigger = false;
                            break;
                        }
                    }

                    var foodManager = Global.GameObjects.GetGameObject(Global.FOOD_MANAGER_TAG);

                    var targets = foodManager
                        .GetComponentsInChildren<Transform>()
                        .Skip(Global.PARENT_TRANSFORM)
                        .ToList();

                    targetFinder.UpdateTargets(targets);

                    var position = positionHandler(targetFinder);
                    position.y = _floorY;
                    print($"target = {position}");
                    pidHandler(position, timeStep);

                    //if (_rigidbody.velocity.sqrMagnitude > 3.0f)
                    //{
                    //    _animator.Play("bird_1_fly_fast");
                    //}
                    //else
                    //{
                    //    _animator.Play("fly");
                    //}

                    if (time++ >= timeOutHz)
                    {
                        _lifeCycle.State = starvingState;
                        break;
                    }
                }
            }
            return huntingHandler;
        }
        protected override Func<Context, IEnumerator> HandleStarvingState()
        {
            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var timeOutHz = sToHz(Range(7, 10));
            var positionHandler = transform.GetPositionResolverHandler();
            var (pidHandler, resetPid) = PidExtensions.GetPidHandler(options =>
            {
                options.X = (10f, 10f, 11f);
                options.Y = (5f, 0f, 0f);
                options.ClampX = (-9f, 9f);
                options.ClampY = (-9f, 9f);
                options.Transform = transform;
                options.Rigidbody2D = _rigidbody;
            });
            var targetFinder = new TargetFinder();


            IEnumerator starvingHandler(Context context)
            {
                var birdContext = context as BirdContext;
                if (birdContext is null) yield return null;
                var time = 0;
                var grayScale = 0.0f;

                _animator.enabled = true;

                //_animator.Play("bird_1_starving");

                while (true)
                {
                    yield return new WaitForSeconds(timeStep);

                    if (birdContext.Data.Channel.TryDequeue(out var result))
                    {
                        if (result.key == BirdSignal.FoundFood
                            && result.value is Collider2D c
                            && c != null)
                        {
                            var energy = c.gameObject.GetComponent<Food>().Energy;
                            _lifeCycle.Data.Channel.Enqueue((BirdSignal.EnergyRegen, energy));

                            birdContext.State = idlingState;
                            _sprite.material.SetFloat("_GrayscaleAmount", 0.0f);

                            Destroy(c.gameObject, 0.1f);
                            break;
                        }
                    }
                    var foodManager = Global.GameObjects.GetGameObject(Global.FOOD_MANAGER_TAG);

                    var targets = foodManager
                        .GetComponentsInChildren<Transform>()
                        .Skip(Global.PARENT_TRANSFORM)
                        .ToList();

                    targetFinder.UpdateTargets(targets);
                    var position = positionHandler(targetFinder);
                    position.y = _floorY;
                    pidHandler(position, timeStep);

                    if (grayScale < 1) grayScale += 0.03f;
                    _sprite.material.SetFloat("_GrayscaleAmount", grayScale);

                    if (time++ >= timeOutHz)
                    {
                        _lifeCycle.State = deathState;
                        break;
                    }
                }
            }
            return starvingHandler;
        }
    }
}
