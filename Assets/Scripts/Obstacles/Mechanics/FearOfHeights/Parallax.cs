using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Parallax : MonoBehaviour
{
    [SerializeField]
    private Transform cloudsMoving;
    [SerializeField]
    private float cloudsSpeed = 0.01f;

    private float textureUnitSize;

    [SerializeField]
    private ParalHelper[] test;

    [System.Serializable]
    public class ParalHelper
    {
        public Transform layer;
        public float parallaxDivider;
        public Vector3 lastPos;
    }

    // Start is called before the first frame update
    void Start()
    {
        HandleCloudsData();

        foreach (ParalHelper layer in test)
        {
            layer.lastPos = transform.position;
        }
    }

 
    private void LateUpdate()
    {
 
       
        foreach (ParalHelper layer in test)
        {
            Vector3 deltaMovement = transform.position - layer.lastPos;
            layer.layer.position += deltaMovement * layer.parallaxDivider * -1f;
            layer.lastPos = transform.position;
        }

        CloudsMove();
    }

   
    private void CloudsMove()
    {
        cloudsMoving.Translate(Vector3.left * cloudsSpeed * Time.deltaTime);
        
        if(transform.position.x - cloudsMoving.position.x >= textureUnitSize)
        {
            cloudsMoving.localPosition = new Vector3(cloudsMoving.localPosition.x + textureUnitSize, cloudsMoving.localPosition.y, cloudsMoving.localPosition.z);
        }
    }
    private void HandleCloudsData()
    {
        Sprite sprite = cloudsMoving.GetComponent<SpriteRenderer>().sprite;
        Texture2D texture = sprite.texture;
        textureUnitSize = (texture.width / sprite.pixelsPerUnit)*cloudsMoving.localScale.x;
    }
}
