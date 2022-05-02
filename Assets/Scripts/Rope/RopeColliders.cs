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
        LineRenderer rope;
        EdgeCollider2D edgeCollider;

        Vector2[] points2 = null;

        private void Start()
        {
            edgeCollider = this.gameObject.AddComponent<EdgeCollider2D>();
            edgeCollider.transform.position = Vector3.zero;
            //edgeCollider.edgeRadius = 0.1f;
            rope = this.gameObject.GetComponent<LineRenderer>();

            getNewPositions();

            edgeCollider.points = points2;
        }

        private void FixedUpdate()
        {
            if (rope.positionCount > 2
                && points2 is null)
            {
                points2 = new Vector2[rope.positionCount];
            }

            getNewPositions();

            edgeCollider.points = points2;
        }

        void getNewPositions()
        {
            if (points2 is null) return;
            for (int i = 0; i < rope.positionCount; i++)
            {
                points2[i] = Vector2.Lerp(points2[i], rope.GetPosition(i), 0.2f);
            }
        }
    }
}
