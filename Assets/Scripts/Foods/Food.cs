using Assets.Contracts;
using Assets.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Food : BaseScript
    {
        private Rigidbody2D _rigidbody;
        public int Price = 10;
        public int UpgradePrice = 1000;
        public int Energy = 200;
        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.velocity = new Vector2(0, -0.5f);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(Global.BOUNDARY_TAG)
                && collision.gameObject.name.ToLower() == Global.BOTTOM_BOUNDARY)
            {
                Destroy(gameObject, 0.1f);
            }
        }
    }
}
