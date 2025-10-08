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
    // Points for the line renderer
    public List<Vector3> points;

    // Bool to know when a pair is starting // Future use
    private bool startingSuturePair = false;

    // Holds a potential point to add
    public Vector3 potentialPoint;

    public List<GameObject> threads;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // if (needlePoint == null || thread == null)
        //     Debug.Log("One of the objects is empty");


        // Make a new points list
        points = new();

        // Make a new threads list
        threads = new();

        // Set up settings on the line renderer
        threadRenderer = GetComponent<LineRenderer>();
        threadRenderer.startWidth = 0.002f;
        threadRenderer.endWidth = 0.002f;
        Debug.Log("Set line renderer settings");

        // Make the first two points of the line renderer
        threadRenderer.positionCount = 2;
        threadRenderer.SetPosition(0, threadSource.transform.position);
        points.Add(threadSource.transform.position);
        threadRenderer.SetPosition(1, needleThreadPoint.transform.position);
        points.Add(needleThreadPoint.transform.position);
        Debug.Log("Line renderer has: " + threadRenderer.positionCount + " points");
    }

    // Update is called once per frame
    void Update()
    {
        // Set each position of the line renderer to the poinst list.
        for (int i = 0; i < points.Count; i++)
        {
            threadRenderer.SetPosition(i, points[i]);
        }



        // if (needleThreadPoint.GetComponent<messanger>().EnteredMesh == true)
        // {
        //     if (startingSuturePair == false)
        //     {
        //         startingSuturePair = true;
        //         potentialPoint = needleThreadPoint.transform.position;
        //     }
        //     else
        //     {
        //         potentialPoint = needleThreadPoint.transform.position;
        //     }

        //     needleThreadPoint.GetComponent<messanger>().EnteredMesh = false;
        // }
        // else if (needleThreadPoint.GetComponent<messanger>().ExitedMesh == true)
        // {
        //     if (startingSuturePair == true)
        //     {
        //         points.Insert(points.Count - 1, potentialPoint);
        //         threadRenderer.positionCount++;
        //         potentialPoint = Vector3.zero;

        //         // Do something about making suture pairs in the future.
        //         startingSuturePair = false;
        //     }
        //     else
        //     {
        //         points.Insert(points.Count - 1, potentialPoint);
        //         threadRenderer.positionCount++;
        //         potentialPoint = Vector3.zero;
        //     }
        //     needleThreadPoint.GetComponent<messanger>().ExitedMesh = false;
        // }


        if (needleThreadPoint.GetComponent<messanger>().EnteredMesh == true)
        {
            if (startingSuturePair == false)
            {
                startingSuturePair = true;
            }

            potentialPoint = needleThreadPoint.transform.position;

            needleThreadPoint.GetComponent<messanger>().EnteredMesh = false;
        }
        else if (needleThreadPoint.GetComponent<messanger>().ExitedMesh == true)
        {
            if (startingSuturePair == true)
            {
                // Do something about making suture pairs in the future.
                startingSuturePair = false;
            }

            points.Insert(points.Count - 1, potentialPoint);
            threadRenderer.positionCount++;
            potentialPoint = Vector3.zero;

            AddThread(points[points.Count - 2], points[points.Count - 1]);
            Debug.Log("Adding thread");

            needleThreadPoint.GetComponent<messanger>().ExitedMesh = false;
        }


        // Alays make the last point the position of the needleThreadPoint
        points[points.Count - 1] = needleThreadPoint.transform.position;
        // Always make the first point the position of the thread source
        points[0] = threadSource.transform.position;
    }

    void AddThread(Vector3 point1, Vector3 point2)
    {
        GameObject thrd = new();
        thrd.AddComponent<ThreadController>();

        thrd.GetComponent<ThreadController>().ropeRadius = threadRenderer.startWidth;

        GameObject strt = new();
        strt.transform.position = point1;

        GameObject end = new();
        strt.transform.position = point2;

        thrd.GetComponent<ThreadController>().startTransform = strt.transform;
        thrd.GetComponent<ThreadController>().endTransform = end.transform;

        thrd.GetComponent<ThreadController>().segmentCount = (int)(Vector3.Distance(point1, point2) / thrd.GetComponent<ThreadController>().segmentLength);

        // Add rigid bodies and make them kinematic so that they don't move ////////////////////////
        
        // strt.GetComponent<Rigidbody>().isKinematic = true;
        // end.GetComponent<Rigidbody>().isKinematic = true;

        threads.Add(thrd);
    }
}
