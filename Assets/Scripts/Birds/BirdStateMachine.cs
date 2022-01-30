﻿using Assets.Contracts;
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
    public class BirdContextState : State
    {
        public new Enumeration ID
        {
            get => base.ID;
            set => base.ID = value;
        }

        public Func<BirdContext, IEnumerator> Coroutine
        {
            get => base.Couroutine;
            set => base.Couroutine = (Func<Context, IEnumerator>)value;
        }
    }
    public class BirdContextData
    {
        public ConcurrentQueue<(BirdSignal key, object value)> Channel { get; set; } = new ConcurrentQueue<(BirdSignal, object)>();
    }

    public class BirdContext : Context
    {
        public BirdContextData Data { get; set; }
        public new BirdContextState State
        {
            get => base.State as BirdContextState;
            set
            {
                if (base.State != value)
                {
                    if (OnStateChanged?.Invoke((State, value)) != true)
                    {
                        base.State = value;
                    }
                }
            }
        }
        public new Func<(BirdContextState oldState, BirdContextState newState), bool> OnStateChanged;
        public BirdContext(MonoBehaviour owner) : base(owner) { }
        public void Run(BirdContextData data, BirdContextState state)
        {
            Data = data;
            base.Run(state);
        }
    }
}
