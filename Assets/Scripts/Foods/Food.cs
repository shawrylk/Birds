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

        private void FixedUpdate()
        {
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y - 0.6f * Time.deltaTime,
                transform.position.z);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(Global.BOUNDARY_TAG))
            {
                Destroy(gameObject, 0.1f);
            }
        }
    }
}
