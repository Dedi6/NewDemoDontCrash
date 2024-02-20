using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Parallax : MonoBehaviour
{
    [SerializeField]
    private Transform cloudsMoving;
    [SerializeField]
    private float cloudsSpeed = 0.01f;
    [SerializeField]
    private CloudsHelper[] cloudsArray;

    private float textureUnitSize;

    [SerializeField]
    private ParalHelper[] test;

    [System.Serializable]
    public class ParalHelper
    {
        public Transform layer;
        public float parallaxDivider;
        [HideInInspector]
        public Vector3 lastPos;
    }

    [System.Serializable]
    public class CloudsHelper
    {
        public Transform cloudsTransform;
        public float cloudsSpeed;
        [HideInInspector]
        public float current_TextureUnitSize;
    }

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
        foreach (CloudsHelper currentClouds in cloudsArray)
        {
            currentClouds.cloudsTransform.Translate(Vector3.left * currentClouds.cloudsSpeed * Time.deltaTime);
            
            if (transform.position.x - currentClouds.cloudsTransform.position.x >= currentClouds.current_TextureUnitSize)
            {
                currentClouds.cloudsTransform.localPosition = new Vector3(currentClouds.cloudsTransform.localPosition.x + currentClouds.current_TextureUnitSize, currentClouds.cloudsTransform.localPosition.y, currentClouds.cloudsTransform.localPosition.z);
            }
        }

    }
    private void HandleCloudsData()
    {

        foreach (CloudsHelper currentCloud in cloudsArray)
        {
            Sprite sprite = currentCloud.cloudsTransform.GetComponent<SpriteRenderer>().sprite;
            Texture2D texture = sprite.texture;
            currentCloud.current_TextureUnitSize = (texture.width / sprite.pixelsPerUnit) * currentCloud.cloudsTransform.localScale.x;
        }
    }
}
