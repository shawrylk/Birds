using Assets.Scripts;
using Assets.Scripts.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
[DefaultExecutionOrder(Global.BIRD_MANAGER_ORDER)]
public class BirdsManager : BaseScript
{
    public GameObject BirdPrefab;
    private Dictionary<string, Transform> _boundaries;
    public void Awake()
    {
        _boundaries = Global.Items[Global.BOUNDARIES_TAG] as Dictionary<string, Transform>;

        StartAutoSpawnBird();
    }
    private void StartAutoSpawnBird(float spawnTime = 5f)
    {
        var offset = 0.6f;
        var horizontalMargin = new Vector3(offset, 0, 0);
        var verticalMargin = new Vector3(0, offset, 0);
        var left = _boundaries[Global.LEFT_BOUNDARY].position + horizontalMargin;
        var right = _boundaries[Global.RIGHT_BOUNDARY].position - horizontalMargin;
        var top = _boundaries[Global.TOP_BOUNDARY].position - verticalMargin;
        gameObject.transform.position = _boundaries[Global.TOP_BOUNDARY].position;

        IEnumerator spawnCoroutine(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested) break;

                if (BirdPrefab != null)
                {
                    var position = left.Range(right);
                    position += top;
                    Object.Instantiate(
                       original: BirdPrefab,
                       position: position,
                       rotation: Quaternion.identity,
                       parent: gameObject.transform);
                }

                yield return new WaitForSeconds(spawnTime);
            }
        }

        StartCoroutine(spawnCoroutine(_cancelSource.Token));
    }
}
