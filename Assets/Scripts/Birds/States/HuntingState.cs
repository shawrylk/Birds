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
    public partial class BirdBase : AnimalBase // Hunting State
    {
        protected BirdState huntingState = null;

        BirdState GetHuntingStae()
        {
            huntingState = new BirdState
            {
                ID = BirdEnum.Hunting,
            };
            huntingState.Coroutine = HandleHuntingState(huntingState);

            return huntingState;
        }
        protected virtual Func<IEnumerator> HandleHuntingState(BirdState state)
        {
            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var timeOutHz = sToHz(Range(8, 10));
            var positionHandler = transform.GetPositionResolver();
            var (pidHandler, resetPid) = PidExtensions.GetPidHandler(options =>
            {
                options.X = (6f, 6f, 6.1f);
                options.Y = (6f, 6f, 6.1f);
                options.ClampX = (-3.4f, 3.4f);
                options.ClampY = (-3.4f, 3.4f);
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

    }
}
