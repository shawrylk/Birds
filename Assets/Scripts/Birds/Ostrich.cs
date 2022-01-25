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
            var (move, resetInertia) = PidExtensions.GetPidHandler(options =>
            {
                options.X = (10f, 10f, 11f);
                options.Y = (5f, 0f, 1f);
                options.ClampX = (-7f, 7f);
                options.ClampY = (-7f, 0f);
                options.Transform = transform;
                options.Rigidbody2D = _rigidbody;
            });

            var randomTargetGenerator = new RandomPathGenerator();
            var positionHandler = transform.GetPositionResolver();
            var randomPositionHandler = new Func<Vector3>(
                () => positionHandler(randomTargetGenerator));

            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var yieldTimeStep = new WaitForSeconds(timeStep);
            var (regenEnergy, runOutEnergy) = EnergyConsumePerSecond.GetEnergyHanlder(sToHz(Range(5, 7)), sToHz);

            IEnumerator idlingHandler(Context context)
            {
                var birdContext = context as BirdContext;
                if (birdContext is null) yield return null;

                var hzedOut = sToHz(Range(1.7f, 2.3f));
                var getRandomPosition = randomPositionHandler.SampleAt(hzedOut);

                while (true)
                {
                    if (birdContext.Data.Channel.TryDequeue(out var result))
                    {
                        if (result.key == BirdSignal.EnergyRegen)
                        {
                            regenEnergy(Convert.ToInt32(result.value));
                        }
                    }

                    var position = getRandomPosition();
                    move(position, timeStep);

                    if (runOutEnergy())
                    {
                        birdContext.State = huntingState;
                        break;
                    };

                    yield return yieldTimeStep;
                }
            }
            return idlingHandler;
        }

        protected override Func<Context, IEnumerator> HandleHuntingState()
        {
            var (move, resetInertia) = PidExtensions.GetPidHandler(options =>
            {
                options.X = (10f, 10f, 11f);
                options.Y = (5f, 0f, 0f);
                options.ClampX = (-7f, 7f);
                options.ClampY = (-7f, 0f);
                options.Transform = transform;
                options.Rigidbody2D = _rigidbody;
            });

            var positionHandler = transform.GetPositionResolver();
            var targetFinder = new TargetFinder();

            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var yieldTimeStep = new WaitForSeconds(timeStep);
            var (regenEnergy, runOutEnergy) = EnergyConsumePerSecond.GetEnergyHanlder(sToHz(Range(7, 10)), sToHz);

            var foodManager = Global.GameObjects.GetGameObject(Global.FOOD_MANAGER_TAG);

            IEnumerator huntingHandler(Context context)
            {
                var birdContext = context as BirdContext;
                if (birdContext is null) yield return null;

                while (true)
                {
                    if (birdContext.Data.Channel.TryDequeue(out var result))
                    {
                        if (result.key == BirdSignal.FoundFood
                            && result.value is Collider2D c
                            && c != null)
                        {
                            var energy = c.gameObject.GetComponent<Food>().Energy;
                            _lifeCycle.Data.Channel.Enqueue((BirdSignal.EnergyRegen, energy));
                            Destroy(c.gameObject, 0.1f);

                            _sprite.material.SetFloat("_GrayscaleAmount", 0.0f);
                            birdContext.State = idlingState;
                            break;
                        }
                    }

                    var targets = foodManager
                        .GetComponentsInChildren<Transform>()
                        .Skip(Global.PARENT_TRANSFORM)
                        .ToList();

                    targetFinder.UpdateTargets(targets);
                    var position = positionHandler(targetFinder);

                    move(position, timeStep);

                    if (runOutEnergy())
                    {
                        _lifeCycle.State = starvingState;
                        break;
                    }

                    yield return yieldTimeStep;
                }
            }
            return huntingHandler;
        }
        protected override Func<Context, IEnumerator> HandleStarvingState()
        {
            var (move, resetInertia) = PidExtensions.GetPidHandler(options =>
            {
                options.X = (10f, 10f, 11f);
                options.Y = (5f, 0f, 0f);
                options.ClampX = (-7f, 7f);
                options.ClampY = (-7f, 0f);
                options.Transform = transform;
                options.Rigidbody2D = _rigidbody;
            });

            var positionHandler = transform.GetPositionResolver();
            var targetFinder = new TargetFinder();

            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var yieldTimeStep = new WaitForSeconds(timeStep);
            var (regenEnergy, runOutEnergy) = EnergyConsumePerSecond.GetEnergyHanlder(sToHz(Range(7, 10)), sToHz);

            var foodManager = Global.GameObjects.GetGameObject(Global.FOOD_MANAGER_TAG);
            var grayScale = 0.0f;

            IEnumerator starvingHandler(Context context)
            {
                var birdContext = context as BirdContext;
                if (birdContext is null) yield return null;

                while (true)
                {
                    if (birdContext.Data.Channel.TryDequeue(out var result))
                    {
                        if (result.key == BirdSignal.FoundFood
                            && result.value is Collider2D c
                            && c != null)
                        {
                            var energy = c.gameObject.GetComponent<Food>().Energy;
                            _lifeCycle.Data.Channel.Enqueue((BirdSignal.EnergyRegen, energy));
                            Destroy(c.gameObject, 0.1f);

                            _sprite.material.SetFloat("_GrayscaleAmount", 0.0f);
                            birdContext.State = idlingState;
                            break;
                        }
                    }

                    var targets = foodManager
                        .GetComponentsInChildren<Transform>()
                        .Skip(Global.PARENT_TRANSFORM)
                        .ToList();

                    targetFinder.UpdateTargets(targets);
                    var position = positionHandler(targetFinder);

                    move(position, timeStep);

                    if (grayScale < 1) grayScale += 0.03f;
                    _sprite.material.SetFloat("_GrayscaleAmount", grayScale);

                    if (runOutEnergy())
                    {
                        _lifeCycle.State = starvingState;
                        break;
                    }

                    yield return yieldTimeStep;
                }
            }

            return starvingHandler;
        }
    }
}
