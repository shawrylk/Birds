using Assets.Scripts.Fish;
using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.Scripts.Lock.Guard;
using static UnityEngine.Random;

namespace Assets.Scripts.Birds
{
    public partial class BirdBase : AnimalBase // Landing State
    {
        protected BirdState landingState = null;
        [SerializeField] private List<Transform> land;
        BirdState GetLandingState()
        {
            landingState = new BirdState
            {
                ID = BirdEnum.Captured,
            };

            landingState.Coroutine = HandleLandingState(landingState);

            return landingState;
        }

        protected virtual Func<IEnumerator> HandleLandingState(BirdState state)
        {
            IEnumerator landdingHandler()
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
            return landdingHandler;
        }

    }
}
