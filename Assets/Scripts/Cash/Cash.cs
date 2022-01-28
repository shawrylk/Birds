using Assets.Contracts;
using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts
{
    public class Cash : BaseScript 
    {
        private TextMeshProUGUI _score;
        private Rigidbody2D _rigidbody;
        public int Value = 10;
        private CashManager _cashManager = null;

        private void Awake()
        {
            _score = Global.GameObjects.GetGameObject("Score").GetComponent<TextMeshProUGUI>();
            _rigidbody = GetComponent<Rigidbody2D>();
            _rigidbody.velocity = new Vector2(0, -1f);
            _cashManager = CashManager.Instance;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(Global.BOUNDARY_TAG))
            {
                if (string.Compare(collision.gameObject.name, Global.BOTTOM_BOUNDARY, true) == 0)
                {
                    Destroy(gameObject, 5f);
                }
                else if (string.Compare(collision.gameObject.name, Global.TOP_BOUNDARY, true) == 0)
                {
                    var (isDone, _) = _cashManager.Add(Value);
                    if (isDone)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }

        public int GetValue()
        {
            IEnumerator move()
            {
                while (true)
                {
                    Vector2 direction = _score.transform.position - transform.position;
                    _rigidbody.AddForce(direction.normalized * 40f);
                    yield return new WaitForFixedUpdate();
                }
            }
            StartCoroutine(move());
            return Value;
        }

    }
}
