using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Rope
{
    public class RopeSlingShot : MonoBehaviour
    {

        public Transform StartPoint;
        public Transform EndPoint;

        private LineRenderer lineRenderer;
        private RopeSegment[] ropeSegments;
        private float ropeSegLen;
        /// <summary>
        /// Smaller means more tension
        /// </summary>
        private float ropeSegLenPercent = 0.7f;
        /// <summary>
        /// Default is 0.5f
        /// </summary>
        private float tensionForceMultiplier = 0.9f;
        private int segmentLength = 20;
        private float lineWidth = 0.02f;

        //Sling shot 
        private bool moveToMouse = false;
        private Vector3 mousePositionWorld;
        private int indexMousePos;

        // Use this for initialization
        void Start()
        {
            positions = new Vector3[segmentLength + 1];
            this.lineRenderer = this.GetComponent<LineRenderer>();
            Vector3 ropeStartPoint = StartPoint.position;
            ropeSegments = new RopeSegment[segmentLength];
            ropeSegLen = (EndPoint.position - StartPoint.position).magnitude / segmentLength * ropeSegLenPercent;
            for (int i = 0; i < segmentLength; i++)
            {
                this.ropeSegments[i] = new RopeSegment(ropeStartPoint);
                ropeStartPoint.y -= ropeSegLen;
            }
        }
        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                this.moveToMouse = true;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                this.moveToMouse = false;
            }
            this.DrawRope();

            Vector3 screenMousePos = Input.mousePosition;
            float xStart = StartPoint.position.x;
            float xEnd = EndPoint.position.x;
            this.mousePositionWorld = Camera.main.ScreenToWorldPoint(new Vector3(screenMousePos.x, screenMousePos.y, 10));
            float currX = this.mousePositionWorld.x;

            float ratio = (currX - xStart) / (xEnd - xStart);
            if (ratio > 0)
            {
                this.indexMousePos = (int)(this.segmentLength * ratio);
            }
        }

        private void FixedUpdate()
        {
            this.Simulate();
        }

        private void Simulate()
        {
            // SIMULATION
            Vector2 forceGravity = new Vector2(0f, -1.2f);

            for (int i = 1; i < this.segmentLength; i++)
            {
                RopeSegment firstSegment = this.ropeSegments[i];
                Vector2 posDelta = firstSegment.posNow - firstSegment.posOld;
                firstSegment.posOld = firstSegment.posNow;
                firstSegment.posNow += posDelta;
                firstSegment.posNow += forceGravity * Time.fixedDeltaTime;
                this.ropeSegments[i] = firstSegment;
            }

            //CONSTRAINTS
            for (int i = 0; i < 10; i++)
            {
                this.ApplyConstraint();
            }
        }

        private void ApplyConstraint()
        {
            //Constrant to First Point 
            RopeSegment firstSegment = this.ropeSegments[0];
            firstSegment.posNow = this.StartPoint.position;
            this.ropeSegments[0] = firstSegment;


            //Constrant to Second Point 
            RopeSegment endSegment = this.ropeSegments[this.ropeSegments.Length - 1];
            endSegment.posNow = this.EndPoint.position;
            this.ropeSegments[this.ropeSegments.Length - 1] = endSegment;

            for (int i = 0; i < this.segmentLength - 1; i++)
            {
                var changeVector = this.ropeSegments[i].posNow - this.ropeSegments[i + 1].posNow;
                float error = changeVector.magnitude - this.ropeSegLen;

                Vector2 changeAmount = changeVector.normalized * error;

                if (i != 0 && i != this.segmentLength - 1)
                {
                    this.ropeSegments[i].posNow -= changeAmount * tensionForceMultiplier;
                    this.ropeSegments[i + 1].posNow += changeAmount * tensionForceMultiplier;
                }
                else
                {
                    this.ropeSegments[i + 1].posNow += changeAmount;
                }

                if (this.moveToMouse && indexMousePos > 0 && indexMousePos < this.segmentLength - 1 && i == indexMousePos)
                {
                    this.ropeSegments[i].posNow = new Vector2(this.mousePositionWorld.x, this.mousePositionWorld.y);
                    this.ropeSegments[i + 1].posNow = new Vector2(this.mousePositionWorld.x, this.mousePositionWorld.y);
                    _collisions.Clear();
                }
                if (_collisions.Count > 0)
                {
                    foreach (var col in _collisions.ToList())
                    {
                        var ratio = (col.position.x - StartPoint.position.x) / (EndPoint.transform.position.x - StartPoint.transform.position.x);
                        if (ratio > 0)
                        {
                            var index = (int)(this.segmentLength * ratio);
                            if (i == index)
                            {
                                var garavity = new Vector2(0, -1f);
                                //this.ropeSegments[i].posNow = new Vector2(col.position.x, col.position.y - 0.45f);
                                //this.ropeSegments[i + 1].posNow = new Vector2(col.position.x, col.position.y - 0.45f);
                                this.ropeSegments[i].posNow += garavity * Time.fixedDeltaTime;
                                this.ropeSegments[i + 1].posNow += garavity * Time.fixedDeltaTime;

                            }
                        }
                    }
                }
            }
        }
        private Vector3[] positions = null;
        private void DrawRope()
        {
            float lineWidth = this.lineWidth;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;

            for (int i = 0; i < this.segmentLength; i++)
            {
                positions[i] = this.ropeSegments[i].posNow;
            }

            positions[segmentLength] = EndPoint.position;

            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
        }

        private List<Transform> _collisions = new List<Transform>();
        private List<Transform> _toRemove = new List<Transform>();
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!_collisions.Contains(collision.gameObject.transform))
            {
                //Debug.Log("Add " + collision.gameObject.name);
                _collisions.Add(collision.gameObject.transform);
            }
            else
            {
                //Debug.Log("Stop removing " + collision.gameObject.name);
                _toRemove.Remove(collision.gameObject.transform);
            }
        }

        private IEnumerator OnCollisionExit2D(Collision2D collision)
        {
            _toRemove.Add(collision.gameObject.transform);
            yield return new WaitForSeconds(0.1f);
            if (_toRemove.Contains(collision.gameObject.transform))
            {
                _collisions.Remove(collision.gameObject.transform);
                _toRemove.Remove(collision.gameObject.transform);
                //Debug.Log("Remove " + collision.gameObject.name);
            }
        }

        public struct RopeSegment
        {
            public Vector2 posNow { get; set; }
            public Vector2 posOld { get; set; }

            public RopeSegment(Vector2 pos)
            {
                this.posNow = pos;
                this.posOld = pos;
            }
        }
    }
}
