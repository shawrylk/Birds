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
    public partial class BirdBase : AnimalBase // Starving State
    {
        protected BirdState starvingState = null;
        BirdState GetStarvingState()
        {
            starvingState = new BirdState
            {
                ID = BirdEnum.Starving,
            };
            starvingState.Coroutine = HandleStarvingState(starvingState);

            return starvingState;
        }

        protected virtual Func<IEnumerator> HandleStarvingState(BirdState state)
        {
            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var timeOutHz = sToHz(Range(10, 12));
            var positionHandler = transform.GetPositionResolver();
            var (pidHandler, resetPid) = PidExtensions.GetPidHandler(options =>
            {
                options.X = (6f, 6f, 6.1f);
                options.Y = (6f, 6f, 6.1f);
                options.ClampX = (-4.4f, 4.4f);
                options.ClampY = (-4.4f, 4.4f);
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

                //_animator.Play("Flying");

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

                            var state = Range(0, 2) == 1 ? idlingState : landingState;
                            birdConductor.ChangeState(state);
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


    }
}
