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
        protected static EdgeCollider2D[] _edgeColliders = null;

        BirdState GetLandingState()
        {
            landingState = new BirdState
            {
                ID = BirdEnum.Landing,
            };

            landingState.Coroutine = HandleLandingState(landingState);

            return landingState;
        }

        protected virtual Func<IEnumerator> HandleLandingState(BirdState state)
        {
            IEnumerator landdingHandler()
            {
                yield return new WaitForSeconds(1);

                // Should refactor this code
                // Get edge collider for each bird is perfomant
                var branches = GameObject.FindGameObjectsWithTag(Global.TREE_TAG)
                    ?.SelectMany(o => o?.transform?.GetComponentsInChildren<EdgeCollider2D>());

                var house = GameObject.FindGameObjectsWithTag(Global.HOUSE_TAG)
                    .Select(o => o?.GetComponent<EdgeCollider2D>());

                var electricLines = GameObject.FindGameObjectsWithTag(Global.ELECTRIC_LINE_TAG)
                                    ?.Select(o => o?.GetComponent<EdgeCollider2D>());

                _edgeColliders = _edgeColliders ?? branches.Concat(house).Concat(electricLines)
                                    ?.ToArray();

                var birdConductor = state.Conductor;
                if (birdConductor is null) yield return null;

                var timeStep = 0.1f;
                var sToHz = timeStep.GetSToHzHandler();
                var timeOutHz = sToHz(Range(8, 10));
                var positionHandler = transform.GetPositionResolver();
                var (pidHandler, resetPid) = PidExtensions.GetPidHandler(options =>
                {
                    options.X = (9f, 9f, 4.1f);
                    options.Y = (9f, 9f, 4.1f);
                    options.ClampX = (-7.4f, 7.4f);
                    options.ClampY = (-7.4f, 7.4f);
                    options.Transform = transform;
                    options.Rigidbody2D = _rigidbody;
                });

                var randomEdge = _edgeColliders[Range(0, _edgeColliders.Length)];
                var nearestPoint = randomEdge
                    .points[Range(0, randomEdge.pointCount)]
                    //.Select(point => ((point - (Vector2)transform.position).sqrMagnitude, point))
                    //.OrderBy(t => t.sqrMagnitude)
                    //.First()
                    //.point
                    .ToUnit(); // Need to convert to game unit
                // Above edge to be able to land
                nearestPoint += new Vector2(0f, 0.15f);

                var landingBird = transform.GetChild(0).GetComponent<Collider2D>();
                using var releaser = new Releaser(() =>
                {
                    landingBird.enabled = false;
                    _animator.SetBool("isLanding", false);
                    _rigidbody.gravityScale = 0;
                    //_collider.isTrigger = true;
                    ////Physics2D.IgnoreLayerCollision(LayerMask.GetMask("Birds"), LayerMask.GetMask("LandableEdge"), true);
                    //Physics2D.IgnoreCollision(_collider, _edgeColliders[0], true);
                    //Physics2D.IgnoreCollision(_collider, _edgeColliders[1], true);
                });

                var time = 0;
                var isPlaying = false;
                while (true)
                {
                    yield return new WaitForSeconds(timeStep);

                    Debug.Log($"nearest = {nearestPoint}, position = {transform.position}, y ={ transform.position.y - nearestPoint.y}, distance = {Vector2.Distance(transform.position, nearestPoint)}");

                    if (!isPlaying
                        && transform.position.y - nearestPoint.y > 0.05f
                        && Vector2.Distance(transform.position, nearestPoint) < 0.5f)
                    {
                        isPlaying = true;
                        //Debug.Log($"{nearestPoint} - {(Vector2)transform.position}");
                        //_animator.Play("Landing");
                        _animator.SetBool("isLanding", true);
                        resetPid();
                        _rigidbody.velocity = Vector2.zero;
                        landingBird.enabled = true;
                        _rigidbody.gravityScale = 0.1f;
                        //_collider.isTrigger = false;
                        ////Physics2D.IgnoreLayerCollision(LayerMask.GetMask("Birds"), LayerMask.GetMask("LandableEdge"), true);
                        //Physics2D.IgnoreCollision(_collider, _edgeColliders[0], false);
                        //Physics2D.IgnoreCollision(_collider, _edgeColliders[1], false);
                    }
                    else
                    {
                        pidHandler(nearestPoint, timeStep);
                    }

                    if (time++ >= timeOutHz)
                    {
                        _conductor.ChangeState(huntingState);
                        break;
                    }
                    if (birdConductor.Context.Channel.TryDequeue(out var result))
                    {
                        if (result.key == BirdSignal.Fly)
                        {
                            _conductor.ChangeState(huntingState);
                            break;
                        }
                    }
                }
            }
            return landdingHandler;
        }
    }
}
