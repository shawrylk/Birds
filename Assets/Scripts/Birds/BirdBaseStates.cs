using Assets.Scripts.Fish;
using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.Scripts.Lock.Guard;
using static UnityEngine.Random;

namespace Assets.Scripts.Birds
{
    public partial class BirdBase : AnimalBase
    {
        public List<BirdState> ListStates;
        #region Local Variables
        protected BirdState idlingState = null;
        protected BirdState huntingState = null;
        protected BirdState starvingState = null;
        protected BirdState deathState = null;
        protected BirdState capturedState = null;
        #endregion

        #region Local Functions
        BirdState GetIdlingState()
        {
            idlingState = new BirdState
            {
                ID = BirdEnum.Idling,
            };
            idlingState.Coroutine = HandleIdlingState(idlingState);

            return idlingState;
        }
        BirdState GetHuntingStae()
        {
            huntingState = new BirdState
            {
                ID = BirdEnum.Hunting,
            };
            huntingState.Coroutine = HandleHuntingState(huntingState);

            return huntingState;
        }
        BirdState GetStarvingState()
        {
            starvingState = new BirdState
            {
                ID = BirdEnum.Starving,
            };
            starvingState.Coroutine = HandleStarvingState(starvingState);

            return starvingState;
        }

        BirdState GetDeathState()
        {
            deathState = new BirdState
            {
                ID = BirdEnum.Death,
            };
            deathState.Coroutine = HandleDeathState(deathState);

            return deathState;
        }
        BirdState GetCapturedState()
        {
            capturedState = new BirdState
            {
                ID = BirdEnum.Captured,
            };
            capturedState.Coroutine = HandleCapturedState(capturedState);

            return capturedState;
        }

        protected virtual Func<IEnumerator> HandleIdlingState(BirdState state)
        {
            var positionHandler = transform.GetPositionResolver();
            //var (pidHandler, resetPid) = transform.GetPidHandler(10f, 10f, 11f, -7f, 7f, _rigidbody);
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
            var timeOutHz = sToHz(Range(6, 8));

            IEnumerator idlingHandler()
            {
                var birdConductor = state?.Conductor;
                if (birdConductor is null) yield return null;

                var time = 0;

                //Func<(Func<Vector3>, Action<Vector3>)> selector = () =>
                //{
                //    var num = Range(0, 2);
                //    Func<Vector3> tempPositionHandler = null;
                //    Action<Vector3> tempLateProcessHandler = null;

                //    switch (num)
                //    {
                //        case 0: // tree slot
                //                //var position = positionHandler(treeSlotFinder);
                //                //tempPositionHandler = () => position;
                //                //tempLateProcessHandler = (p) =>
                //                //{
                //                //    if (Math.Round((p - position).sqrMagnitude, 2) <= 0.001
                //                //    && _animator.isActiveAndEnabled)
                //                //    {
                //                //        _rigidbody.velocity = Vector2.zero;
                //                //        _animator.enabled = false;
                //                //    }
                //                //    else
                //                //    {
                //                //        print("fuck");
                //                //    }
                //                //};
                //                //return (tempPositionHandler, tempLateProcessHandler);
                //        default: // random
                //            return (() => positionHandler(randomTargetGenerator), tempLateProcessHandler);
                //    }
                //};

                //var (tempPositionHandler, lateProcessHandler) = selector();

                var hzedOut = sToHz(Range(1.7f, 2.3f));
                var getRandomPosition = new Func<Vector3>(
                    () => positionHandler(randomTargetGenerator))
                    .SampleAt(hzedOut);

                while (true)
                {
                    if (birdConductor.Context.Channel.TryDequeue(out var result))
                    {
                        if (result.key == BirdSignal.GrownStage1)
                        {
                            transform.localScale = new Vector3(
                                transform.localScale.x * 1.3f,
                                transform.localScale.y * 1.3f,
                                transform.localScale.z);
                        }
                        else if (result.key == BirdSignal.GrownStage2)
                        {
                            transform.localScale = new Vector3(
                                transform.localScale.x * 1.3f,
                                transform.localScale.y * 1.3f,
                                transform.localScale.z);
                        }
                        else if (result.key == BirdSignal.EnergyRegen)
                        {
                            var Energytime = Convert.ToInt32(result.value) / _energyConsumePerSecond * Range(0.7f, 1f);
                            timeOutHz = sToHz(Energytime);
                        }
                    }

                    var position = getRandomPosition();
                    pidHandler(position, timeStep);

                    if (_rigidbody.velocity.sqrMagnitude > 3.0f)
                    {
                        _animator.Play("bird_1_fly_fast");
                    }
                    else
                    {
                        _animator.Play("fly");
                    }

                    //lateProcessHandler?.Invoke(transform.position);

                    if (++time >= timeOutHz)
                    {
                        UnityEngine.Debug.Log("Change to hunting");
                        birdConductor.ChangeState(huntingState);
                        break;
                    }

                    yield return new WaitForSeconds(timeStep);
                }
            }
            return idlingHandler;
        }
        protected virtual Func<IEnumerator> HandleHuntingState(BirdState state)
        {
            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var timeOutHz = sToHz(Range(6, 8));
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
                            var energy = c.gameObject.GetComponent<Food>().Energy;
                            _conductor.Context.Channel.Enqueue((BirdSignal.EnergyRegen, energy));

                            birdConductor.ChangeState(idlingState);
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
                    pidHandler(position, timeStep);

                    if (_rigidbody.velocity.sqrMagnitude > 3.0f)
                    {
                        _animator.Play("bird_1_fly_fast");
                    }
                    else
                    {
                        _animator.Play("fly");
                    }

                    if (time++ >= timeOutHz)
                    {
                        _conductor.ChangeState(starvingState);
                        break;
                    }
                }
            }
            return huntingHandler;
        }

        protected virtual Func<IEnumerator> HandleStarvingState(BirdState state)
        {
            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var timeOutHz = sToHz(Range(7, 9));
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

                _animator.Play("bird_1_starving");

                while (true)
                {
                    yield return new WaitForSeconds(timeStep);
                    if (birdConductor.Context.Channel.TryDequeue(out var result))
                    {
                        if (result.key == BirdSignal.FoundFood
                            && result.value is Collider2D c
                            && c != null)
                        {
                            var energy = c.gameObject.GetComponent<Food>().Energy;
                            _conductor.Context.Channel.Enqueue((BirdSignal.EnergyRegen, energy));

                            birdConductor.ChangeState(idlingState);
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

        protected virtual Func<IEnumerator> HandleDeathState(BirdState state)
        {
            var timeStep = 0.1f;
            var fadeOut = 1.0f;
            var fadeOutStep = 0.1f;
            IEnumerator deathHandler()
            {
                var birdConductor = state.Conductor;
                if (birdConductor is null) yield return null;
                //print($"{birdConductor.State.ID}");

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
            return deathHandler;
        }

        protected virtual Func<IEnumerator> HandleCapturedState(BirdState state)
        {
            IEnumerator capturedHandler()
            {
                var birdConductor = state.Conductor;
                if (birdConductor is null) yield return null;
                //print($"{birdConductor.State.ID}");
                _collider.enabled = false;
                enabled = false;
                using var releaser = new Releaser(() =>
                {
                    _collider.enabled = true;
                    enabled = true;
                });
                var bubble = default(Bubble);
                var gotBubble = false;
                var startTime = Time.time;
                while (true)
                {
                    if (birdConductor.Context.Channel.TryDequeue(out var result))
                    {
                        if (result.key == BirdSignal.Captured
                                  && result.value is Bubble b
                                  && b.transform != null)
                        {
                            bubble = b;
                            gotBubble = true;
                        }

                    }
                    if (gotBubble && bubble == null)
                    {
                        birdConductor.ChangeState(starvingState);
                        break;
                    }
                    else if (bubble != null)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, bubble.transform.position, 1f);
                        if (Time.time - startTime > bubble.CaptureTime)
                        {
                            Destroy(bubble.gameObject);
                            birdConductor.ChangeState(deathState);
                            break;
                        }
                    }
                    yield return null;
                }
            }
            return capturedHandler;
        }

        #endregion

        public IEnumerable<BirdState> GetAllBirdStates()
        {
            yield return GetIdlingState();

            yield return GetHuntingStae();

            yield return GetStarvingState();

            yield return GetDeathState();

            yield return GetCapturedState();
        }
    }
}
