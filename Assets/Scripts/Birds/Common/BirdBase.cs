using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Random;
using Channel = System.Collections.Concurrent.ConcurrentQueue<(Assets.Scripts.Birds.BirdSignal signal, object data)>;

namespace Assets.Scripts.Birds
{
    public partial class BirdBase : AnimalBase
    {
        [SerializeField] public int Price = 100;
        [SerializeField] protected float _energyConsumePerSecond = 30;
        [SerializeField] protected GameObject[] _cashPrefabs;
        protected BirdConductor _conductor = null;
        public List<BirdState> ListStates;


        /// <summary>
        /// Uses by states
        /// </summary>
        public Channel CooperativeChannel = null;

        /// <summary>
        /// Uses by state conductor
        /// </summary>
        public Channel PreemptiveChannel = null;
        protected List<string> _foodNames = new List<string> { Food.Name };
        protected override void Awake()
        {
            PreemptiveChannel = new Channel();
            base.Awake();
            StartConductor();
        }

        protected override void Update()
        {
            if (PreemptiveChannel.TryDequeue(out var result))
            {
                if (result.signal == BirdSignal.Captured)
                {
                    _conductor.ChangeState(
                        ListStates.FirstOrDefault(s => s.ID == BirdEnum.Captured),
                        isForce: true);
                    CooperativeChannel.Enqueue((
                        signal: BirdSignal.Captured,
                        data: result.data));
                }
            }
            base.Update();
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (_foodNames.Contains(collision.gameObject.name))
            {
                CooperativeChannel.Enqueue((
                    signal: BirdSignal.FoundFood,
                    data: collision));
            }
            else
            {
                base.OnTriggerEnter2D(collision);
            }
        }

        protected override void OnTriggerStay2D(Collider2D collision)
        {
            const float force = 5f;
            if (collision.gameObject.CompareTag(Global.BOUNDARY_TAG))
            {
                if (collision.gameObject.name.ToLower() == Global.LAND_BOUNDARY)
                {
                    _rigidbody.AddForce(new Vector2(1, force * Time.fixedDeltaTime) * Vector2.up, ForceMode2D.Impulse);
                }
            }
            base.OnTriggerStay2D(collision);
        }

        public IEnumerable<BirdState> GetAllBirdStates()
        {
            yield return GetIdlingState();

            yield return GetHuntingStae();

            yield return GetStarvingState();

            yield return GetDeathState();

            yield return GetCapturedState();

            yield return GetLandingState();
        }

        protected virtual void StartConductor()
        {
            ListStates = GetAllBirdStates().ToList();
            _conductor = new BirdConductor(this);
            var @enum = Range(0, 2) == 1 ? BirdEnum.Idling : BirdEnum.Landing;
            _conductor.Run(
                data: new BirdContext(),
                state: ListStates
                    .First(s => s.ID == @enum));
            CooperativeChannel = _conductor.Context.Channel;
        }

    }
}
