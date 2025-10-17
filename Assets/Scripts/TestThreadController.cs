using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class TestThreadController : MonoBehaviour
{
    public Transform startTransform;
    public Transform endTransform;
    public float segmentLength = 0.2f;
    public float ropeRadius = 0.05f;
    public Material ropeMaterial;
    public int maxSegments = 100;
    public int minSegments = 1;

    private List<Transform> segmentList = new List<Transform>();
    private LineRenderer lineRenderer;
    private int currentSegmentCount = 0;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh ropeMesh;

    void Start()
    {
        if (!startTransform || !endTransform)
        {
            Debug.LogError("Start or End Transform not assigned.");
            return;
        }

        SetupSegment(startTransform, true);
        SetupSegment(endTransform, false);

        endTransform.GetComponent<Rigidbody>().isKinematic = true;

        SetupLineRenderer();
        SetupMeshComponents();
    }

    void Update()
    {
        float currentLength = Vector3.Distance(startTransform.position, endTransform.position);
        int targetSegmentCount = Mathf.Clamp(Mathf.FloorToInt(currentLength / segmentLength), minSegments, maxSegments);

        if (targetSegmentCount != currentSegmentCount)
        {
            UpdateSegments(targetSegmentCount);
            currentSegmentCount = targetSegmentCount;
        }

        List<Vector3> controlPoints = new List<Vector3> { startTransform.position };
        foreach (Transform t in segmentList)
            controlPoints.Add(t.position);
        controlPoints.Add(endTransform.position);

        List<Vector3> smoothPoints = CatmullRom.GenerateSpline(controlPoints.ToArray(), 8);
        lineRenderer.positionCount = smoothPoints.Count;
        lineRenderer.SetPositions(smoothPoints.ToArray());
    }

    void SetupSegment(Transform t, bool isKinematic)
    {
        Rigidbody rb = t.GetComponent<Rigidbody>();
        if (rb == null)
            rb = t.gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = isKinematic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        t.gameObject.AddComponent<RopeGrabber>(); // Replace with your grab logic

        if (ropeMaterial != null)
        {
            Renderer r = t.GetComponent<Renderer>();
            if (r != null)
                r.material = ropeMaterial;
        }
    }

    void SetupLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = ropeMaterial;
        lineRenderer.widthMultiplier = ropeRadius * 2f;
    }

    void SetupMeshComponents()
    {
        GameObject meshObj = new GameObject("RopeMesh", typeof(MeshFilter), typeof(MeshRenderer));
        meshObj.transform.SetParent(this.transform);

        meshFilter = meshObj.GetComponent<MeshFilter>();
        meshRenderer = meshObj.GetComponent<MeshRenderer>();
        meshRenderer.material = ropeMaterial;

        ropeMesh = new Mesh();
        meshFilter.mesh = ropeMesh;

        if (GetComponent<MeshRenderer>() != null)
            Destroy(GetComponent<MeshRenderer>());
        if (GetComponent<MeshFilter>() != null)
            Destroy(GetComponent<MeshFilter>());
    }

    void UpdateSegments(int targetCount)
    {
        while (segmentList.Count < targetCount)
            AddSegment();

        while (segmentList.Count > targetCount)
            RemoveLastSegment();

        UpdateEndJoint();
    }

    void AddSegment()
    {
        Transform prev = segmentList.Count > 0 ? segmentList[segmentList.Count - 1] : startTransform;

        GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.tag = "Thread";
        capsule.transform.localScale = new Vector3(ropeRadius * 2f, ropeRadius * 1.5f, ropeRadius * 2f);

        Vector3 pos = Vector3.Lerp(startTransform.position, endTransform.position, (segmentList.Count + 1f) / (currentSegmentCount + 1f));
        capsule.transform.position = pos;

        Vector3 direction = (pos - prev.position).normalized;
        capsule.transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);

        Rigidbody rb = capsule.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        ConfigurableJoint joint = capsule.AddComponent<ConfigurableJoint>();
        joint.connectedBody = prev.GetComponent<Rigidbody>();
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = Vector3.zero;

        joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Limited;
        joint.linearLimit = new SoftJointLimit { limit = segmentLength };

        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;

        Destroy(capsule.GetComponent<MeshRenderer>());
        Destroy(capsule.GetComponent<MeshFilter>());

        capsule.AddComponent<RopeGrabber>(); // Replace if needed

        segmentList.Add(capsule.transform);
    }

    void RemoveLastSegment()
    {
        Transform last = segmentList[segmentList.Count - 1];
        Destroy(last.gameObject);
        segmentList.RemoveAt(segmentList.Count - 1);
    }

    void UpdateEndJoint()
    {
        ConfigurableJoint joint = endTransform.GetComponent<ConfigurableJoint>();
        if (joint == null)
            joint = endTransform.gameObject.AddComponent<ConfigurableJoint>();

        if (segmentList.Count == 0) return;

        joint.connectedBody = segmentList[segmentList.Count - 1].GetComponent<Rigidbody>();
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = Vector3.zero;

        joint.xMotion = joint.yMotion = joint.zMotion = ConfigurableJointMotion.Limited;
        joint.linearLimit = new SoftJointLimit { limit = segmentLength };

        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;
    }
}