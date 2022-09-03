using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage : MonoBehaviour
{
    List<GameObject> trailParts = new List<GameObject>();
    [SerializeField]
    private Color newColor;
    [SerializeField]
    float repeatRate = 0.05f, lifetime = 0.5f, alphaInterval = 0.025f, alphaChange = 0.15f;

    void Start()
    {
        InvokeRepeating("SpawnTrailPart", 0, repeatRate);
    }

    void SpawnTrailPart()
    {
        GameObject trailPart = new GameObject();
        SpriteRenderer trailPartRenderer = trailPart.AddComponent<SpriteRenderer>();
        trailPartRenderer.sprite = GetComponent<SpriteRenderer>().sprite;
        trailPartRenderer.color = newColor;
        trailPart.transform.position = transform.position;
        trailPart.transform.localScale = transform.localScale;
        trailPart.transform.rotation = transform.rotation;
        trailParts.Add(trailPart);

        StartCoroutine(FadeTrailPart(trailPartRenderer));
        Destroy(trailPart, lifetime);
    }

    IEnumerator FadeTrailPart(SpriteRenderer trailPartRenderer)
    {
        Color color = trailPartRenderer.color;
        color.a -= alphaChange; 
        trailPartRenderer.color = color;
        Debug.Log("D");

        yield return new WaitForSeconds(alphaInterval);

        if(color.a > 0)
            StartCoroutine(FadeTrailPart(trailPartRenderer));
    }

    private void OnDisable()
    {
        CancelInvoke();
    }

    private void OnEnable()
    {
        InvokeRepeating("SpawnTrailPart", 0, 0.05f); // replace 0.2f with needed repeatRate
    }
}
