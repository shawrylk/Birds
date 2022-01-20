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
    public partial class BirdBase : BaseScript
    {
        #region Local Variables
        protected BirdContextState idlingState = null;
        protected BirdContextState huntingState = null;
        protected BirdContextState starvingState = null;
        protected BirdContextState deathState = null;
        #endregion

        #region Local Functions
        protected virtual BirdContextState GetIdlingState()
        {
            idlingState = new BirdContextState
            {
                ID = BirdEnum.Idling,
                Couroutine = HandleIdlingState()
            };

            return idlingState;
        }
        protected virtual BirdContextState GetHuntingStae()
        {
            huntingState = new BirdContextState
            {
                ID = BirdEnum.Hunting,
                Couroutine = HandleHuntingState()
            };

            return huntingState;
        }
        protected virtual BirdContextState GetStarvingState()
        {
            starvingState = new BirdContextState
            {
                ID = BirdEnum.Starving,
                Couroutine = HandleStarvingState()
            };

            return starvingState;
        }

        protected virtual BirdContextState GetDeathState()
        {
            deathState = new BirdContextState
            {
                ID = BirdEnum.Death,
                Couroutine = HandleDeathState()
            };

            return deathState;
        }
        protected virtual Func<Context, IEnumerator> HandleIdlingState()
        {
            var positionHandler = transform.GetPositionResolverHandler();
            var (pidHandler, resetPid) = transform.GetPidHandler(10f, 10f, 11f, -7f, 7f, _rigidbody);

            //var treeSlotFinder = new TreeFinder();
            //treeSlotFinder.UpdateTargets(Trees);

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

                Func<(Func<Vector3>, Action<Vector3>)> selector = () =>
                {
                    var num = Range(0, 2);
                    Func<Vector3> tempPositionHandler = null;
                    Action<Vector3> tempLateProcessHandler = null;

                    switch (num)
                    {
                        case 0: // tree slot
                                //var position = positionHandler(treeSlotFinder);
                                //tempPositionHandler = () => position;
                                //tempLateProcessHandler = (p) =>
                                //{
                                //    if (Math.Round((p - position).sqrMagnitude, 2) <= 0.001
                                //    && _animator.isActiveAndEnabled)
                                //    {
                                //        _rigidbody.velocity = Vector2.zero;
                                //        _animator.enabled = false;
                                //    }
                                //    else
                                //    {
                                //        print("fuck");
                                //    }
                                //};
                                //return (tempPositionHandler, tempLateProcessHandler);
                        default: // random
                            return (() => positionHandler(randomTargetGenerator), tempLateProcessHandler);
                    }
                };

                var (tempPositionHandler, lateProcessHandler) = selector();

                while (true)
                {
                    if (birdContext.Data.Channel.TryDequeue(out var result))
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
                            var Energytime = Convert.ToInt32(result.value) / EnergyConsumePerSecond;
                            timeOutHz = sToHz(Energytime);
                        }
                    }

                    var position = tempPositionHandler();
                    pidHandler(position, timeStep);

                    if (_rigidbody.velocity.sqrMagnitude > 3.0f)
                    {
                        _animator.Play("bird_1_fly_fast");
                    }
                    else
                    {
                        _animator.Play("fly");
                    }

                    lateProcessHandler?.Invoke(transform.position);

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
        protected virtual Func<Context, IEnumerator> HandleHuntingState()
        {
            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var timeOutHz = sToHz(Range(7, 10));
            var positionHandler = transform.GetPositionResolverHandler();
            var (pidHandler, resetPid) = transform.GetPidHandler(10f, 10f, 11f, -7f, 7f, _rigidbody);
            var targetFinder = new TargetFinder();


            IEnumerator huntingHandler(Context context)
            {
                var birdContext = context as BirdContext;
                if (birdContext is null) yield return null;
                var time = 0;

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
                        _lifeCycle.State = starvingState;
                        break;
                    }
                }
            }
            return huntingHandler;
        }

        protected virtual Func<Context, IEnumerator> HandleStarvingState()
        {
            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var timeOutHz = sToHz(Range(7, 10));
            var positionHandler = transform.GetPositionResolverHandler();
            var (pidHandler, resetPid) = transform.GetPidHandler(10f, 10f, 11f, -9f, 9f, _rigidbody);
            var targetFinder = new TargetFinder();


            IEnumerator starvingHandler(Context context)
            {
                var birdContext = context as BirdContext;
                if (birdContext is null) yield return null;
                var time = 0;
                var grayScale = 0.0f;

                _animator.enabled = true;

                _animator.Play("bird_1_starving");

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

        protected virtual Func<Context, IEnumerator> HandleDeathState()
        {
            var timeStep = 0.1f;
            var fadeOut = 1.0f;
            var fadeOutStep = 0.1f;
            IEnumerator deathHandler(Context context)
            {
                var birdContext = context as BirdContext;
                if (birdContext is null) yield return null;
                //print($"{birdContext.State.ID}");

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
        #endregion

        public IEnumerable<BirdContextState> GetAllBirdStates()
        {
            yield return GetIdlingState();

            yield return GetHuntingStae();

            yield return GetStarvingState();

            yield return GetDeathState();
        }
    }
}
