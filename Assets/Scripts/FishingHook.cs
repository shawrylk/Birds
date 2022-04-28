using Assets.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class FishingHook : MonoBehaviour
    {
        private Vector3 _origin = Vector3.zero;
        private Rigidbody2D _rigidbody2D;
        //[SerializeField] private GameObject _foodPrefabs;
        private Dictionary<string, Transform> _boundaries;
        private FoodManager _foodManager;
        private void Awake()
        {
            _origin = transform.position;
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _boundaries = Global.Items[Global.BOUNDARIES_TAG] as Dictionary<string, Transform>;
            _foodManager = FoodManager.Instance;

        }

        private void Update()
        {
            if (transform.position.y >= _origin.y)
            {
                _rigidbody2D.velocity = Vector2.zero;
                _rigidbody2D.position = _origin;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag(Global.BOUNDARY_TAG))
            {
                if (collision.gameObject.name.ToLower() != Global.BOTTOM_BOUNDARY)
                {
                    _rigidbody2D.velocity = -_rigidbody2D.velocity;
                }
            }
            else if (collision.gameObject.CompareTag("Fish"))
            {
                Destroy(collision.gameObject);
                var top = _boundaries[Global.TOP_BOUNDARY].position;
                _foodManager.SpawnFood(top);
                //var food = Instantiate(
                //    original: _foodPrefabs,
                //    position: top,
                //    rotation: Quaternion.identity);

            }
        }
    }
}
