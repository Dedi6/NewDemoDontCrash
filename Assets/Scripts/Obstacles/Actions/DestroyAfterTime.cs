using System.Collections;
using UnityEngine;

public class DestroyAfterTime : MonoBehaviour
{
    [SerializeField]
    private float destroyTimer;
    void Start()
    {
        StartCoroutine(DestroyCoroutine());
    }

   private IEnumerator DestroyCoroutine()
    {
        yield return new WaitForSeconds(destroyTimer);

        Destroy(gameObject);
    }
}
