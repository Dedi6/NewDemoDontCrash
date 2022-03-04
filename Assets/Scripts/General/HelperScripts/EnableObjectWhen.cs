using UnityEngine;

public class EnableObjectWhen : MonoBehaviour
{
    [SerializeField]
    private GameObject[] gameObjects;
    [SerializeField]
    private bool shouldActivate;

    public void SetObjectsActive()
    {
        foreach (GameObject gameObject in gameObjects)
        {
            gameObject.SetActive(shouldActivate);
        }
    }
}
