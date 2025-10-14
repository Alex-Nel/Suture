using System.Collections.Generic;
using UnityEngine;

public class ThreadContinuer : MonoBehaviour
{
    // Objects related to the needle
    public GameObject needle;
    public GameObject needlePoint;
    public GameObject needleThreadPoint;

    // Objects related to the thread
    // public GameObject thread; // might be needed in the future
    public GameObject threadSource;

    // Line Renderer
    public LineRenderer threadRenderer;

    public Material threadMaterial;
    // Points for the line renderer
    // public List<Vector3> points;
    public List<GameObject> points;


    // Bool to know when a pair is starting // Future use
    // private bool startingSuturePair = false;
    private int totalPoints = 0;

    // Holds a potential point to add
    private Vector3 potentialPoint;

    private (GameObject, GameObject, Vector3) pair;
    public List<(GameObject, GameObject, Vector3)> suturePairs;
    private GameObject pairObj1;

    public List<GameObject> threads;

    public GameObject mesh;


    void Start()
    {
        // Make a new points list
        points = new();

        // Make a new threads list
        threads = new();

        // Make a new suturePairs list
        suturePairs = new();

        // Set up settings on the line renderer
        threadRenderer = GetComponent<LineRenderer>();
        threadRenderer.startWidth = 0.001f;
        threadRenderer.endWidth = 0.001f;
        threadRenderer.material = threadMaterial;
        Debug.Log("Set line renderer settings");

        // Make the first two points of the line renderer
        threadRenderer.positionCount = 2;
        threadRenderer.SetPosition(0, threadSource.transform.position);
        // points.Add(threadSource.transform.position);
        points.Add(threadSource);
        threadRenderer.SetPosition(1, needleThreadPoint.transform.position);
        // points.Add(needleThreadPoint.transform.position);
        points.Add(needleThreadPoint);
        Debug.Log("Line renderer has: " + threadRenderer.positionCount + " points");
    }

    // Update is called once per frame
    void Update()
    {
        // Set each position of the line renderer to the poinst list.
        for (int i = 0; i < points.Count; i++)
        {
            threadRenderer.SetPosition(i, points[i].transform.position);
        }

        // Only make a line renderer on the last two points
        // threadRenderer.SetPosition(0, points[points.Count - 2]);
        // threadRenderer.SetPosition(1, points[points.Count - 1]);


        // Happens when the needle thread point enters the mesh
        if (needleThreadPoint.GetComponent<messanger>().EnteredMesh == true)
        {
            // Mark the point of entry as a potential point
            potentialPoint = needleThreadPoint.transform.position;

            needleThreadPoint.GetComponent<messanger>().EnteredMesh = false;
        }
        else if (needleThreadPoint.GetComponent<messanger>().ExitedMesh == true)
        {
            totalPoints++;

            // Put the potential point in the points list, and to the line renderer. Reset the potential point to zero.
            // points.Insert(points.Count - 1, potentialPoint);
            GameObject obj = new();
            obj.transform.position = potentialPoint;
            if (totalPoints == 1)
            {
                pairObj1 = obj;
            }
            else if (totalPoints == 4)
            {
                var midpoint = (pairObj1.transform.position + obj.transform.position) / 2f;
                pair = (pairObj1, obj, midpoint);
                suturePairs.Add(pair);
                Debug.Log("Point1: " + pairObj1.transform.position);
                Debug.Log("Point2: " + obj.transform.position);
                Debug.Log("Midpoint: " + midpoint);

                totalPoints = 0;
            }
            points.Insert(points.Count - 1, obj);
            threadRenderer.positionCount++;

            // threadRenderer.positionCount++;
            potentialPoint = Vector3.zero;

            // Make a thread between the new point and the previous one
            // AddThread(points[points.Count - 3], points[points.Count - 2]);
            Debug.Log("Adding thread");

            needleThreadPoint.GetComponent<messanger>().ExitedMesh = false;
        }

        if (suturePairs.Count > 0)
        {
            if (Vector3.Distance(needle.transform.position, suturePairs[suturePairs.Count-1].Item3) > 0.08f &&
                Vector3.Distance(threadSource.transform.position, suturePairs[suturePairs.Count-1].Item3) > 0.08f)
            {
                mesh.GetComponent<SuturingMeshDeformer>().ApplySuture(suturePairs[suturePairs.Count-1].Item1, suturePairs[suturePairs.Count-1].Item2);
            }
        }


        // Alays make the last point the position of the needleThreadPoint
        // points[points.Count - 1] = needleThreadPoint.transform.position;
        points[points.Count - 1] = needleThreadPoint;
        // Always make the first point the position of the thread source
        // points[0] = threadSource.transform.position;
        points[0] = threadSource;
    }

    void AddThread(Vector3 point1, Vector3 point2)
    {
        GameObject thrd = new();
        thrd.AddComponent<ThreadController>();

        thrd.GetComponent<ThreadController>().ropeRadius = threadRenderer.startWidth;
        thrd.GetComponent<ThreadController>().ropeMaterial = threadMaterial;


        // Make two new objects for the start and end point of the thread.
        GameObject strt = new();
        strt.transform.position = point1;
        strt.AddComponent<Rigidbody>();

        GameObject end = new();
        end.transform.position = point2;
        end.AddComponent<Rigidbody>();

        // In the very unique case that point 1 is the threadSrc, use that instead, otherwise use the new object
        if (point1 == points[0].transform.position)
        {
            thrd.GetComponent<ThreadController>().startTransform = threadSource.transform;
            Destroy(strt);
        }
        else
        {
            thrd.GetComponent<ThreadController>().startTransform = strt.transform;
        }
        thrd.GetComponent<ThreadController>().endTransform = end.transform;

        // Set the segment count based on distance (kind of broken right now)
        // thrd.GetComponent<ThreadController>().segmentCount = (int)(Vector3.Distance(point1, point2) / 0.02f);
        thrd.GetComponent<ThreadController>().segmentCount = 2;
        thrd.GetComponent<ThreadController>().segmentLength = Vector3.Distance(point1, point2) / 3f;


        // Make rigid bodies and make them kinematic so that they don't move
        strt.GetComponent<Rigidbody>().isKinematic = true;
        end.GetComponent<Rigidbody>().isKinematic = true;

        threads.Add(thrd);
    }
}
