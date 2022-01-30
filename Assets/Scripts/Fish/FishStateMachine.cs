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
    public class FishContextState : State
    {
        public new Enumeration ID
        {
            get => base.ID;
            set => base.ID = value;
        }

        public Func<FishContext, IEnumerator> Coroutine
        {
            get => base.Couroutine;
            set => base.Couroutine = (Func<Context, IEnumerator>)value;
        }
    }
    public class FishContextData
    {
        public ConcurrentQueue<(FishSignal key, object value)> Channel { get; set; } = new ConcurrentQueue<(FishSignal, object)>();
    }

    public class FishContext : Context
    {
        public FishContextData Data { get; set; }
        public new FishContextState State
        {
            get => base.State as FishContextState;
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
        public new Func<(FishContextState oldState, FishContextState newState), bool> OnStateChanged;
        public FishContext(MonoBehaviour owner) : base(owner) { }
        public void Run(FishContextData data, FishContextState state)
        {
            Data = data;
            base.Run(state);
        }
    }
}
