using UnityEngine;
using System.Collections.Generic;

public class ContinueThread : MonoBehaviour
{

    // List of all "threads"
    public List<GameObject> threads = new();

    // List of all thread points pairs
    public List<(Transform, Transform)> endPairs = new();

    // Initial thread variable
    public GameObject initialThread;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Start list of "threads" with initial thread
        if (initialThread != null)
        {
            endPairs.Add((initialThread.GetComponent<ThreadController>().startTransform,
                            initialThread.GetComponent<ThreadController>().endTransform));

            threads.Add(initialThread);
        }
        else
            Debug.Log("No initial thread");
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ApplyNewPoint()
    {
        // Create empty object at specified location

        // Set the end point of the previous thread to the new location

        // Make a new thread object
        // Set end point1 to the new empty object
        // Make the end point 2 to the head of the thread

        // Adjust length of thread segments if needed
    }
}
