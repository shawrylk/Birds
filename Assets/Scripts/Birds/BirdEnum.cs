using Assets.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Birds
{
    public class BirdEnum : Enumeration
    {
        public BirdEnum(int id, string name) : base(id, name) { }
        public static BirdEnum Idling = new BirdEnum(0, nameof(Idling));
        public static BirdEnum Hunting = new BirdEnum(1, nameof(Hunting));
        public static BirdEnum Starving = new BirdEnum(2, nameof(Starving));
        public static BirdEnum Death = new BirdEnum(3, nameof(Death));
    }

    public class BirdSignal : Enumeration
    {
        public BirdSignal(int id, string name) : base(id, name) { }
        public static BirdSignal FoundFood = new BirdSignal(0, nameof(FoundFood));
        public static BirdSignal Grown = new BirdSignal(1, nameof(Grown));
        public static BirdSignal EnergyRegen = new BirdSignal(2, nameof(EnergyRegen));
    }
}
