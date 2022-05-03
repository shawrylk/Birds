﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utilities
{
    public static class Global
    {
        public const string FOOD_MANAGER_TAG = "FoodManager";
        public const string FOOD_TAG = "Food";
        public const string BOUNDARIES_TAG = "Boundaries";
        public const string BOUNDARY_TAG = "Boundary";
        public const string BIRD_MANAGER_TAG = "BirdManager";
        public const string BIRD_TAG = "Bird";
        public const string CASH_MANAGER_TAG = "CashManager";
        public const string CASH_TAG = "Cash";
        public const string SCORE_TAG = "Score";
        public const string UI_TAG = "UI";
        public const string ENEMY_MANAGER_TAG = "EnemyManager";
        public const string ENEMY_TAG = "Enemy";
        public const string SMALL_FISH_TAG = "SmallFish";
        public const string ELECTRIC_LINE_TAG = "ElectricLine";
        public const string TREE_TAG = "Tree";
        public const string HOUSE_TAG = "House";
        public const string INPUT = "Input";

        public const string BIRDS_MARK_LAYER = "Birds";

        public const string LEFT_BOUNDARY = "left";
        public const string RIGHT_BOUNDARY = "right";
        public const string TOP_BOUNDARY = "top";
        public const string BOTTOM_BOUNDARY = "bottom";
        public const string LAND_BOUNDARY = "land";
        //public const string BOTTOM2_BOUNDARY = "bottom2";
        //public const string WATER_SURFACE_BOUNDARY = "surface";
        public const string UNITS_PER_PIXEL = "UnitsPerPixel";

        public const int PARENT_TRANSFORM = 1;
        public const int PIXEL_PER_UNIT = 100;

        public static Dictionary<string, GameObject> GameObjects = new Dictionary<string, GameObject>();
        public static Dictionary<string, object> Items = new Dictionary<string, object>();

        public static readonly float UnitsPerPixel;

        public const int INPUT_SCRIPT = -110;
        public const int LEVEL_MANAGER_ORDER = -100;
        public const int CASH_MANAGER_ORDER = -90;
        public const int BOUNDARIES_ORDER = -80;
        public const int BIRD_MANAGER_ORDER = -70;
        public const int ENEMY_MANAGER_ORDER = -60;
        public const int FOOD_MANAGER_ORDER = -50;
        static Global()
        {
            GameObjects.Clear();
            Items.Clear();

            var camera = Camera.main;
            //if (Screen.height / Screen.width >= 2)
            //{
            camera.orthographicSize = Screen.height / 600f;
            //}
            //else
            //{
            //    camera.orthographicSize = 3.2f;
            //}
            var screenHeightInUnit = camera.orthographicSize * 2;
            UnitsPerPixel = screenHeightInUnit / Screen.height;
            Debug.Log("orthographicSize " + camera.orthographicSize);
            Debug.Log("UnitsPerPixel " + UnitsPerPixel);
            Debug.Log("PixelsPerUnit " + 1 / UnitsPerPixel);
        }
    }
}
