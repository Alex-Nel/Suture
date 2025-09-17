using System.Collections.Generic;
using UnityEngine;

public class SkinDeformer : MonoBehaviour
{

    private Mesh mesh;
    private Vector3[] originalVertices;
    private Vector3[] displacedVertices;
    private MeshCollider meshCollider;

    public Material lineMaterial;

    public float selectionRadius = 0.01f;

    private List<(int, int)> suturePairs = new();
    private List<LineRenderer> lines = new();

    bool meshChanged = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = GetComponent<MeshCollider>();
        mesh.MarkDynamic();

        originalVertices = mesh.vertices;
        displacedVertices = (Vector3[])mesh.vertices.Clone();
    }

    // Update is called once per frame
    void Update()
    {
        if (meshChanged)
        {
            mesh.vertices = displacedVertices;
            mesh.RecalculateNormals();
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
            meshChanged = false;
        }

        if (displacedVertices == null)
        {
            Debug.Log("displacedVertices array is null");
        }
    }

    public void ApplySuture(Vector3 entryWorld, Vector3 exitWorld)
    {
        int vertexOne = GetClosestVertex(entryWorld);
        int vertexTwo = GetClosestVertex(exitWorld);

        Debug.Log("First vertex is at index: " + vertexOne);
        Debug.Log("Second vertex is at index: " + vertexTwo);

        suturePairs.Add((vertexOne, vertexTwo));

        // LineRenderer lr = new GameObject("SutureLine").AddComponent<LineRenderer>();
        // lr.material = lineMaterial;
        // lr.startWidth = lr.endWidth = 0.002f;
        // lr.positionCount = 2;
        // lr.useWorldSpace = true;
        // lr.SetPosition(0, transform.TransformPoint(displacedVertices[vertexOne]));
        // lr.SetPosition(1, transform.TransformPoint(displacedVertices[vertexTwo]));
        // lines.Add(lr);


        Vector3 vaWorld = transform.TransformPoint(displacedVertices[vertexOne]);
        Vector3 vbWorld = transform.TransformPoint(displacedVertices[vertexTwo]);
        Vector3 midpoint = (vaWorld + vbWorld) / 2f;

        // displacedVertices[vertexOne].x = midpoint.x;
        // displacedVertices[vertexOne].y = midpoint.y;
        // displacedVertices[vertexOne].z = midpoint.z;
        displacedVertices[vertexOne] = transform.InverseTransformPoint(midpoint);

        // displacedVertices[vertexTwo].x = midpoint.x;
        // displacedVertices[vertexTwo].y = midpoint.y;
        // displacedVertices[vertexTwo].z = midpoint.z;
        displacedVertices[vertexTwo] = transform.InverseTransformPoint(midpoint);

        meshChanged = true;

    }

    private int GetClosestVertex(Vector3 worldPoint)
    {
        // float minDist = float.MaxValue;
        // int closestIndex = -1;

        // for (int i = 0; i < displacedVertices.Length; i++)
        // {
        //     Vector3 vWorld = transform.TransformPoint(displacedVertices[i]);
        //     float dist = Vector3.Distance(vWorld, worldPoint);
        //     if (dist < selectionRadius && dist < minDist)
        //     {
        //         minDist = dist;
        //         closestIndex = i;
        //     }
        // }

        int closest = -1;
        float minDist = float.MaxValue;

        for (int i = 0; i < displacedVertices.Length; i++)
        {
            Vector3 vWorld = transform.TransformPoint(displacedVertices[i]);
            float dist = Vector3.Distance(vWorld, worldPoint);

            Debug.Log($"Vertex {i}: world position = {vWorld}, distance = {dist}");

            if (dist < minDist)
            {
                minDist = dist;
                closest = i;
            }
        }

        return closest;
    }
}
