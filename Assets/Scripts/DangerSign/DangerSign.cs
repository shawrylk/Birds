using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.DangerSign
{
    public class DangerSign : BaseScript
    {
        //private Animator _animator = null;
        //private Collider2D _collider = null;
        //private Rigidbody2D _rigidbody;
        private SpriteRenderer _sprite = null;

        private void Awake()
        {
            //_rigidbody = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            //_animator = GetComponent<Animator>();
            //_collider = GetComponent<Collider2D>();
            StartWarningCoroutine();
        }

        public void StartWarningCoroutine(float timeOut = 3.0f)
        {
            var timeStep = 0.02f;
            var sToHz = timeStep.GetSToHzHandler();
            var totalHz = sToHz(timeOut);
            var fadeOut = 1.0f;
            var fadeOutStep = 0.1f;
            var hz = 0;

            IEnumerator warningHandler()
            {
                //_animator.enabled = false;
                //_collider.enabled = false;
                //_rigidbody.gravityScale = 1;

                while (true)
                {
                    while (fadeOut > 0)
                    {
                        hz++;

                        _sprite.color = new Color(
                            _sprite.color.r,
                            _sprite.color.g,
                            _sprite.color.b,
                            fadeOut -= fadeOutStep);

                        yield return new WaitForSeconds(timeStep);
                    }

                    while (fadeOut < 1.0f)
                    {
                        hz++;

                        _sprite.color = new Color(
                            _sprite.color.r,
                            _sprite.color.g,
                            _sprite.color.b,
                            fadeOut += fadeOutStep);

                        yield return new WaitForSeconds(timeStep);
                    }

                    if (hz++ >= totalHz)
                    {
                        break;
                    }
                }

                Destroy(gameObject);
            }

            StartCoroutine(warningHandler());
        }
    }
}
