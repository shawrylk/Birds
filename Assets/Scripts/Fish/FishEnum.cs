using Assets.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Fishes
{
    public class FishEnum : Enumeration
    {
        public FishEnum(int id, string name) : base(id, name) { }
        public static FishEnum HuntingStage1 = new FishEnum(0, nameof(HuntingStage1));
        public static FishEnum HuntingStage2 = new FishEnum(1, nameof(HuntingStage2));
        public static FishEnum BeingAttacked = new FishEnum(2, nameof(BeingAttacked));
        public static FishEnum Death = new FishEnum(3, nameof(Death));
    }

    public class FishSignal : Enumeration
    {
        private FishSignal(int id, string name) : base(id, name) { }
        public static FishSignal FoundFood = new FishSignal(0, nameof(FishSignal));
    }
}
