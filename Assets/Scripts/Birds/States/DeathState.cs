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
    public partial class BirdBase : AnimalBase // Death State
    {
        protected BirdState deathState = null;
        BirdState GetDeathState()
        {
            deathState = new BirdState
            {
                ID = BirdEnum.Death,
            };
            deathState.Coroutine = HandleDeathState(deathState);

            return deathState;
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
    }
}
