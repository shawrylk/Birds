using Assets.Contracts;
using Assets.Scripts;
using Assets.Scripts.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Channel = System.Collections.Concurrent.ConcurrentQueue<(Assets.Scripts.Fishes.FishSignal signal, object data)>;

namespace Assets.Scripts.Fishes
{
    public partial class FishBase : AnimalBase, IFood
    {
        private float _energyConsumePerSecond = 30f;
        private FishManager _fishManager;
        private BirdManager _birdManager;

        public const string Name = "SmallFish";
        public const string Name2 = "MediumFish";
        protected Channel _foodChannel = null;
        protected List<string> _foodNames = new List<string> { Food.Name };
        protected FishConductor _lifeCycle = null;
        private int _energy = 150;
        public int Energy
        {
            get => _energy;
            private set => _energy = value;
        }

        protected override void Awake()
        {
            base.Awake();
            _fishManager = FishManager.Instance;
            _birdManager = BirdManager.Instance;
            StartLifeCycleOfFish();
        }

        protected override void OnTriggerEnter2D(Collider2D collision)
        {
            if (_foodNames.Contains(collision.gameObject.name))
            {
                _foodChannel.Enqueue((
                    signal: FishSignal.FoundFood,
                    data: collision));
            }
            else
            {
                base.OnTriggerEnter2D(collision);
            }
        }
        protected virtual void StartLifeCycleOfFish()
        {
            _lifeCycle = new FishConductor(this);
            _lifeCycle.Run(
                data: new FishContext(),
                state: GetAllFishStates()
                    .ToList()
                    .First(s => s.ID == FishEnum.HuntingStage1));
            _foodChannel = _lifeCycle.Data.Channel;
        }
    }
}
