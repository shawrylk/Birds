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
        private float _maxHeight1 = 0.0f;
        private float _maxHeight2 = 0.0f;
        protected override void Awake()
        {
            base.Awake();
            //var birdMask = LayerMask.GetMask(Global.BIRDS_MARK_LAYER);
            //Physics2D.IgnoreLayerCollision(birdMask, birdMask, false);
            var floorY = (Global.Items[Global.BOUNDARIES_TAG] as Dictionary<string, Transform>)
                [Global.BOTTOM_BOUNDARY]
                .position.y;

            _floorY = floorY + _collider.bounds.size.y * transform.localScale.y;
            _maxHeight1 = Screen.height / Global.PIXEL_PER_UNIT * 0.4f + _floorY;
            _maxHeight2 = Screen.height / Global.PIXEL_PER_UNIT * 0.5f + _floorY;
            _birdManager = BirdManager.Instance;

            ProduceCash();
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.name.ToLower() == Global.BOTTOM_BOUNDARY)
            {
                _conductor.Context.Channel.Enqueue((BirdSignal.Grounded, null));
            }
            base.OnTriggerEnter2D(collision);
        }
        private void ProduceCash()
        {
            var coroutine = this.GetProduceCashCoroutine(options =>
            {
                options.CoroutineTime = (timeStep: 10.0f, variationRange: 1f);
                options.CashInfo = new List<(float timeOut, float variationRange, GameObject cash)>
                {
                    (timeOut: 0f, variationRange: 5f, cash: _cashPrefabs[0]),
                };
                options.HzedOutHandler = null;
                options.InitialVelocity = new Vector2(0, 1f);
            });

            StartCoroutine(coroutine?.Invoke(_cancelSource.Token));
        }
        protected override Func<IEnumerator> HandleIdlingState(BirdState state)
        {
            var (move, resetInertia) = PidExtensions.GetPidHandler(options =>
            {
                options.X = (10f, 10f, 11f);
                options.Y = (10f, 10f, 11f);
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

            IEnumerator idlingHandler()
            {
                var birdConductor = state.Conductor;
                if (birdConductor is null) yield return null;

                var (regenEnergy, runOutEnergy) = _energyConsumePerSecond.GetEnergyHanlder(sToHz(Range(5, 7)), sToHz);

                var hzedOut = sToHz(Range(1.7f, 2.3f));
                var getRandomPosition = randomPositionHandler.SampleAt(hzedOut);

                while (true)
                {
                    if (birdConductor.Context.Channel.TryDequeue(out var result))
                    {
                        if (result.key == BirdSignal.EnergyRegen)
                        {
                            regenEnergy(Convert.ToInt32(result.value));
                        }
                    }

                    var position = getRandomPosition();
                    position.y = _floorY;
                    move(position, timeStep);

                    if (runOutEnergy())
                    {
                        birdConductor.ChangeState(huntingState);
                        break;
                    };

                    yield return yieldTimeStep;
                }
            }
            return idlingHandler;
        }

        protected override Func<IEnumerator> HandleHuntingState(BirdState state)
        {
            var (move, resetInertia) = PidExtensions.GetPidHandler(options =>
            {
                options.X = (10f, 10f, 11f);
                options.Y = (70f, 85f, 45f);
                options.ClampX = (-5f, 5f);
                options.ClampY = (-21f, 21f);
                options.Transform = transform;
                options.Rigidbody2D = _rigidbody;
            });

            var targetFinder = new TargetFinder();

            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var yieldTimeStep = new WaitForSeconds(timeStep);

            var foodManager = Global.GameObjects.GetGameObject(Global.FOOD_MANAGER_TAG);

            IEnumerator huntingHandler()
            {
                var birdConductor = state.Conductor;
                if (birdConductor is null) yield return null;

                var (regenEnergy, runOutEnergy) = _energyConsumePerSecond.GetEnergyHanlder(sToHz(Range(7, 10)), sToHz);

                while (true)
                {
                    if (birdConductor.Context.Channel.TryDequeue(out var result))
                    {
                        if (result.key == BirdSignal.FoundFood
                            && result.value is Collider2D c
                            && c != null)
                        {
                            var energy = c.gameObject.GetComponent<Food>().Energy;
                            _conductor.Context.Channel.Enqueue((BirdSignal.EnergyRegen, energy));
                            Destroy(c.gameObject, 0.1f);

                            resetInertia();

                            _sprite.material.SetFloat("_GrayscaleAmount", 0.0f);
                            birdConductor.ChangeState(idlingState);
                            break;
                        }
                        else if (result.key == BirdSignal.Grounded)
                        {
                            var targets = foodManager
                                .GetComponentsInChildren<Transform>()
                                .Skip(Global.PARENT_TRANSFORM)
                                .ToList();

                            targetFinder.UpdateTargets(targets);
                        }
                    }


                    var (targetTransform, targetPosition) = targetFinder.GetHighestPriorityTarget(transform);

                    if (targetTransform is null || targetPosition.y > _maxHeight1)
                    {
                        targetPosition.y = _floorY;
                    }

                    move(targetTransform?.position ?? targetPosition, timeStep);

                    if (runOutEnergy())
                    {
                        _conductor.ChangeState(starvingState);
                        break;
                    }

                    yield return yieldTimeStep;
                }
            }
            return huntingHandler;
        }
        protected override Func<IEnumerator> HandleStarvingState(BirdState state)
        {
            var (move, resetInertia) = PidExtensions.GetPidHandler(options =>
            {
                options.X = (10f, 10f, 11f);
                options.Y = (80f, 90f, 50f);
                options.ClampX = (-7f, 7f);
                options.ClampY = (-28f, 28f);
                options.Transform = transform;
                options.Rigidbody2D = _rigidbody;
            });

            var targetFinder = new TargetFinder();

            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var yieldTimeStep = new WaitForSeconds(timeStep);

            var foodManager = Global.GameObjects.GetGameObject(Global.FOOD_MANAGER_TAG);
            var grayScale = 0.0f;

            IEnumerator starvingHandler()
            {
                var birdConductor = state.Conductor;
                if (birdConductor is null) yield return null;

                var (regenEnergy, runOutEnergy) = _energyConsumePerSecond.GetEnergyHanlder(sToHz(Range(7, 10)), sToHz);

                while (true)
                {
                    if (birdConductor.Context.Channel.TryDequeue(out var result))
                    {
                        if (result.key == BirdSignal.FoundFood
                            && result.value is Collider2D c
                            && c != null)
                        {
                            var energy = c.gameObject.GetComponent<Food>().Energy;
                            _conductor.Context.Channel.Enqueue((BirdSignal.EnergyRegen, energy));
                            Destroy(c.gameObject, 0.1f);

                            resetInertia();

                            _sprite.material.SetFloat("_GrayscaleAmount", 0.0f);
                            birdConductor.ChangeState(idlingState);
                            break;
                        }
                        else if (result.key == BirdSignal.Grounded)
                        {
                            var targets = foodManager
                                .GetComponentsInChildren<Transform>()
                                .Skip(Global.PARENT_TRANSFORM)
                                .ToList();

                            targetFinder.UpdateTargets(targets);
                        }
                    }

                    var (targetTransform, targetPosition) = targetFinder.GetHighestPriorityTarget(transform);

                    if (targetTransform is null || targetPosition.y > _maxHeight2)
                    {
                        targetPosition.y = _floorY;
                    }

                    move(targetTransform?.position ?? targetPosition, timeStep);

                    if (grayScale < 1) grayScale += 0.03f;
                    _sprite.material.SetFloat("_GrayscaleAmount", grayScale);

                    if (runOutEnergy())
                    {
                        _conductor.ChangeState(deathState);
                        break;
                    }

                    yield return yieldTimeStep;
                }
            }

            return starvingHandler;
        }
    }
}
