using Assets.Scripts.Birds;
using Assets.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.Scripts.Lock;

namespace Assets.Scripts.Fish
{
    public class Bubble : MonoBehaviour
    {
        private Rigidbody2D _rigidbody2D;
        private EnemyManager _enemyManager;
        private const float castThickness = 0.1f;

        private bool _capturedBird = false;
        private Guard _guard = new Guard();
        //private Action<InputContext> _touchHandler = null;
        public float CaptureTime { get; set; } = 5;
        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _enemyManager = Global.GameObjects.GetGameObject(Global.ENEMY_MANAGER_TAG).GetComponent<EnemyManager>();
            //_enemyManager.TouchEvent += _touchHandler = TouchHandler();
        }
        protected void OnDestroy()
        {
            //_enemyManager.TouchEvent -= _touchHandler;
        }

        //private Action<InputContext> TouchHandler()
        //{
        //    int touchCount = 0;
        //    float startTime = 0;
        //    return new Action<InputContext>((input) =>
        //    {
        //        var position = input.ScreenPosition.ToWorldCoord();
        //        var hits = Physics2D.CircleCastAll(position, castThickness, Vector2.zero);
        //        if (hits != null
        //        && hits.Any(h => h.collider.transform == transform))
        //        {
        //            touchCount++;
        //            if (touchCount == 1)
        //            {
        //                startTime = Time.time;
        //                input.Handled = true;
        //            }
        //            else if (touchCount == 2)
        //            {
        //                if (Time.time - startTime < 1f)
        //                {
        //                    Destroy(gameObject);
        //                    input.Handled = true;
        //                }
        //            }
        //            else
        //            {
        //                touchCount = 0;
        //                startTime = 0;
        //            }
        //        }
        //    });
        //}
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (_capturedBird == false)
            {
                if (collision.gameObject.CompareTag(Global.BOUNDARY_TAG))
                {
                    if (collision.gameObject.name.ToLower() == Global.TOP_BOUNDARY)
                    {
                        Destroy(gameObject);
                    }
                }
            }
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_capturedBird == false)
            {
                if (collision.CompareTag(Global.BIRD_TAG))
                {
                    var (gotLck, lck) = _guard.TryGet();
                    if (!gotLck) return;
                    using (var _ = lck)
                    {
                        var script = collision.gameObject.GetComponent<BirdBase>();
                        script.PreemptiveChannel.Enqueue((BirdSignal.Captured, this));
                        _capturedBird = true;
                    }
                }
            }
        }
    }
}
