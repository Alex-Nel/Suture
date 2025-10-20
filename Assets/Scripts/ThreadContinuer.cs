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

    // Objects related to the suture pairs
    private (GameObject, GameObject, Vector3) pair;
    public List<(GameObject, GameObject, Vector3)> suturePairs;
    private GameObject pairObj1;

    public List<GameObject> threads;

    // Objects related to the mesh deformation
    public GameObject mesh;
    public float DistanceToApplySuture;
    public GameObject CutPoint;

    // Objects related to tying
    public GameObject TiePoint1;
    public GameObject TiePoint2;
    public bool Tied = false;


    void Start()
    {
        // Make a new points list
        points = new();

        // Make a new threads list
        threads = new();

        // Make a new suturePairs list
        suturePairs = new();

        // Reset Tie Points
        TiePoint1 = null;
        TiePoint2 = null;
        Tied = false;

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

            // Make a new GameObject at the new suture point
            GameObject obj = new();
            obj.name = "Suture Point";
            obj.transform.position = potentialPoint;

            // If total points only eqals 1, that means a new suture pair is being made. Hold suture point in pairObj1 until 4th is made.
            // If a 4th point is made, that is the other suture pair. Create a midpoint, and make the suture pair tuple. Add that to the list
            if (totalPoints == 1)
            {
                pairObj1 = obj;
            }
            else if (totalPoints == 4)
            {
                var midpoint = (pairObj1.transform.position + obj.transform.position) / 2f;

                CutPoint = new();
                CutPoint.transform.position = midpoint;
                CutPoint.name = "CutPoint";
                CutPoint.AddComponent<SphereCollider>();
                CutPoint.GetComponent<SphereCollider>().isTrigger = true;
                CutPoint.GetComponent<SphereCollider>().radius = 0.01f;
                CutPoint.AddComponent<messanger>();

                pair = (pairObj1, obj, midpoint);
                suturePairs.Add(pair);
                Debug.Log("Point1: " + pairObj1.transform.position);
                Debug.Log("Point2: " + obj.transform.position);
                Debug.Log("Midpoint: " + midpoint);

                totalPoints = 0;
            }

            // If there are more than 3 points, then insert at point.Count - 2 to account fo the tie objects.
            // Otherwise insert at the point.Count - 1.
            if (points.Count >= 3)
                points.Insert(points.Count - 2, obj);
            else
                points.Insert(points.Count - 1, obj);

            threadRenderer.positionCount++;

            potentialPoint = Vector3.zero;

            needleThreadPoint.GetComponent<messanger>().ExitedMesh = false;
        }

        // If the two items are pulled up a certain distance, apply the suture point for deformation
        if (suturePairs.Count > 0 && pair.Item1 != null)
        {
            if (Vector3.Distance(needle.transform.position, pair.Item3) > DistanceToApplySuture &&
                Vector3.Distance(threadSource.transform.position, pair.Item3) > DistanceToApplySuture)
            {
                mesh.GetComponent<SuturingMeshDeformer>().ApplySuture(pair.Item1, pair.Item2);
                pair = (null, null, Vector3.zero);
            }
        }


        // If there is a cut point, listen for a cut instruction
        if (CutPoint != null)
        {
            // If scissors enter the mesh, cut the string and finalize the suture pair
            if (CutPoint.GetComponent<messanger>().EnteredMesh == true)
            {
                Debug.Log("MidPoint entered, cut string");
                FinalizeSuture();
                CutPoint.GetComponent<messanger>().EnteredMesh = false;
            }
        }


        // If there are more than 3 points, make the tie points if they haven't been made
        // Set them as the midpoints of the first 2, and last 2 points
        if (points.Count >= 3)
        {
            if (TiePoint1 == null)
            {
                TiePoint1 = new();
                TiePoint1.name = "TiePoint1";
                points.Insert(1, TiePoint1);
                threadRenderer.positionCount++;
                Debug.Log("Tie point 1 Created");
            }
            if (TiePoint2 == null)
            {
                TiePoint2 = new();
                TiePoint2.name = "TiePoint2";
                points.Insert(points.Count - 1, TiePoint2);
                threadRenderer.positionCount++;
                Debug.Log("Tie point 2 Created");
            }

            if (Tied == false)
            {
                TiePoint1.transform.position = (points[0].transform.position + points[2].transform.position) / 2.0f;
                TiePoint2.transform.position = (points[points.Count - 1].transform.position + points[points.Count - 3].transform.position) / 2.0f;
            }
        }


        // Always make the first point the position of the thread source
        // points[0] = threadSource.transform.position;
        points[0] = threadSource;
        if (TiePoint1 != null)
            points[1] = TiePoint1;

        // Alays make the last point the position of the needleThreadPoint
        // points[points.Count - 1] = needleThreadPoint.transform.position;
        points[points.Count - 1] = needleThreadPoint;
        if (TiePoint2 != null)
            points[points.Count - 2] = TiePoint2;
    }

    void FinalizeSuture()
    {
        Destroy(CutPoint);

        // Reset main thread renderer
        points.Clear();

        Destroy(TiePoint1);
        TiePoint1 = null;
        Destroy(TiePoint2);
        TiePoint2 = null;

        threadRenderer.positionCount = 2;
        threadRenderer.SetPosition(0, threadSource.transform.position);
        points.Add(threadSource);

        threadRenderer.SetPosition(1, needleThreadPoint.transform.position);
        points.Add(needleThreadPoint);

        // Add a line for the new pair
        GameObject obj = new();
        obj.AddComponent<LineRenderer>();
        obj.GetComponent<LineRenderer>().startWidth = 0.001f;
        obj.GetComponent<LineRenderer>().endWidth = 0.001f;
        obj.GetComponent<LineRenderer>().material = threadMaterial;

        obj.GetComponent<LineRenderer>().positionCount = 2;
        obj.GetComponent<LineRenderer>().SetPosition(0, suturePairs[suturePairs.Count - 1].Item1.transform.position);
        obj.GetComponent<LineRenderer>().SetPosition(1, suturePairs[suturePairs.Count - 1].Item2.transform.position);
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
