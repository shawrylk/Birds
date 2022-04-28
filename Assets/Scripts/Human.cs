using Assets.Inputs;
using Assets.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class Human : MonoBehaviour
    {
        private IInput _input;
        [SerializeField] private GameObject _handPrefab;
        private GameObject _handObject;
        private Vector3 _handOrigin = Vector3.zero;
        private void Awake()
        {
            _input = Global.Items[Global.INPUT] as IInput;

            _handObject = Instantiate(
              original: _handPrefab,
              position: transform.position,
              rotation: Quaternion.identity,
              parent: transform);
            _handOrigin = _handObject.transform.position;
            var rigidbody = _handObject.GetComponent<Rigidbody2D>();
            _input.SwipeHandler += (vector) =>
            {
                if (_handOrigin != _handObject.transform.position) return Task.CompletedTask;

                Vector3.RotateTowards(_handObject.transform.rotation.eulerAngles, vector, 1, 1);
                rigidbody.AddForce(vector.normalized * 200f);
                return Task.CompletedTask;
            };
        }
    }
}
