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
    public partial class BirdBase : AnimalBase // Idling State
    {
        protected BirdState idlingState = null;
        BirdState GetIdlingState()
        {
            idlingState = new BirdState
            {
                ID = BirdEnum.Idling,
            };
            idlingState.Coroutine = HandleIdlingState(idlingState);

            return idlingState;
        }
        protected virtual Func<IEnumerator> HandleIdlingState(BirdState state)
        {
            var positionHandler = transform.GetPositionResolver();
            //var (pidHandler, resetPid) = transform.GetPidHandler(10f, 10f, 11f, -7f, 7f, _rigidbody);
            var (pidHandler, resetPid) = PidExtensions.GetPidHandler(options =>
            {
                options.X = (6f, 6f, 6.1f);
                options.Y = (6f, 6f, 6.1f);
                options.ClampX = (-3.4f, 3.4f);
                options.ClampY = (-3.4f, 3.4f);
                options.Transform = transform;
                options.Rigidbody2D = _rigidbody;
            });

            //var treeSlotFinder = new TreeFinder();
            //treeSlotFinder.UpdateTargets(Trees);

            var randomTargetGenerator = new RandomPathGenerator();
            randomTargetGenerator.UpdateTargets(null);

            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var timeOutHz = sToHz(Range(8, 10));

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
                        birdConductor.ChangeState(huntingState);
                        break;
                    }

                    yield return new WaitForSeconds(timeStep);
                }
            }
            return idlingHandler;
        }


    }
}
