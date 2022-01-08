using Assets.Contracts;
using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Birds
{
    public partial class Bird : BaseScript
    {
        public IEnumerable<BirdContextState> GetAllBirdStates()
        {
            #region Local Variables
            BirdContextState idlingState = null;
            BirdContextState huntingState = null;
            BirdContextState starvingState = null;
            BirdContextState deathState = null;
            #endregion
            #region Local Functions
            BirdContextState GetIdlingState()
            {
                idlingState = new BirdContextState
                {
                    ID = BirdEnum.Idling,
                    Couroutine = HandleIdlingState()
                };

                return idlingState;
            }
            BirdContextState GetHuntingStae()
            {
                huntingState = new BirdContextState
                {
                    ID = BirdEnum.Hunting,
                    Couroutine = HandleHuntingState()
                };

                return huntingState;
            }
            BirdContextState GetStarvingState()
            {
                starvingState = new BirdContextState
                {
                    ID = BirdEnum.Starving,
                    Couroutine = HandleStarvingState
                };

                return starvingState;
            }

            BirdContextState GetDeathState()
            {
                deathState = new BirdContextState
                {
                    ID = BirdEnum.Death,
                    Couroutine = HandleDeathState
                };

                return deathState;
            }
            Func<Context, IEnumerator> HandleIdlingState()
            {
                var positionHandler = GetPositionResolverHandler();
                var pidHandler = GetPidHandler();
                var targetFinder = new RandomPathGenerator();
                var timeStep = 0.1f;
                IEnumerator idlingHandler(Context context)
                {
                    var birdContext = context as BirdContext;
                    if (birdContext is null) yield return null;
                    var time = 0;
                    while (true)
                    {
                        yield return new WaitForSeconds(timeStep);

                        if (birdContext.Data.Channel.TryDequeue(out var result))
                        {
                            // TODO
                        }

                        var position = positionHandler(targetFinder);
                        pidHandler(position, timeStep);
                        if (++time == 150)
                        {
                            birdContext.State = huntingState;
                            break;
                        }
                    }
                }
                return idlingHandler;
            }
            Func<Context, IEnumerator> HandleHuntingState()
            {
                var positionHandler = GetPositionResolverHandler();
                var pidHandler = GetPidHandler();
                var targetFinder = new TargetFinder();
                var timeStep = 0.1f;
                IEnumerator huntingHandler(Context context)
                {
                    var birdContext = context as BirdContext;
                    if (birdContext is null) yield return null;
                    var time = 0;
                    while (true)
                    {
                        yield return new WaitForSeconds(timeStep);

                        if (birdContext.Data.Channel.TryDequeue(out var result))
                        {
                            switch (result.key)
                            {
                                case BirdSignal s when s == BirdSignal.AteFood:
                                    if (result.value is Collider2D c)
                                    {
                                        Destroy(c, 0.1f);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        var position = positionHandler(targetFinder);
                        pidHandler(position, timeStep);
                        if (++time == 150)
                        {
                            context.State = idlingState;
                            break;
                        }
                    }
                }
                return huntingHandler;
            }

            IEnumerator HandleStarvingState(Context context)
            {
                yield return null;
            }
            IEnumerator HandleDeathState(Context context)
            {
                yield return null;
            }
            #endregion

            yield return GetIdlingState();

            yield return GetHuntingStae();

            yield return GetStarvingState();

            yield return GetDeathState();
        }
    }
}
