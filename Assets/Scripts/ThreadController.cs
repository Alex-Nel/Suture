using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class ThreadController : MonoBehaviour
{
    public Transform startTransform;
    public Transform endTransform;
    public int segmentCount = 15;
    public float segmentLength = 0.2f;
    public float ropeRadius = 0.05f;
    public Material ropeMaterial;

    private List<Transform> segmentList = new List<Transform>();
    private LineRenderer lineRenderer;

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
        Transform prev = startTransform;

        for (int i = 0; i < segmentCount; i++)
        {
            GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            // GameObject capsule = new GameObject("segment");

            capsule.transform.localScale = new Vector3(ropeRadius * 2f, segmentLength * 1.5f, ropeRadius * 2f);

            Vector3 pos = Vector3.Lerp(startTransform.position, endTransform.position, (i + 1f) / (segmentCount + 1f));
            capsule.transform.position = pos;

            Vector3 direction = (pos - prev.position).normalized;
            if (direction != Vector3.zero)
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

            MeshRenderer rend = capsule.GetComponent<MeshRenderer>();
            if (rend != null)
                Destroy(rend);

            MeshFilter filt = capsule.GetComponent<MeshFilter>();
            if (filt != null)
                Destroy(filt);

            capsule.AddComponent<RopeGrabber>(); // <-- changed from Grabbable

            segmentList.Add(capsule.transform);
            prev = capsule.transform;
        }

        SetupSegment(endTransform, false);

        ConfigurableJoint endJoint = endTransform.gameObject.AddComponent<ConfigurableJoint>();
        endJoint.connectedBody = prev.GetComponent<Rigidbody>();
        endJoint.autoConfigureConnectedAnchor = false;
        endJoint.anchor = Vector3.zero;
        endJoint.connectedAnchor = Vector3.zero;
        endJoint.xMotion = endJoint.yMotion = endJoint.zMotion = ConfigurableJointMotion.Limited;
        endJoint.linearLimit = new SoftJointLimit { limit = segmentLength };
        endJoint.angularXMotion = ConfigurableJointMotion.Locked;
        endJoint.angularYMotion = ConfigurableJointMotion.Locked;
        endJoint.angularZMotion = ConfigurableJointMotion.Locked;

        
        if (startTransform.parent != null)
            startTransform.gameObject.GetComponent<Rigidbody>().isKinematic = false;

        SetupLineRenderer();
        SetupMeshComponents();
    }

    void SetupSegment(Transform t, bool isKinematic)
    {
        Rigidbody rb = t.GetComponent<Rigidbody>();
        if (rb == null)
            rb = t.gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = isKinematic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (ropeMaterial != null)
        {
            Renderer r = t.GetComponent<Renderer>();
            if (r != null)
                r.material = ropeMaterial;
        }

        t.gameObject.AddComponent<RopeGrabber>(); // <-- changed from Grabbable
    }

    void SetupLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = ropeMaterial;
        lineRenderer.positionCount = segmentCount + 2;
        lineRenderer.widthMultiplier = ropeRadius * 2f;
    }

    void SetupMeshComponents()
    {
        GameObject meshObj = new GameObject("RopeMesh", typeof(MeshFilter), typeof(MeshRenderer));
        // GameObject meshObj = new GameObject("RopeMesh");
        meshObj.transform.SetParent(this.transform);

        meshFilter = meshObj.GetComponent<MeshFilter>();
        meshRenderer = meshObj.GetComponent<MeshRenderer>();
        meshRenderer.material = ropeMaterial;

        ropeMesh = new Mesh();
        meshFilter.mesh = ropeMesh;

        MeshRenderer rendr = GetComponent<MeshRenderer>();
        if (rendr != null)
            Destroy(rendr);

        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter != null)
            Destroy(filter);

    }

    void Update()
    {
        if (segmentList.Count == 0) return;

        List<Vector3> controlPoints = new List<Vector3> { startTransform.position };
        foreach (Transform t in segmentList)
            controlPoints.Add(t.position);
        controlPoints.Add(endTransform.position);

        List<Vector3> smoothPoints = CatmullRom.GenerateSpline(controlPoints.ToArray(), 8);
        lineRenderer.positionCount = smoothPoints.Count;
        lineRenderer.SetPositions(smoothPoints.ToArray());

        // GenerateTubeMesh(smoothPoints);
    }

    void GenerateTubeMesh(List<Vector3> points, int radialSegments = 6, float radius = 0.05f)
    {
        if (points.Count < 2)
            return;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        List<Vector3> tangents = new List<Vector3>();
        for (int i = 0; i < points.Count - 1; i++)
            tangents.Add((points[i + 1] - points[i]).normalized);
        tangents.Add(tangents[tangents.Count - 1]);

        Vector3 arbitraryUp = Vector3.up;
        if (Mathf.Abs(Vector3.Dot(arbitraryUp, tangents[0])) > 0.99f)
            arbitraryUp = Vector3.forward;

        List<Vector3> normals = new List<Vector3>();
        List<Vector3> binormals = new List<Vector3>();

        normals.Add(Vector3.Cross(tangents[0], arbitraryUp).normalized);
        binormals.Add(Vector3.Cross(tangents[0], normals[0]).normalized);

        for (int i = 1; i < points.Count; i++)
        {
            Vector3 v = tangents[i - 1];
            Vector3 w = tangents[i];
            float cosTheta = Vector3.Dot(v, w);

            if (cosTheta > 0.9995f)
            {
                normals.Add(normals[i - 1]);
                binormals.Add(binormals[i - 1]);
            }
            else
            {
                Vector3 axis = Vector3.Cross(v, w).normalized;
                float angle = Mathf.Acos(Mathf.Clamp(cosTheta, -1f, 1f));
                Quaternion rot = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis);

                normals.Add(rot * normals[i - 1]);
                binormals.Add(rot * binormals[i - 1]);
            }
        }

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 center = points[i];
            Vector3 normal = normals[i];
            Vector3 binormal = binormals[i];

            for (int j = 0; j < radialSegments; j++)
            {
                float angle = 2 * Mathf.PI * j / radialSegments;
                Vector3 localPos = normal * Mathf.Cos(angle) * radius + binormal * Mathf.Sin(angle) * radius;
                vertices.Add(center + localPos);
                uvs.Add(new Vector2(j / (float)(radialSegments - 1), i / (float)(points.Count - 1)));
            }
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int curr = i * radialSegments + j;
                int next = curr + radialSegments;

                int nextJ = (j + 1) % radialSegments;
                int currNext = i * radialSegments + nextJ;
                int nextNext = currNext + radialSegments;

                triangles.Add(curr);
                triangles.Add(next);
                triangles.Add(currNext);

                triangles.Add(currNext);
                triangles.Add(next);
                triangles.Add(nextNext);
            }
        }

        ropeMesh.Clear();
        ropeMesh.vertices = vertices.ToArray();
        ropeMesh.triangles = triangles.ToArray();
        ropeMesh.uv = uvs.ToArray();
        ropeMesh.RecalculateNormals();
    }
}
