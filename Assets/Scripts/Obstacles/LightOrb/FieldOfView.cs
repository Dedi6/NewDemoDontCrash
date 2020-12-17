using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    private int layerMask = (1 << 8) | (1 << 22); //ground soft and bullet go through/ for shadows
    private Mesh mesh;
    private float fov;
    private Vector3 origin;
    private float startingAngle, currentTime, cooldown;
    private GameObject player;
    public float viewDistance = 30f, time = 2f, cooldownMax = 0.5f, hitTime = 0.5f;
    private int detectionLayerMask = (1 << 11) | (1 << 8) | (1 << 22); //player and ground AND transparent(bulletgoThrough)
    private bool isCycling, goingForward = true, isActivated;
    private Material myMaterial;
    public Color StartColor, EndColor;
    private Animator animator;

    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        fov = 360f;
        player = GameMaster.instance.playerInstance;
        myMaterial = GetComponent<Renderer>().material;
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        origin = transform.position;
        FindTargetPlayer();
        if (isActivated && !isCycling)
        {
            isCycling = true;
            if (goingForward)
            {
                StartCoroutine(CycleMaterial(StartColor, EndColor, time, myMaterial));
                animator.SetTrigger("Expand");
                StartCoroutine(CheckForHit());
            }
        }
        if (cooldown >= 0)
            cooldown -= Time.deltaTime;
    }

    void LateUpdate()
    {

        int rayCount = 1000;
        float angle = 0f;
        float angleIncrease = fov / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = transform.InverseTransformPoint(origin);

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;
            RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, GetVectorfromAngle(angle), viewDistance, layerMask);
            if (raycastHit2D.collider == null)
            {
                //no hit
                vertex = origin + GetVectorfromAngle(angle) * viewDistance;
            }
            else
            {
                //hit object
                vertex = raycastHit2D.point;
            }

            vertices[vertexIndex] = transform.InverseTransformPoint(vertex);


            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }

    private void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    private Vector3 GetVectorfromAngle(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    private void FindTargetPlayer()
    {
        if (Vector3.Distance(transform.position, player.transform.position) < viewDistance)
        {
            if (CanDetectPlayer())
            {
                if (cooldown <= 0)
                {
                    isActivated = true;
                }
            }
            else
                isActivated = false;
        }
        else
        {
            if(currentTime > 0 )
                isActivated = false;
        }
    }

    IEnumerator CycleMaterial(Color startColor, Color endColor, float cycleTime, Material mat)
    {
        cooldown = cooldownMax;
        currentTime = 0;
        while (currentTime < cycleTime)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / cycleTime;
            Color currentColor = Color.Lerp(startColor, endColor, t);
            mat.color = currentColor;
            yield return null;
        }

        cooldown = cooldownMax;
        currentTime = 0;
        goingForward = !goingForward;

        if (mat.color == EndColor)
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(CycleMaterial(EndColor, StartColor, time, myMaterial));
            animator.SetTrigger("Retract");
        }
        else if (mat.color == StartColor)
        {
            isActivated = false;
            isCycling = false;
        }
    }
    
    private bool CanDetectPlayer()
    {
        Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
        RaycastHit2D rayToPlayer = Physics2D.Raycast(transform.position, dirToPlayer, viewDistance, detectionLayerMask);
        if (rayToPlayer.collider != null && rayToPlayer.collider.gameObject.layer == 11)
        {
            return true;
        }
        else if (rayToPlayer.collider == null && currentTime > 0)
            return false;

        return false;
    }

    private IEnumerator CheckForHit()
    {
        yield return new WaitForSeconds(hitTime);

        if(CanDetectPlayer())
        {
            player.GetComponent<Health>().DealDamage(1); //for now
        }

    }

}
