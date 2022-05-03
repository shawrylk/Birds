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
        private BirdEnum(int id, string name) : base(id, name) { }
        public static BirdEnum Idling = new BirdEnum(0, nameof(Idling));
        public static BirdEnum Hunting = new BirdEnum(1, nameof(Hunting));
        public static BirdEnum Starving = new BirdEnum(2, nameof(Starving));
        public static BirdEnum Death = new BirdEnum(3, nameof(Death));
        public static BirdEnum Captured = new BirdEnum(4, nameof(Captured));
        public static BirdEnum Landing = new BirdEnum(5, nameof(Landing));
    }

    public class BirdSignal : Enumeration
    {
        private BirdSignal(int id, string name) : base(id, name) { }
        public static BirdSignal FoundFood = new BirdSignal(0, nameof(FoundFood));
        public static BirdSignal GrownStage1 = new BirdSignal(1, nameof(GrownStage1));
        public static BirdSignal GrownStage2 = new BirdSignal(2, nameof(GrownStage2));
        public static BirdSignal EnergyRegen = new BirdSignal(3, nameof(EnergyRegen));
        public static BirdSignal Grounded = new BirdSignal(4, nameof(Grounded));
        public static BirdSignal Captured = new BirdSignal(5, nameof(Captured));
        public static BirdSignal Landing = new BirdSignal(6, nameof(Landing));
        public static BirdSignal Fly = new BirdSignal(6, nameof(Fly));

    }
}
