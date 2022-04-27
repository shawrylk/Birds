using Assets.Scripts.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(Global.BOUNDARIES_ORDER)]
public class Boundaries : MonoBehaviour
{
    private void Awake()
    {
        var boundaries = gameObject
            .transform
            .GetComponentsInChildren<Transform>()
            .Skip(Global.PARENT_TRANSFORM)
            .ToDictionary(t => t.name.ToLower(), t => t);

        SetUpBoundaries(boundaries, Global.UnitsPerPixel);

        Global.Items.Add(Global.BOUNDARIES_TAG, boundaries);
    }
    private void SetUpBoundaries(Dictionary<string, Transform> boundaries, float unit)
    {
        var topOffset = -50.ToUnit();
        var heightInUnit = Screen.height * unit;
        var widthInUnit = Screen.width * unit;
        var offset = 0;
        var boundaryWidth = 1;
        var boundaryHeight = 1;

        boundaries[Global.LEFT_BOUNDARY].transform.position = new Vector3(widthInUnit / -2f - offset - boundaryWidth / 2f, 0, 0);
        boundaries[Global.LEFT_BOUNDARY].transform.localScale = new Vector3(boundaryWidth, heightInUnit + offset, 0);

        boundaries[Global.RIGHT_BOUNDARY].transform.position = new Vector3(widthInUnit / 2f + offset + boundaryWidth / 2f, 0, 0);
        boundaries[Global.RIGHT_BOUNDARY].transform.localScale = new Vector3(boundaryWidth, heightInUnit + offset, 0);

        boundaries[Global.TOP_BOUNDARY].transform.position = new Vector3(0, heightInUnit / 2f + offset + topOffset + boundaryHeight / 2f, 0);
        boundaries[Global.TOP_BOUNDARY].transform.localScale = new Vector3(widthInUnit + offset, boundaryHeight, 0);

        boundaries[Global.BOTTOM_BOUNDARY].transform.position = new Vector3(0, heightInUnit / -2f - offset - boundaryHeight / 2f, 0);
        boundaries[Global.BOTTOM_BOUNDARY].transform.localScale = new Vector3(widthInUnit + offset, boundaryHeight, 0);

        boundaries[Global.WATER_SURFACE_BOUNDARY].transform.position = new Vector3(0, (heightInUnit / -2f - offset) * 0.5f, 0);
        boundaries[Global.WATER_SURFACE_BOUNDARY].transform.localScale = new Vector3(widthInUnit + offset, boundaryHeight, 0);

    }
}
