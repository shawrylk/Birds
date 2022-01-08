using Assets.Scripts;
using Assets.Scripts.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class BirdsManager : BaseScript
{
    public GameObject BirdPrefab;
    private const float SPAWN_TIME = 1f;

    private async void Awake()
    {
        var boundaries = await Global.Items.TryGetUntilNotNull(Global.BOUNDARIES_TAG) as Dictionary<string, Transform>;

        transform.position = boundaries[Global.TOP_BOUNDARY].position;

        StartCoroutine(SpawnBird(_cancelSource.Token, boundaries));
    }

    private IEnumerator SpawnBird(CancellationToken token, Dictionary<string, Transform> boundaries)
    {
        var offset = 0.6f;
        var horizontalMargin = new Vector3(offset, 0, 0);
        var verticalMargin = new Vector3(0, offset, 0);
        var left = boundaries[Global.LEFT_BOUNDARY].position + horizontalMargin;
        var right = boundaries[Global.RIGHT_BOUNDARY].position - horizontalMargin;
        var top = boundaries[Global.TOP_BOUNDARY].position - verticalMargin;

        while (true)
        {
            if (token.IsCancellationRequested) break;

            if (BirdPrefab != null)
            {
                var position = left.Range(right);
                position += top;
                Instantiate(
                   original: BirdPrefab,
                   position: position,
                   rotation: Quaternion.identity,
                   parent: transform);
            }

            yield return new WaitForSeconds(SPAWN_TIME);
        }
    }
}
