using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Decorations_Parallax : MonoBehaviour
{
    private Transform camTransform;

    [SerializeField]
    private DecorationHelper[] layers;

    [System.Serializable]
    public class DecorationHelper
    {
        public Transform layer;
        public float parallaxDivider;
        [HideInInspector]
        public Vector3 lastPos;
    }

    void Start()
    {
        camTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        foreach (DecorationHelper layer in layers)
        {
            Vector3 deltaMovement = camTransform.position - layer.lastPos;
            layer.layer.position += deltaMovement * layer.parallaxDivider;
            layer.lastPos = camTransform.position;
        }
    }
}
