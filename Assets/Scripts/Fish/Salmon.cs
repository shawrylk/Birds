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
            _rigidbody.AddForce(Vector2.up * 100f
                + Vector2.right * Random.Range(0, 100f)
                + Vector2.left * Random.Range(0, 100f));
            //StartCoroutine(SpawnBubble());
            System.Func<Vector2> isFall()
            {
                var oldPosition = Vector3.zero;
                return () =>
                {
                    var vector = transform.position - oldPosition;
                    oldPosition = transform.position;
                    var dot = Vector2.Dot(vector, Vector2.up);
                    //Debug.Log($"vector = {vector} - dot = {dot}");
                    if (dot > 0f)
                    {
                        return Vector2.up;
                    }
                    return Vector2.down;
                };
            }
            _isFall = _isFall ?? isFall();
        }

        private System.Func<Vector2> _isFall = null;
        protected override void Update()
        {
            var isfall = _isFall.Invoke();
            _animator.SetBool("Falling", isfall.y < 0);
            _animator.SetBool("Jumping", isfall.y > 0);
            base.Update();
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
        protected override void OnTriggerStay2D(Collider2D collision)
        {
            const float force = 1f;
            if (collision.gameObject.CompareTag(Global.BOUNDARY_TAG))
            {
                //if (collision.gameObject.name.ToLower() == Global.BOTTOM_BOUNDARY)
                //{
                //    _rigidbody.AddForce(new Vector2(1, force * Time.fixedDeltaTime) * Vector2.up, ForceMode2D.Impulse);
                //}
                if (collision.gameObject.name.ToLower() == Global.TOP_BOUNDARY)
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
        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(Global.BOUNDARY_TAG))
            {
                //if (collision.gameObject.name.ToLower() == Global.BOTTOM2_BOUNDARY)
                //{
                //    Destroy(gameObject);
                //}
            }
        }
    }
}
