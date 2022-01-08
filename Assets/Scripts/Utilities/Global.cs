using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public static class Global
    {
        public const string FOODS_TAG = "Foods";
        public const string FOOD_TAG = "Food";
        public const string BOUNDARIES_TAG = "Boundaries";
        public const string BOUNDARY_TAG = "Boundary";
        public const string BIRDS_TAG= "Birds";
        public const string BIRD_TAG = "Bird";

        public const string LEFT_BOUNDARY = "left";
        public const string RIGHT_BOUNDARY = "right";
        public const string TOP_BOUNDARY = "top";
        public const string BOTTOM_BOUNDARY = "bottom";
        public const string UNITS_PER_PIXEL= "UnitsPerPixel";

        public const int PARENT_TRANSFORM = 1;
        public const int PIXEL_PER_UNIT = 100;

        public static Dictionary<string, GameObject> GameObjects = new Dictionary<string, GameObject>();
        public static Dictionary<object, object> Items = new Dictionary<object, object>();

        public static readonly float UnitsPerPixel;
        static Global()
        {
            GameObjects.Clear();
            Items.Clear();

            var camera = Camera.main;
            var screenHeightInUnit = camera.orthographicSize * 2;
            UnitsPerPixel = screenHeightInUnit / Screen.height;
        }
    }
}
