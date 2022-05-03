using Assets.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.LandableEdge
{
    public class FixEdgeColliderPoints : MonoBehaviour
    {
        private EdgeCollider2D _edge;
        private void Awake()
        {
            _edge = GetComponent<EdgeCollider2D>();
            //_edge.points = _edge.points.Select(p => p.ToWorldCoord()).ToArray();
            //_edge.points = _edge.points.Select(p => (Vector2)transform.InverseTransformPoint(p)).ToArray();
            _edge.points = _edge.points.Select(p => p * Global.UnitsPerPixel * Camera.main.orthographicSize).ToArray();
        }
    }
}
