using Assets.Contracts;
using Assets.Scripts;
using Assets.Scripts.Birds;
using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Random;

namespace Assets.Scripts.Birds
{
    public partial class Pigeon : BirdBase, IFood
    {
        public const string Name = "Pigeon";
        public int Energy => 150;
        private BirdManager _birdManager;

        //public Transform[] Trees;
        protected override void Awake()
        {
            _birdManager = BirdManager.Instance;
            base.Awake();
            ProduceCash();
        }

        protected override void OnDestroy()
        {
            _birdManager.AllBirds.Remove(gameObject);
            base.OnDestroy();
        }
        private void ProduceCash()
        {
            IEnumerator handleHzedOut(int index)
            {
                bool done = false;
                switch (index)
                {
                    case 0:
                        bool stateChanged1((BirdContextState oldState, BirdContextState newState) states)
                        {
                            if (states.newState.ID == BirdEnum.Idling)
                            {
                                _lifeCycle.Data.Channel.Enqueue((BirdSignal.GrownStage1, null));
                                EnergyConsumePerSecond *= 1.5f;
                                _lifeCycle.OnStateChanged -= stateChanged1;
                                done = true;
                            }
                            return false;
                        }
                        _lifeCycle.OnStateChanged += stateChanged1;
                        yield return new WaitUntil(() => done);
                        done = false;
                        if (_birdManager.AllBirds.Remove(gameObject))
                        {
                            gameObject.transform.name = "Pigeon2";
                            _birdManager.AllBirds.Add(gameObject);
                        }
                        break;
                    case 1:
                        bool stateChanged2((BirdContextState oldState, BirdContextState newState) states)
                        {
                            if (states.newState.ID == BirdEnum.Idling)
                            {
                                _lifeCycle.Data.Channel.Enqueue((BirdSignal.GrownStage2, null));
                                EnergyConsumePerSecond *= 1.5f;
                                _lifeCycle.OnStateChanged -= stateChanged2;
                                done = true;
                            }
                            return false;
                        }
                        _lifeCycle.OnStateChanged += stateChanged2;
                        yield return new WaitUntil(() => done);
                        done = false;
                        if (_birdManager.AllBirds.Remove(gameObject))
                        {
                            gameObject.transform.name = "Pigeon3";
                            _birdManager.AllBirds.Add(gameObject);
                        }
                        break;
                    default:
                        break;
                }
            };

            var coroutine = this.GetProduceCashCoroutine(options =>
            {
                options.CoroutineTime = (timeStep: 7.0f, variationRange: 1f);
                options.CashInfo = new List<(float timeOut, float variationRange, GameObject cash)>
                {
                    (timeOut: 40f, variationRange: 5f, cash: null),
                    (timeOut: 60f, variationRange: 5f, cash: CashPrefabs[0]),
                    (timeOut: 0f, variationRange: 10f, cash: CashPrefabs[1])
                };
                options.HzedOutHandler = handleHzedOut;
            });

            StartCoroutine(coroutine?.Invoke(_cancelSource.Token));
        }
    }
}