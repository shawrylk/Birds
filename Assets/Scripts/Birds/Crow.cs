using Assets.Contracts;
using Assets.Scripts.Fishes;
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
    public class Crow : BirdBase
    {
        private bool testGit = false;
        private int _foodIndex = 0;
        private BirdManager _birdManager;
        private FishManager _fishManager;
        protected override void Awake()
        {
            //var birdMask = LayerMask.GetMask(Global.BIRDS_MARK_LAYER);
            //Physics2D.IgnoreLayerCollision(birdMask, birdMask, false);
            base.Awake();
            _birdManager = BirdManager.Instance;
            _fishManager = FishManager.Instance;
            _foodNames = new List<string>{
                //FishBase.Name2,
                //FishBase.Name,
                Pigeon.Name
            };
            ProduceCash();
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
            });

            StartCoroutine(coroutine?.Invoke(_cancelSource.Token));
        }
        protected override Func<IEnumerator> HandleIdlingState(BirdState state)
        {
            var positionHandler = transform.GetPositionResolver();
            var (pidHandler, resetPid) = PidExtensions.GetPidHandler(options =>
            {
                options.X = (10f, 10f, 11f);
                options.Y = (10f, 10f, 11f);
                options.ClampX = (-7f, 7f);
                options.ClampY = (-7f, 7f);
                options.Transform = transform;
                options.Rigidbody2D = _rigidbody;
            });
            //var treeSlotFinder = new TreeFinder();
            //treeSlotFinder.UpdateTargets(Trees);

            var randomTargetGenerator = new RandomPathGenerator();
            randomTargetGenerator.UpdateTargets(null);

            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var timeOutHz = sToHz(Range(5, 7));

            IEnumerator idlingHandler()
            {
                var birdConductor = state.Conductor;
                if (birdConductor is null) yield return null;

                var time = 0;
                var hzedOut = sToHz(Range(1.7f, 2.3f));
                var getRandomPosition = new Func<Vector3>(
                    () => positionHandler(randomTargetGenerator))
                    .SampleAt(hzedOut);


                while (true)
                {
                    if (birdConductor.Context.Channel.TryDequeue(out var result))
                    {
                        if (result.key == BirdSignal.EnergyRegen)
                        {
                            var Energytime = Convert.ToInt32(result.value) / _energyConsumePerSecond;
                            timeOutHz = sToHz(Energytime);
                        }
                    }

                    var position = getRandomPosition();
                    pidHandler(position, timeStep);

                    if (++time >= timeOutHz)
                    {
                        birdConductor.ChangeState(huntingState);
                        break;
                    }

                    yield return new WaitForSeconds(timeStep);
                }
            }
            return idlingHandler;
        }

        protected override Func<IEnumerator> HandleHuntingState(BirdState state)
        {
            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var timeOutHz = sToHz(Range(7, 10));
            var positionHandler = transform.GetPositionResolver();
            var (pidHandler, resetPid) = PidExtensions.GetPidHandler(options =>
            {
                options.X = (10f, 10f, 11f);
                options.Y = (10f, 10f, 11f);
                options.ClampX = (-7f, 7f);
                options.ClampY = (-7f, 7f);
                options.Transform = transform;
                options.Rigidbody2D = _rigidbody;
            });
            var targetFinder = new TargetFinder();


            IEnumerator huntingHandler()
            {
                var birdConductor = state.Conductor;
                if (birdConductor is null) yield return null;
                var time = 0;

                _collider.isTrigger = true;
                _animator.enabled = true;

                while (true)
                {
                    yield return new WaitForSeconds(timeStep);

                    if (birdConductor.Context.Channel.TryDequeue(out var result))
                    {
                        if (result.key == BirdSignal.FoundFood
                            && result.value is Collider2D c
                            && c != null)
                        {
                            if (_foodIndex != _foodNames.IndexOf(c.name)) continue;

                            var energy = c.gameObject.GetComponent<IFood>().Energy;
                            _conductor.Context.Channel.Enqueue((BirdSignal.EnergyRegen, energy));

                            birdConductor.ChangeState(idlingState);
                            _sprite.material.SetFloat("_GrayscaleAmount", 0.0f);

                            Destroy(c.gameObject, 0.1f);
                            _collider.isTrigger = false;
                            break;
                        }
                    }
                    var listFood = default(List<GameObject>);
                    var targets = default(List<Transform>);

                    _foodIndex = 0;
                    while (_foodIndex < _foodNames.Count)
                    {
                        //_fishManager.AllFishes
                        //    .TryGetValue(_foodNames[_foodIndex], out listFood);
                        _birdManager.AllBirds
                            .TryGetValue(_foodNames[_foodIndex], out listFood);

                        if (listFood is null
                            || listFood.Count == 0)
                        {
                            _foodIndex++;
                            continue;
                        }

                        targets = listFood
                            ?.Where(g => g != null)
                            ?.Select(g => g.transform)
                            ?.ToList();

                        break;
                    }

                    targetFinder.UpdateTargets(targets);
                    var position = positionHandler(targetFinder);
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
                        _conductor.ChangeState(starvingState);
                        break;
                    }
                }
            }
            return huntingHandler;
        }
        protected override Func<IEnumerator> HandleStarvingState(BirdState state)
        {
            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var timeOutHz = sToHz(Range(7, 10));
            var positionHandler = transform.GetPositionResolver();
            var (pidHandler, resetPid) = PidExtensions.GetPidHandler(options =>
            {
                options.X = (10f, 10f, 11f);
                options.Y = (10f, 10f, 11f);
                options.ClampX = (-9f, 9f);
                options.ClampY = (-9f, 9f);
                options.Transform = transform;
                options.Rigidbody2D = _rigidbody;
            });
            var targetFinder = new TargetFinder();


            IEnumerator starvingHandler()
            {
                var birdConductor = state.Conductor;
                if (birdConductor is null) yield return null;
                var time = 0;
                var grayScale = 0.0f;

                _animator.enabled = true;

                //_animator.Play("bird_1_starving");

                while (true)
                {
                    yield return new WaitForSeconds(timeStep);

                    if (birdConductor.Context.Channel.TryDequeue(out var result))
                    {
                        if (result.key == BirdSignal.FoundFood
                            && result.value is Collider2D c
                            && c != null)
                        {
                            var energy = c.gameObject.GetComponent<Pigeon>().Energy;
                            _conductor.Context.Channel.Enqueue((BirdSignal.EnergyRegen, energy));

                            birdConductor.ChangeState(idlingState);
                            _sprite.material.SetFloat("_GrayscaleAmount", 0.0f);

                            Destroy(c.gameObject, 0.1f);
                            break;
                        }
                    }
                    var listFood = default(List<GameObject>);
                    var targets = default(List<Transform>);

                    _foodIndex = 0;
                    while (_foodIndex < _foodNames.Count)
                    {
                        //_fishManager.AllFishes
                        //    .TryGetValue(_foodNames[_foodIndex], out listFood);
                        _birdManager.AllBirds
                            .TryGetValue(_foodNames[_foodIndex], out listFood);

                        if (listFood is null
                            || listFood.Count == 0)
                        {
                            _foodIndex++;
                            continue;
                        }

                        targets = listFood
                            ?.Where(g => g != null)
                            ?.Select(g => g.transform)
                            ?.ToList();

                        break;
                    }

                    targetFinder.UpdateTargets(targets);
                    var position = positionHandler(targetFinder);
                    pidHandler(position, timeStep);

                    if (grayScale < 1) grayScale += 0.03f;
                    _sprite.material.SetFloat("_GrayscaleAmount", grayScale);

                    if (time++ >= timeOutHz)
                    {
                        _conductor.ChangeState(deathState);
                        break;
                    }
                }
            }
            return starvingHandler;
        }
    }
}
