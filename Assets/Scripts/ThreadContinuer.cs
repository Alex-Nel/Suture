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
    public float threadWidth; // Recommended value: 0.0005f

    // Points for the line renderer
    // public List<Vector3> points;
    public List<GameObject> points;

    // Bool to know when a pair is starting // Future use
    // private bool startingSuturePair = false;
    private int totalPoints = 0;

    // Holds a potential point to add
    private Vector3 potentialPoint;

    // Objects related to the suture pairs
    [Header("Objects related to the suture pairs")]
    private (GameObject, GameObject, Vector3) pair;
    public List<(GameObject, GameObject, Vector3)> suturePairs;
    private GameObject pairObj1;

    public List<GameObject> threads; // Unused currently


    // Objects and values related to the mesh deformation
    [Header("Objects and values related to the mesh deformation")]
    public GameObject mesh;
    public float DistanceToApplySuture; // Recommended value: 0.035
    public float PullStrengthWeight; // Recommended value: ~30
    public float MaxPullStrength; // Unused, but recommended value: 0.3
    public GameObject CutPoint;

    // Objects and values related to tieing
    [Header("Objects and values related to tieing")]
    public GameObject TiePoint1;
    public GameObject TiePoint2;
    public bool Tied = false;
    
    public float wA, wB, wC; // These weights mostly stay at 1, might remove
    public float wCbias; // Recommended value: 1.25
    public float DistanceBetweenMainPoints;
    public float DistanceFromMidPoint;

    [Header("Other Values")]
    public bool needleLocked;
    public bool needlePointInMesh;
    public Vector3 lockedPos;
    public Quaternion lockedRot;


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

        //Reset other variables
        wA = 1.0f;
        wB = 1.0f;
        wC = 1.0f;
        DistanceBetweenMainPoints = -1f;
        DistanceFromMidPoint = 0f;

        // Set up settings on the line renderer
        threadRenderer = GetComponent<LineRenderer>();
        threadRenderer.startWidth = threadWidth;
        threadRenderer.endWidth = threadWidth;
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

        // Get the distance between the 2 main points (needle thread point, and the thread source point)
        DistanceBetweenMainPoints = Vector3.Distance(needleThreadPoint.transform.position, threadSource.transform.position);

        // When needleThreadPoint touches the skin, save the point as a potential point
        // If it exits, make the potential point a position for a suture
        if (needleThreadPoint.GetComponent<messenger>().EnteredMesh == true)
        {
            // Mark the point of entry as a potential point
            potentialPoint = needleThreadPoint.transform.position;
            needleThreadPoint.GetComponent<messenger>().EnteredMesh = false;
        }
        else if (needleThreadPoint.GetComponent<messenger>().ExitedMesh == true)
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
                CutPoint.GetComponent<SphereCollider>().radius = 0.002f;
                CutPoint.AddComponent<messenger>();
                CutPoint.GetComponent<messenger>().TargetTag = "Scissors";

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

            needleThreadPoint.GetComponent<messenger>().ExitedMesh = false;
        }

        // 
        // Skin Deformation:
        // If the two items are pulled up a certain distance, apply the suture point for deformation
        //
        if (suturePairs.Count > 0 /*&& pair.Item1 != null*/)
        {
            // Calculate the distance of the tools from the Cutpoint
            Vector3 temp = (needleThreadPoint.transform.position + threadSource.transform.position) / 2.0f;
            // DistanceFromMidPoint = Vector3.Distance(temp, CutPoint.transform.position);
            DistanceFromMidPoint = Vector3.Distance(temp, pair.Item3);

            // If that distance if more than the minimum, apply the pair
            if (DistanceFromMidPoint > DistanceToApplySuture)
            {
                if (pair.Item1 != null)
                {
                    mesh.GetComponent<SuturingMeshDeformer>().ApplySuture(pair.Item1, pair.Item2);
                    pair.Item1 = null;
                    pair.Item2 = null;
                }
            }

            // Adjust the distance from cutpoint relative to the minimum distance
            DistanceFromMidPoint -= DistanceToApplySuture;
            if (DistanceFromMidPoint < 0)
                DistanceFromMidPoint = 0;

            // Set the mesh deformation pull strength to the distance from cutpoint.
            mesh.GetComponent<SuturingMeshDeformer>().pullStrength = DistanceFromMidPoint * PullStrengthWeight;
            // mesh.GetComponent<SuturingMeshDeformer>().pullStrength = DistanceFromMidPoint;
        }


        //
        // Cutting and finalizing suture:
        // If there is a cut point, listen for a cut instruction
        //
        if (CutPoint != null)
        {
            // If scissors enter the mesh, cut the string and finalize the suture pair
            if (CutPoint.GetComponent<messenger>().EnteredMesh == true)
            {
                CutPoint.GetComponent<messenger>().EnteredMesh = false;
                if (Tied == true)
                {
                    Debug.Log("MidPoint entered, cut string");
                    FinalizeSuture();
                    pair = (null, null, Vector3.zero);
                }
            }
        }


        //
        // If there are more than 3 points, make the tie points if they haven't been made
        // Set them as the midpoints of the first 2, and last 2 points
        //
        if (points.Count >= 3)
        {
            if (TiePoint1 == null)
            {
                TiePoint1 = new();
                TiePoint1.name = "TiePoint1";
                TiePoint1.tag = "TiePoint";
                TiePoint1.AddComponent<CapsuleCollider>();
                TiePoint1.GetComponent<CapsuleCollider>().isTrigger = true;
                TiePoint1.GetComponent<CapsuleCollider>().radius = 0.0005f;
                TiePoint1.GetComponent<CapsuleCollider>().height = 0.01f;
                TiePoint1.AddComponent<messenger>();
                TiePoint1.GetComponent<messenger>().TargetTag = "TiePoint";
                TiePoint1.AddComponent<Rigidbody>();
                TiePoint1.GetComponent<Rigidbody>().isKinematic = true;

                points.Insert(1, TiePoint1);
                threadRenderer.positionCount++;
                Debug.Log("Tie point 1 Created");
            }
            if (TiePoint2 == null)
            {
                TiePoint2 = new();
                TiePoint2.name = "TiePoint2";
                TiePoint2.tag = "TiePoint";
                TiePoint2.AddComponent<CapsuleCollider>();
                TiePoint2.GetComponent<CapsuleCollider>().isTrigger = true;
                TiePoint2.GetComponent<CapsuleCollider>().radius = 0.0005f;
                TiePoint2.GetComponent<CapsuleCollider>().height = 0.01f;
                TiePoint2.AddComponent<messenger>();
                TiePoint2.GetComponent<messenger>().TargetTag = "TiePoint";

                points.Insert(points.Count - 1, TiePoint2);
                threadRenderer.positionCount++;
                Debug.Log("Tie point 2 Created");
            }

            // Dependong on whether the string is tied, make the tie points have a different position
            // If they are tied, make them the midpoint between the two "ends", and the cutpoint
            if (Tied == false)
            {
                TiePoint1.transform.position = (points[0].transform.position + points[2].transform.position) / 2.0f;
                TiePoint1.transform.rotation = Quaternion.FromToRotation(Vector3.up, points[0].transform.position - points[2].transform.position);
                TiePoint2.transform.position = (points[points.Count - 1].transform.position + points[points.Count - 3].transform.position) / 2.0f;
                TiePoint2.transform.rotation = Quaternion.FromToRotation(Vector3.up, points[points.Count - 1].transform.position - points[points.Count - 3].transform.position);
            }
            else if (Tied == true)
            {
                // wA = 1.0f / (DistanceBetweenMainPoints + 0.00001f) / 10.0f;
                // wB = 1.0f / (DistanceBetweenMainPoints + 0.00001f) / 10.0f;
                wC = ((DistanceBetweenMainPoints + 0.00001f) * 10.0f) + wCbias;
                if (wC < 0.5f)
                    wC = 0.5f;

                Vector3 MidPoint = (
                    threadSource.transform.position * wA +
                    needleThreadPoint.transform.position * wB +
                    CutPoint.transform.position * wC
                ) / (wA + wB + wC);

                TiePoint1.transform.position = MidPoint;
                TiePoint2.transform.position = MidPoint;
            }
        }



        // Checking if the user is trying to tie the string
        if (TiePoint1 != null)
        {
            // Debug.Log("Checking for a tie");
            if (TiePoint1.GetComponent<messenger>().EnteredMesh == true && pair.Item2 != null)
            {
                // Debug.Log("Tied Points touched, tie started");
                Tied = true;
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
        // Reset main thread renderer
        points.Clear();
        Tied = false;

        // Destory and reset all the tie points and cut point
        Destroy(CutPoint);
        CutPoint = null;

        Destroy(TiePoint1);
        TiePoint1 = null;
        Destroy(TiePoint2);
        TiePoint2 = null;

        // Reset hte line renderer
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
