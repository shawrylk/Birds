using Assets.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Rope
{
    public class RopeColliders : MonoBehaviour
    {
        private LineRenderer _rope;
        private EdgeCollider2D _edgeCollider;

        Vector2[] points2 = null;

        private void Awake()
        {
            _edgeCollider = gameObject.GetComponent<EdgeCollider2D>();
            _rope = gameObject.GetComponent<LineRenderer>();

            getNewPositions();

            _edgeCollider.points = points2;
        }

        private void FixedUpdate()
        {
            if (_rope.positionCount > 2
                && points2 is null)
            {
                points2 = new Vector2[_rope.positionCount];
            }

            getNewPositions();

            _edgeCollider.points = points2;
        }

        void getNewPositions()
        {
            if (points2 is null) return;
            for (int i = 0; i < _rope.positionCount; i++)
            {
                points2[i] = Vector2.Lerp(points2[i], transform.InverseTransformPoint(_rope.GetPosition(i)), 2f);
            }
        }
    }
}
