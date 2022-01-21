using Assets.Contracts;
using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Food : BaseScript, IFood
    {
        public const string Name = "Food";
        private Rigidbody2D _rigidbody;
        private SpriteRenderer _sprite;
        public int UpgradePrice = 1000;
        public int Price { set; get; } = 10;
        public int Energy { set; get; } = 200;
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _rigidbody.velocity = new Vector2(0, -1f);
            MoveRandom();
        }

        private void MoveRandom()
        {
            IEnumerator moveRandom()
            {
                while (true)
                {
                    _rigidbody.AddForce(Vector2.right * UnityEngine.Random.Range(-15f, 15f));
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
                if (collision.gameObject.name.ToLower() == Global.BOTTOM_BOUNDARY)
                {
                    Destroy(gameObject, 0.1f);
                }
                else
                {
                    _rigidbody.AddForce(new Vector2(_rigidbody.velocity.x * -2, 1) * Vector2.right, ForceMode2D.Impulse);
                }
            }
        }
    }
}
