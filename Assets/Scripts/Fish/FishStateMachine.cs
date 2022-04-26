using Assets.Contracts;
using Assets.Scripts.Birds;
using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Fishes
{
    public class FishState : State
    {
        public new Enumeration ID
        {
            get => base.ID;
            set => base.ID = value;
        }

        public new FishConductor Conductor { get; set; }
        public new Func<IEnumerator> Coroutine
        {
            get => base.Coroutine;
            set => base.Coroutine = (Func<IEnumerator>)value;
        }
        public new Func<(FishState oldState, FishState newState), bool> OnStateChanged;

    }
    public class FishContext
    {
        public ConcurrentQueue<(FishSignal key, object value)> Channel { get; set; } = new ConcurrentQueue<(FishSignal, object)>();
    }

    public class FishConductor : StateConductor
    {
        public FishContext Data { get; set; }
        public new FishState State
        {
            get => base.State as FishState;
            set
            {
                if (base.State != value)
                {
                    var oldState = State;
                    if (oldState?.OnStateChanged?.Invoke((oldState, value)) != true)
                    {
                        base.State = value;
                        value.Conductor = oldState.Conductor;
                        value.Owner = oldState?.Owner ?? owner;
                        value?.OnStateChanged?.Invoke((oldState, value));
                        Array.ForEach(
                            oldState?.OnStateChanged?.GetInvocationList() ?? Array.Empty<Func<(BirdState oldState, BirdState newState)>>(),
                            e => oldState.OnStateChanged -= (Func<(FishState oldState, FishState newState), bool>)e);

                    }
                }
            }
        }
        public new Func<(FishState oldState, FishState newState), bool> OnStateChanged;
        public FishConductor(MonoBehaviour owner) : base(owner) { }
        public void Run(FishContext data, FishState state)
        {
            Data = data;
            state.Conductor = this;
            base.Run(state);
        }
        public override void ChangeState(State state, bool isForce = false)
        {
            if (state is FishState fs)
            {
                State = fs;
                base.ChangeState(state, isForce);
            }
        }
    }
}
