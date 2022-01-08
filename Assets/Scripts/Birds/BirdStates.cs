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
                    Couroutine = HandleStarvingState()
                };

                return starvingState;
            }

            BirdContextState GetDeathState()
            {
                deathState = new BirdContextState
                {
                    ID = BirdEnum.Death,
                    Couroutine = HandleDeathState()
                };

                return deathState;
            }
            Func<Context, IEnumerator> HandleIdlingState()
            {
                var positionHandler = GetPositionResolverHandler();
                var pidHandler = GetPidHandler();
                var targetFinder = new RandomPathGenerator();
                var timeStep = 0.02f;
                var sToHz = timeStep.GetSToHzHandler();
                var timeOutHz = sToHz(Range(7, 10));
                IEnumerator idlingHandler(Context context)
                {
                    var birdContext = context as BirdContext;
                    if (birdContext is null) yield return null;
                    var time = 1;
                    print($"{birdContext.State.ID}");
                    while (true)
                    {
                        yield return new WaitForSeconds(timeStep);

                        if (birdContext.Data.Channel.TryDequeue(out var result))
                        {
                            // TODO
                        }

                        var position = positionHandler(targetFinder);
                        pidHandler(position, timeStep);
                        if (++time >= timeOutHz)
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
                var timeStep = 0.1f;
                var sToHz = timeStep.GetSToHzHandler();
                var timeOutHz1 = sToHz(Range(5, 7));
                var timeOutHz2 = timeOutHz1 + sToHz(Range(10, 12));
                var positionHandler = GetPositionResolverHandler();
                var pidHandler = GetPidHandler();
                var targetFinder = new TargetFinder();
                IEnumerator huntingHandler(Context context)
                {
                    var birdContext = context as BirdContext;
                    if (birdContext is null) yield return null;
                    var time = 0;
                    var grayScale = 0.0f;
                    print($"{birdContext.State.ID}");
                    while (true)
                    {
                        yield return new WaitForSeconds(timeStep);

                        if (birdContext.Data.Channel.TryDequeue(out var result))
                        {
                            if (result.key == BirdSignal.FoundFood
                                && result.value is Collider2D c)
                            {
                                birdContext.State = idlingState;
                                _sprite.material.SetFloat("_GrayscaleAmount", 0.0f);
                                if (c.gameObject.activeSelf) Destroy(c.gameObject, 0.1f);
                                break;
                            }
                        }

                        var position = positionHandler(targetFinder);
                        pidHandler(position, timeStep);

                        if (++time >= timeOutHz2)
                        {
                            birdContext.State = deathState;
                            break;
                        }
                        else if (time >= timeOutHz1)
                        {
                            if (grayScale < 1) grayScale += 0.03f;
                            _sprite.material.SetFloat("_GrayscaleAmount", grayScale);
                        }
                    }
                }
                return huntingHandler;
            }

            Func<Context, IEnumerator> HandleStarvingState()
            {
                var timeStep = 0.1f;
                IEnumerator starvingHandler(Context context)
                {
                    var birdContext = context as BirdContext;
                    if (birdContext is null) yield return null;
                    print($"{birdContext.State.ID}");

                    while (true)
                    {
                        yield return new WaitForSeconds(timeStep);
                    }
                }
                return starvingHandler;
            }

            Func<Context, IEnumerator> HandleDeathState()
            {
                var timeStep = 0.1f;
                var fadeOut = 1.0f;
                var fadeOutStep = 0.1f;
                IEnumerator deathHandler(Context context)
                {
                    var birdContext = context as BirdContext;
                    if (birdContext is null) yield return null;
                    print($"{birdContext.State.ID}");

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

            yield return GetIdlingState();

            yield return GetHuntingStae();

            yield return GetStarvingState();

            yield return GetDeathState();
        }


    }
}
