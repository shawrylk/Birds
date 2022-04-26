using Assets.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class AnimalBase : BaseScript
    {
        protected Rigidbody2D _rigidbody = null;
        protected SpriteRenderer _sprite = null;
        protected Animator _animator = null;
        protected Collider2D _collider = null;

        protected virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _collider = GetComponent<Collider2D>();
        }
        protected virtual void Update()
        {
            _sprite.flipX = _rigidbody.velocity.x < 0;
        }

        protected virtual void OnTriggerStay2D(Collider2D collision)
        {
            const float force = 50f;
            if (collision.gameObject.CompareTag(Global.BOUNDARY_TAG))
            {
                if (collision.gameObject.name.ToLower() == Global.BOTTOM_BOUNDARY)
                {
                    _rigidbody.AddForce(new Vector2(1, force * Time.fixedDeltaTime) * Vector2.up, ForceMode2D.Impulse);
                }
                else if (collision.gameObject.name.ToLower() == Global.TOP_BOUNDARY)
                {
                    _rigidbody.AddForce(new Vector2(1, -force * Time.fixedDeltaTime) * Vector2.up, ForceMode2D.Impulse);
                }
                else if (collision.gameObject.name.ToLower() == Global.LEFT_BOUNDARY)
                {
                    _rigidbody.AddForce(new Vector2(force * Time.fixedDeltaTime, 1) * Vector2.right, ForceMode2D.Impulse);
                }
                else if (collision.gameObject.name.ToLower() == Global.RIGHT_BOUNDARY)
                {
                    _rigidbody.AddForce(new Vector2(-force * Time.fixedDeltaTime, 1) * Vector2.right, ForceMode2D.Impulse);
                }
            }
        }
        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(Global.BOUNDARY_TAG))
            {
                if (collision.gameObject.name.ToLower() == Global.BOTTOM_BOUNDARY
                || collision.gameObject.name.ToLower() == Global.TOP_BOUNDARY)
                {
                    _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0);
                }
                else
                {
                    _rigidbody.velocity = new Vector2(0, _rigidbody.velocity.y);
                }
            }
        }
    }
}
