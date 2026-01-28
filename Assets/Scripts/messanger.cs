using System;
using UnityEngine;

public class messenger : MonoBehaviour
{

    public Collider col;

    public String TargetTag;

    public bool EnteredMesh;

    public bool ExitedMesh;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        col = GetComponent<Collider>();
        if (TargetTag == null)
            Debug.Log("No Tag set");
        EnteredMesh = false;
        ExitedMesh = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == TargetTag)
            EnteredMesh = true;
        Debug.Log(gameObject.name + " touched by: " + other.tag);
        // ExitedMesh = false;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == TargetTag)
            ExitedMesh = true;
        Debug.Log(gameObject.name + " touched by: " + other.tag);
        // EnteredMesh = false;
    }
}
