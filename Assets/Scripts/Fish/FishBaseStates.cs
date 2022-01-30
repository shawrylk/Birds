using Assets.Scripts.Birds;
using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Random;

namespace Assets.Scripts.Fishes
{
    public partial class FishBase : AnimalBase
    {
        protected FishContextState huntingStage1State = null;
        protected FishContextState huntingStage2State = null;
        protected FishContextState beingAttackedState = null;
        protected FishContextState deathState = null;
        FishContextState GetHuntingStage1State()
        {
            huntingStage1State = new FishContextState
            {
                ID = FishEnum.HuntingStage1,
                Couroutine = HandleHuntingStage1State()
            };

            return huntingStage1State;
        }
        FishContextState GetHuntingStage2State()
        {
            huntingStage2State = new FishContextState
            {
                ID = FishEnum.HuntingStage2,
                Couroutine = HandleHuntingStage2State()
            };

            return huntingStage2State;
        }

        FishContextState GetBeingAttackedState()
        {
            beingAttackedState = new FishContextState
            {
                ID = FishEnum.BeingAttacked,
                Couroutine = HandleBeingAttackedState()
            };

            return beingAttackedState;
        }

        FishContextState GetDeathState()
        {
            deathState = new FishContextState
            {
                ID = FishEnum.Death,
                Couroutine = HandleDeathState()
            };

            return deathState;
        }
        protected virtual Func<Context, IEnumerator> HandleHuntingStage1State()
        {
            var (move, resetInertia) = PidExtensions.GetPidHandler(options =>
            {
                options.X = (3f, 0f, 3f);
                options.Y = (3f, 0f, 3f);
                options.ClampX = (-3f, 3f);
                options.ClampY = (-3f, 3f);
                options.Transform = transform;
                options.Rigidbody2D = _rigidbody;
            });

            //var randomTargetGenerator = new RandomPathGenerator();
            var targetFinder = new TargetFinder();

            var positionHandler = transform.GetPositionResolver();
            //var randomPositionHandler = new Func<Vector3>(
            //    () => positionHandler(targetFinder));

            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var yieldTimeStep = new WaitForSeconds(timeStep);

            var foodManager = Global.GameObjects.GetGameObject(Global.FOOD_MANAGER_TAG);
            var ateFoodCount = 0;
            IEnumerator huntingHandler(Context context)
            {
                var fishContext = context as FishContext;
                if (fishContext is null) yield return null;

                //var (regenEnergy, runOutEnergy) = EnergyConsumePerSecond.GetEnergyHanlder(sToHz(Range(50, 70)), sToHz);

                var hzedOut = sToHz(Range(1.7f, 2.3f));
                //var getRandomPosition = randomPositionHandler.SampleAt(hzedOut);

                while (true)
                {
                    if (fishContext.Data.Channel.TryDequeue(out var result))
                    {
                        if (result.key == FishSignal.FoundFood
                            && result.value is Collider2D c
                            && c != null)
                        {
                            ateFoodCount++;
                            Destroy(c.gameObject, 0.1f);
                            resetInertia();
                        }
                    }

                    var targets = foodManager
                        .GetComponentsInChildren<Transform>()
                        .Skip(Global.PARENT_TRANSFORM)
                        .ToList();

                    targetFinder.UpdateTargets(targets);

                    var position = positionHandler(targetFinder);
                    move(position, timeStep);

                    if (_foodNames.Count == 1
                        && ateFoodCount >= 5)
                    {
                        transform.localScale = new Vector3(
                            transform.localScale.x * 1.7f,
                            transform.localScale.y * 1.7f,
                            transform.localScale.z);
                        Energy = 200;
                        _foodNames = new List<string> { Pigeon.Name };

                        if (_fishManager.AllFishes.Remove(gameObject))
                        {
                            gameObject.transform.name = FishBase.Name2;
                            _fishManager.AllFishes.Add(gameObject);
                        }

                        fishContext.State = huntingStage2State;
                        break;
                    };

                    yield return yieldTimeStep;
                }
            }
            return huntingHandler;
        }
        protected virtual Func<Context, IEnumerator> HandleHuntingStage2State()
        {
            var (move, resetInertia) = PidExtensions.GetPidHandler(options =>
            {
                options.X = (10f, 7f, 10f);
                options.Y = (10f, 7f, 10f);
                options.ClampX = (-0.7f, 0.7f);
                options.ClampY = (-0.7f, 0.7f);
                options.Transform = transform;
                options.Rigidbody2D = _rigidbody;
            });

            var randomTargetGenerator = new RandomPathGenerator();
            var targetFinder = new TargetFinder();

            var positionHandler = transform.GetPositionResolver();
            //var randomPositionHandler = new Func<Vector3>(
            //    () => positionHandler(targetFinder));

            var timeStep = 0.1f;
            var sToHz = timeStep.GetSToHzHandler();
            var yieldTimeStep = new WaitForSeconds(timeStep);

            var coolDown = 0f;
            IEnumerator huntingHandler(Context context)
            {
                var fishContext = context as FishContext;
                if (fishContext is null) yield return null;

                var (regenEnergy, runOutEnergy) = EnergyConsumePerSecond.GetEnergyHanlder(sToHz(Range(50, 70)), sToHz);

                var hzedOut = sToHz(Range(1.7f, 2.3f));
                //var getRandomPosition = randomPositionHandler.SampleAt(hzedOut);

                while (true)
                {
                    var position = default(Vector3);

                    if (coolDown-- < 0)
                    {
                        if (fishContext.Data.Channel.TryDequeue(out var result))
                        {
                            if (result.key == FishSignal.FoundFood
                                && result.value is Collider2D c
                                && c != null)
                            {
                                coolDown = sToHz(Range(5, 7));
                                Destroy(c.gameObject, 0.1f);
                                resetInertia();
                            }
                        }

                        var listFood = default(List<GameObject>);
                        var targets = default(List<Transform>);

                        _birdManager.AllBirds
                            .TryGetValue(Pigeon.Name, out listFood);

                        targets = listFood
                            ?.Where(g => g != null)
                            ?.Select(g => g.transform)
                            ?.ToList();

                        targetFinder.UpdateTargets(targets);
                        position = positionHandler(targetFinder);
                    }
                    else
                    {
                        position = positionHandler(randomTargetGenerator);
                    }

                    move(position, timeStep);

                    yield return yieldTimeStep;
                }
            }
            return huntingHandler;
        }


        protected virtual Func<Context, IEnumerator> HandleBeingAttackedState()
        {
            IEnumerator beingAttackedHandler(Context context) { yield return null; }
            return beingAttackedHandler;
        }
        protected virtual Func<Context, IEnumerator> HandleDeathState()
        {
            IEnumerator deathHandler(Context context) { yield return null; }
            return deathHandler;
        }
        public IEnumerable<FishContextState> GetAllFishStates()
        {
            yield return GetHuntingStage1State();

            yield return GetHuntingStage2State();

            yield return GetBeingAttackedState();

            yield return GetDeathState();
        }

    }
}
