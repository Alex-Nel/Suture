using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class SuturingMeshDeformer : MonoBehaviour
{
    [Header("Deformation Settings")]
    public float selectionRadius = 0.01f;
    public float influenceRadius = 0.01f;
    public float pullStrength = 0.03f;
    public Material lineMaterial;

    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] displacedVertices;
    private MeshCollider meshCollider;

    private List<(int, int)> suturePairs = new();
    private List<LineRenderer> lines = new();

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = GetComponent<MeshCollider>();
        mesh.MarkDynamic();

        originalVertices = mesh.vertices;
        displacedVertices = (Vector3[])originalVertices.Clone();
    }

    void Update()
    {
        bool meshChanged = false;

        for (int i = 0; i < suturePairs.Count; i++)
        {
            var (a, b) = suturePairs[i];
            if (a == -1 || b == -1) continue;

            Vector3 vaWorld = transform.TransformPoint(displacedVertices[a]);
            Vector3 vbWorld = transform.TransformPoint(displacedVertices[b]);
            Vector3 midpoint = (vaWorld + vbWorld) / 2f;

            for (int j = 0; j < displacedVertices.Length; j++)
            {
                Vector3 vWorld = transform.TransformPoint(displacedVertices[j]);
                float distA = Vector3.Distance(vWorld, vaWorld);
                float distB = Vector3.Distance(vWorld, vbWorld);

                if (distA < influenceRadius || distB < influenceRadius)
                {
                    float weight = 1f - Mathf.Min(distA, distB) / influenceRadius;
                    Vector3 target = Vector3.Lerp(vWorld, midpoint, weight * pullStrength * Time.deltaTime);
                    displacedVertices[j] = transform.InverseTransformPoint(target);
                    meshChanged = true;
                }
            }

            if (i < lines.Count)
            {
                lines[i].SetPosition(0, vaWorld);
                lines[i].SetPosition(1, vbWorld);
            }
        }

        if (meshChanged)
        {
            mesh.vertices = displacedVertices;
            mesh.RecalculateNormals();
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
        }
    }

    public void ApplySuture(Vector3 entryWorld, Vector3 exitWorld)
    {
        int entryIndex = GetClosestVertex(entryWorld);
        int exitIndex = GetClosestVertex(exitWorld);

        if (entryIndex == -1 || exitIndex == -1) return;

        suturePairs.Add((entryIndex, exitIndex));

        LineRenderer lr = new GameObject("SutureLine").AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.startWidth = lr.endWidth = 0.002f;
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.SetPosition(0, transform.TransformPoint(displacedVertices[entryIndex]));
        lr.SetPosition(1, transform.TransformPoint(displacedVertices[exitIndex]));
        lines.Add(lr);
    }

    private int GetClosestVertex(Vector3 worldPoint)
    {
        float minDist = float.MaxValue;
        int closestIndex = -1;

        for (int i = 0; i < displacedVertices.Length; i++)
        {
            Vector3 vWorld = transform.TransformPoint(displacedVertices[i]);
            float dist = Vector3.Distance(vWorld, worldPoint);
            if (dist < selectionRadius && dist < minDist)
            {
                minDist = dist;
                closestIndex = i;
            }
        }

        return closestIndex;
    }
}
