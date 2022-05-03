using Assets.Inputs;
using Assets.Scripts.Birds;
using Assets.Scripts.Utilities;
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

        [SerializeField] private Transform _startPoint;
        [SerializeField] private Transform _endPoint;

        private IInput _input;
        private LineRenderer _lineRenderer;
        private RopeSegment[] _ropeSegments;
        private float _ropeSegLen;
        /// <summary>
        /// Smaller means more tension
        /// </summary>
        private float _ropeSegLenPercent = 0.6f;
        /// <summary>
        /// Default is 0.5f
        /// </summary>
        private float _tensionForceMultiplier = 0.8f;
        private int _segmentCount = 10;
        private float _lineWidth = 0.02f;

        //Sling shot 
        private bool _moveToMouse = false;
        private Vector3 _mousePositionWorld;
        private int _indexMousePos;
        private Vector3[] _positions = null;

        // Use this for initialization
        void Awake()
        {
            _input = Global.Items[Global.INPUT] as IInput;
            _input.DragStartHandler += HandleSlingShot;
            _input.DragEndHandler += (_) =>
            {
                _moveToMouse = false;
                return Task.FromResult(false);
            };
            _lineRenderer = GetComponent<LineRenderer>();
            // Because final point is not include, its line is not draw either
            _positions = new Vector3[_segmentCount + 1];
            _ropeSegments = new RopeSegment[_segmentCount];
            _ropeSegLen = Vector2.Distance(_endPoint.position, _startPoint.position) / _segmentCount * _ropeSegLenPercent;
            // Draw a straight line from top to bottom
            var ropeStartPoint = _startPoint.position;
            for (int i = 0; i < _segmentCount; i++)
            {
                _ropeSegments[i] = new RopeSegment(ropeStartPoint);
                ropeStartPoint.y -= _ropeSegLen;
            }

            //StartCoroutine(CompressBirdCollisions());
        }

        private Task<bool> HandleSlingShot(Vector2 startPoint)
        {
            var ratio = (startPoint.x - _startPoint.position.x) / (_endPoint.position.x - _startPoint.position.x);
            var yDistance = _startPoint.position.y - startPoint.y;
            if (ratio > 0 && ratio < 1 && yDistance < 0.4f)
            {
                _indexMousePos = (int)(_segmentCount * ratio);
                var yOffset = Mathf.Abs(startPoint.y - _ropeSegments[_indexMousePos].posNow.y);
                if (yOffset < 0.4f)
                {
                    _moveToMouse = true;
                    _mousePositionWorld = startPoint;
                    return Task.FromResult(true);
                }
            }
            _moveToMouse = false;
            return Task.FromResult(false);
        }

        void Update()
        {
            //_moveToMouse = false;
            //if (Input.GetMouseButtonDown(0))
            //{
            //    moveToMouse = true;
            //}
            //else if (Input.GetMouseButtonUp(0))
            //{
            //    moveToMouse = false;
            //}
            DrawRope();

            //Vector3 screenMousePos = Input.mousePosition;
            //float xStart = StartPoint.position.x;
            //float xEnd = EndPoint.position.x;
            //mousePositionWorld = Camera.main.ScreenToWorldPoint(new Vector3(screenMousePos.x, screenMousePos.y, 10));
            //float currX = mousePositionWorld.x;

            //float ratio = (currX - xStart) / (xEnd - xStart);
            //if (ratio > 0)
            //{
            //    indexMousePos = (int)(segmentLength * ratio);
            //}
        }

        private void FixedUpdate()
        {
            Simulate();
        }

        private void Simulate()
        {
            // SIMULATION
            Vector2 forceGravity = new Vector2(0f, -0.2f);

            for (int i = 1; i < _segmentCount; i++)
            {
                RopeSegment firstSegment = _ropeSegments[i];
                Vector2 posDelta = firstSegment.posNow - firstSegment.posOld;
                firstSegment.posOld = firstSegment.posNow;
                firstSegment.posNow += posDelta;
                firstSegment.posNow += forceGravity * Time.fixedDeltaTime;
                _ropeSegments[i] = firstSegment;
            }

            //CONSTRAINTS
            for (int i = 0; i < 2; i++)
            {
                ApplyConstraint();
            }
        }

        private void ApplyConstraint()
        {
            //Constrant to First Point 
            RopeSegment firstSegment = _ropeSegments[0];
            firstSegment.posNow = _startPoint.position;
            _ropeSegments[0] = firstSegment;


            //Constrant to Second Point 
            RopeSegment endSegment = _ropeSegments[_ropeSegments.Length - 1];
            endSegment.posNow = _endPoint.position;
            _ropeSegments[_ropeSegments.Length - 1] = endSegment;

            for (int i = 0; i < _segmentCount - 1; i++)
            {
                var changeVector = _ropeSegments[i].posNow - _ropeSegments[i + 1].posNow;
                float error = changeVector.magnitude - _ropeSegLen;

                Vector2 changeAmount = changeVector.normalized * error;

                if (i != 0 && i != _segmentCount - 1)
                {
                    _ropeSegments[i].posNow -= changeAmount * _tensionForceMultiplier;
                    _ropeSegments[i + 1].posNow += changeAmount * _tensionForceMultiplier;
                }
                else
                {
                    _ropeSegments[i + 1].posNow += changeAmount;
                }

                if (_moveToMouse && _indexMousePos > 0 && _indexMousePos < _segmentCount - 1 && i == _indexMousePos)
                {
                    _ropeSegments[i].posNow = new Vector2(_mousePositionWorld.x, _mousePositionWorld.y);
                    _ropeSegments[i + 1].posNow = new Vector2(_mousePositionWorld.x, _mousePositionWorld.y);

                    _collisions.ForEach(c =>
                    {
                        if (c != null && c.gameObject != null)
                        {
                            c.gameObject
                              .GetComponent<BirdBase>()
                              ?.CooperativeChannel
                              ?.Enqueue((BirdSignal.Fly, null));
                        }
                    });

                    _collisions.Clear();
                }
                if (_collisions.Count > 0)
                {
                    foreach (var col in _collisions)
                    {
                        if (col == null || col.gameObject == null) continue;
                        var ratio = (col.position.x - _startPoint.position.x) / (_endPoint.transform.position.x - _startPoint.transform.position.x);
                        if (ratio > 0)
                        {
                            var index = (int)(_segmentCount * ratio);
                            if (i == index)
                            {
                                var garavity = new Vector2(0, -0.06f);
                                _ropeSegments[i].posNow += garavity * Time.fixedDeltaTime;
                                _ropeSegments[i + 1].posNow += garavity * Time.fixedDeltaTime;

                            }
                        }
                    }
                }
            }
        }

        //private IEnumerator CompressBirdCollisions()
        //{
        //    var time = new WaitForSeconds(60);
        //    while (true)
        //    {
        //        yield return time;
        //        //using var _ = _collisions.GetLock();
        //        var count = _collisions.Count;
        //        if (count > 0)
        //        {
        //            _collisions = _collisions.Where(c => c != null && c.gameObject != null).ToList();
        //            count = count - _collisions.Count;
        //            Debug.Log($"Compress {count} bird(s), remain {_collisions.Count} bird(s)");
        //        }
        //    }
        //}
        private void DrawRope()
        {
            float lineWidth = _lineWidth;
            _lineRenderer.startWidth = lineWidth;
            _lineRenderer.endWidth = lineWidth;

            for (int i = 0; i < _segmentCount; i++)
            {
                _positions[i] = _ropeSegments[i].posNow;
            }

            _positions[_segmentCount] = _endPoint.position;
            _positions[_segmentCount].z = 0;

            _lineRenderer.positionCount = _positions.Length;
            _lineRenderer.SetPositions(_positions);
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
            if (collision.gameObject == null) yield return null;
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
                posNow = pos;
                posOld = pos;
            }
        }
    }
}
