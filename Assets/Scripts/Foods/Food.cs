using Assets.Contracts;
using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.Scripts.Lock;

namespace Assets.Scripts
{
    public class Food : BaseScript, IFood
    {
        public const string Name = "Food";
        private Rigidbody2D _rigidbody;
        private SpriteRenderer _sprite;
        private Guard _guard = new Guard();
        private bool _eaten = false;
        public int UpgradePrice = 1000;
        public int Price { set; get; } = 10;
        public int Energy { set; get; } = 200;
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _rigidbody.velocity = new Vector2(0, -0.2f);
            MoveRandom();
        }

        private void MoveRandom()
        {
            IEnumerator moveRandom()
            {
                while (true)
                {
                    _rigidbody.AddForce(Vector2.right * UnityEngine.Random.Range(-3f, 3f));
                    _sprite.flipX = _rigidbody.velocity.x < 0;
                    yield return new WaitForSeconds(0.2f);
                }
            }
            StartCoroutine(moveRandom());
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(Global.BOUNDARY_TAG))
            {
                if (_eaten == false)
                {
                    if (collision.gameObject.name.ToLower() == Global.BOTTOM_BOUNDARY
                        || collision.gameObject.name.ToLower() == Global.LAND_BOUNDARY)
                    {
                        var (gotLck, lck) = _guard.TryGet();
                        if (!gotLck) return;
                        using var _ = lck;
                        Destroy(gameObject, 0.1f);
                        _eaten = true;
                    }
                }
                else
                {
                    _rigidbody.AddForce(new Vector2(_rigidbody.velocity.x * -2, 1) * Vector2.right, ForceMode2D.Impulse);
                }
            }
        }
    }
}
