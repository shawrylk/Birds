using Assets.Contracts;
using Assets.Scripts.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Assets.Scripts.Fishes
{

    public class Salmon : AnimalBase, IFood
    {
        [SerializeField] private GameObject _bubble;
        private int _energy = 150;
        public int Energy
        {
            get => _energy;
            private set => _energy = value;
        }
        protected override void Awake()
        {
            base.Awake();
            _rigidbody.AddForce(Vector2.up * 10f);
            StartCoroutine(SpawnBubble());
        }
        private IEnumerator SpawnBubble()
        {
            while (true)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(15, 20));

                var bubble = Instantiate(
                   original: _bubble,
                   position: transform.position,
                   rotation: Quaternion.identity,
                   parent: gameObject.transform);

                bubble.GetComponent<Rigidbody2D>().AddForce(Vector2.up * 80f
                    + Vector2.left * UnityEngine.Random.Range(30, 75)
                    + Vector2.right * UnityEngine.Random.Range(30, 75));
            }
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(Global.BOUNDARY_TAG))
            {
                _rigidbody.velocity = Vector2.zero;
            }
            base.OnTriggerEnter2D(collision);
        }
    }
}
