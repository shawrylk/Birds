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
        public int Price = 100;
        public float EnergyConsumePerSecond = 10;
        public GameObject[] CashPrefabs;

        protected Channel _foodChannel = null;
        protected string _foodTag = Food.Name;

        protected override void Awake()
        {
            base.Awake();
            StartLifeCycleOfBird();
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.name == _foodTag)
            {
                _foodChannel.Enqueue((
                    signal: BirdSignal.FoundFood,
                    data: collision));
            }
            else
            {
                base.OnTriggerEnter2D(collision);
            }
        }
        protected virtual void StartLifeCycleOfBird()
        {
            _lifeCycle = new BirdContext(this);
            _lifeCycle.Run(
                data: new BirdContextData(),
                state: GetAllBirdStates()
                    .ToList()
                    .First(s => s.ID == BirdEnum.Hunting));
            _foodChannel = _lifeCycle.Data.Channel;
        }

    }
}
