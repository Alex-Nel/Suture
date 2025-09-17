using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class FreeformMeshDeformer : MonoBehaviour
{
    [Range(0.0001f, 0.05f)] public float radius = 0.05f;
    [Range(0.001f, 0.05f)] public float deformationStrength = 0.05f;
    public float pushThreshold = 0.005f;

    [Header("AR Interaction Settings")]
    public GameObject controllerObject;
    public GameObject oppositeController;

    [Range(1f, 10f)] public float inflationMultiplier = 3f;

    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] displacedVertices;
    private Vector3[] vertexNormals;

    private float adjustedRadius;
    private float adjustedPushThreshold;

    private List<int> cavityTriangles;
    private List<int> healthyTriangles;

    private Dictionary<int, List<int>> vertexToTriangles;
    private HashSet<int> healedTriangles;

    [Header("Sound Effects")]
    public AudioSource drillAudio;
    public AudioSource fillAudio;

    void Start()
    {
        float scaleFactor = transform.localScale.x;
        adjustedRadius = radius / scaleFactor;
        adjustedPushThreshold = pushThreshold / scaleFactor;

        MeshFilter mf = GetComponent<MeshFilter>();
        mesh = Instantiate(mf.sharedMesh);
        mf.mesh = mesh;

        mesh.MarkDynamic();

        originalVertices = mesh.vertices;
        displacedVertices = (Vector3[])originalVertices.Clone();
        vertexNormals = mesh.normals;

        healthyTriangles = new List<int>(mesh.GetTriangles(0)); // submesh 0 = healthy
        cavityTriangles = new List<int>(mesh.GetTriangles(1));  // submesh 1 = cavity
        healedTriangles = new HashSet<int>();
        vertexToTriangles = new Dictionary<int, List<int>>();

        for (int i = 0; i < cavityTriangles.Count; i += 3)
        {
            for (int j = 0; j < 3; j++)
            {
                int vertexIndex = cavityTriangles[i + j];
                if (!vertexToTriangles.ContainsKey(vertexIndex))
                    vertexToTriangles[vertexIndex] = new List<int>();
                vertexToTriangles[vertexIndex].Add(i);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("MultiTarget")) return;

        float direction = 1f;
        if (other.gameObject == oppositeController) direction = -1f;

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        bool meshWasDeformed = false;

        for (int i = 0; i < displacedVertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(displacedVertices[i]);
            float dist = Vector3.Distance(worldPos, hitPoint);

            if (dist < adjustedRadius)
            {
                float falloff = 1f - (dist / adjustedRadius) * (dist / adjustedRadius);
                Vector3 normalWorld = transform.TransformDirection(vertexNormals[i]);

                float adjustedStrength = deformationStrength;
                if (direction < 0f) adjustedStrength *= inflationMultiplier;

                Vector3 displacement = -normalWorld * adjustedStrength * falloff * Time.deltaTime * direction;
                Vector3 newWorldPos = worldPos + displacement;

                Vector3 originalWorldPos = transform.TransformPoint(originalVertices[i]);
                if (Vector3.Dot(newWorldPos - originalWorldPos, normalWorld) > 0f)
                    newWorldPos = worldPos;

                Vector3 localNew = transform.InverseTransformPoint(newWorldPos);

                if ((displacedVertices[i] - localNew).sqrMagnitude > 1e-8f)
                {
                    displacedVertices[i] = localNew;
                    meshWasDeformed = true;

                    // Mark triangles for healing
                    if (vertexToTriangles.ContainsKey(i))
                    {
                        foreach (int triIndex in vertexToTriangles[i])
                        {
                            if (!healedTriangles.Contains(triIndex))
                            {
                                healedTriangles.Add(triIndex);

                                // Transfer triangle from cavity to healthy
                                healthyTriangles.Add(cavityTriangles[triIndex]);
                                healthyTriangles.Add(cavityTriangles[triIndex + 1]);
                                healthyTriangles.Add(cavityTriangles[triIndex + 2]);

                                cavityTriangles[triIndex] = -1; // invalidate
                                cavityTriangles[triIndex + 1] = -1;
                                cavityTriangles[triIndex + 2] = -1;
                            }
                        }
                    }
                }
            }
        }

        if (meshWasDeformed)
        {
            mesh.vertices = displacedVertices;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            mesh.subMeshCount = 2;
            mesh.SetTriangles(healthyTriangles, 0);  // Goes to white material
            mesh.SetTriangles(cavityTriangles.FindAll(index => index != -1), 1);  // Goes to black material


            MeshCollider collider = GetComponent<MeshCollider>();
            if (collider != null)
            {
                collider.sharedMesh = null;
                collider.sharedMesh = mesh;
            }

            if (direction > 0f)
            {
                if (drillAudio != null && !drillAudio.isPlaying)
                    drillAudio.Play();
                if (fillAudio != null && fillAudio.isPlaying)
                    fillAudio.Stop();
            }
            else
            {
                if (fillAudio != null && !fillAudio.isPlaying)
                    fillAudio.Play();
                if (drillAudio != null && drillAudio.isPlaying)
                    drillAudio.Stop();
            }
        }
        else
        {
            if (drillAudio != null && drillAudio.isPlaying)
                drillAudio.Stop();
            if (fillAudio != null && fillAudio.isPlaying)
                fillAudio.Stop();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (drillAudio != null) drillAudio.Stop();
        if (fillAudio != null) fillAudio.Stop();
    }
}