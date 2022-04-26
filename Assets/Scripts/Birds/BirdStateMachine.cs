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

namespace Assets.Scripts.Birds
{
    public class BirdState : State
    {
        public new Enumeration ID
        {
            get => base.ID;
            set => base.ID = value;
        }

        public new BirdConductor Conductor { get; set; }
        public new Func<IEnumerator> Coroutine
        {
            get => base.Coroutine;
            set => base.Coroutine = (Func<IEnumerator>)value;
        }

        public new Func<(BirdState oldState, BirdState newState), bool> OnStateChanged;

    }
    public class BirdContext
    {
        public int RemainHuntingTime { get; set; }
        public ConcurrentQueue<(BirdSignal key, object value)> Channel { get; set; } = new ConcurrentQueue<(BirdSignal, object)>();
    }

    public class BirdConductor : StateConductor
    {
        public new Func<(BirdState oldState, BirdState newState), bool> OnStateChanged;
        public BirdContext Context { get; set; }
        public new BirdState State
        {
            get => base.State as BirdState;
            private set
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
                        value?.Conductor?.OnStateChanged?.Invoke((oldState, value));
                        Array.ForEach(
                            oldState?.OnStateChanged?.GetInvocationList() ?? Array.Empty<Func<(BirdState oldState, BirdState newState)>>(),
                            e => oldState.OnStateChanged -= (Func<(BirdState oldState, BirdState newState), bool>)e);
                    }
                }
            }
        }
        public BirdConductor(MonoBehaviour owner) : base(owner) { }
        public void Run(BirdContext data, BirdState state)
        {
            Context = data;
            state.Conductor = this;
            base.Run(state);
        }
        protected override IEnumerator Coroutine()
        {
            while (true)
            {
                yield return State.Coroutine?.Invoke();
            }
        }
        /// <summary>
        /// Changes to new state only when old state yields
        /// </summary>
        /// <param name="state"></param>
        public override void ChangeState(State state, bool isForce = false)
        {
            if (state is BirdState bs)
            {
                State = bs;
                base.ChangeState(state, isForce);
            }
        }
    }
}
