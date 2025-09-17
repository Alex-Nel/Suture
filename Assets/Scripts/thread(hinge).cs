using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ThreadHinge : MonoBehaviour
{
    [Header("Endpoints")]
    public Transform startTransform;
    public Transform endTransform;

    [Header("Rope Settings")]
    public int segmentCount = 20;
    public float totalLength = 0.5f;
    public float totalWeight = 1f;
    public float drag = 0.2f;
    public float angularDrag = 0.1f;

    [Header("Visuals")]
    public Material ropeMaterial;
    public float ropeWidth = 0.05f;

    private Transform[] segments;
    private LineRenderer lineRenderer;
    private Rigidbody grabbedRigidbody;

    void Start()
    {
        SetupRigidbody(startTransform, true);   // Kinematic, we drag this one
        SetupRigidbody(endTransform, false);    // Free to move, physics-based
        CreateSegments();
        SetupLineRenderer();
    }

    void Update()
    {
        HandleMouseInput();
        UpdateLineRenderer();
    }

    void CreateSegments()
    {
        segments = new Transform[segmentCount];
        float segmentLength = totalLength / (segmentCount + 1);
        Vector3 direction = (endTransform.position - startTransform.position).normalized;

        Rigidbody previousRB = startTransform.GetComponent<Rigidbody>();

        for (int i = 0; i < segmentCount; i++)
        {
            GameObject segObj = new GameObject($"Segment_{i}");
            segObj.name = $"Segment_{i}";
            // segObj.transform.localScale = new Vector3(ropeWidth, segmentLength / 2f, ropeWidth);
            // segObj.transform.rotation = Quaternion.LookRotation(direction);
            segObj.transform.position = Vector3.Lerp(startTransform.position, endTransform.position, (i + 1f) / (segmentCount + 1f));

            // segObj.AddComponent<MeshRenderer>();
            // segObj.AddComponent<MeshFilter>();
            // segObj.GetComponent<MeshFilter>().mesh = Resources.GetBuiltinResource<Mesh>("Cylinder.fbx");

            Rigidbody rb = segObj.AddComponent<Rigidbody>();
            rb.mass = totalWeight / segmentCount;
            rb.linearDamping = drag;
            rb.angularDamping = angularDrag;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            CharacterJoint joint = segObj.AddComponent<CharacterJoint>();
            joint.connectedBody = previousRB;
            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = Vector3.zero;
            joint.connectedAnchor = new Vector3(0f, 0.3f, 0f);
            joint.anchor = new Vector3(0f, 0.5f, 0f);
            joint.axis = new Vector3(1f, 0f, 0f);
            joint.swing1Limit = new SoftJointLimit{limit = 175f};

            // joint.xMotion = ConfigurableJointMotion.Limited;
            // joint.yMotion = ConfigurableJointMotion.Limited;
            // joint.zMotion = ConfigurableJointMotion.Limited;


            // SoftJointLimit limit = new SoftJointLimit { limit = segmentLength * 0.5f };
            // joint.linearLimit = limit;

            previousRB = rb;
            segments[i] = segObj.transform;
        }

        // Connect last segment to endTransform
        HingeJoint finalJoint = endTransform.gameObject.AddComponent<HingeJoint>();
        finalJoint.connectedBody = previousRB;
        finalJoint.autoConfigureConnectedAnchor = false;
        finalJoint.anchor = Vector3.zero;
        finalJoint.connectedAnchor = Vector3.zero;
        // finalJoint.xMotion = ConfigurableJointMotion.Limited;
        // finalJoint.yMotion = ConfigurableJointMotion.Limited;
        // finalJoint.zMotion = ConfigurableJointMotion.Limited;
        // finalJoint.linearLimit = new SoftJointLimit { limit = segmentLength * 0.5f };
    }

    Rigidbody SetupRigidbody(Transform t, bool isKinematic)
    {
        Rigidbody rb = t.GetComponent<Rigidbody>();
        if (rb == null)
            rb = t.gameObject.AddComponent<Rigidbody>();

        rb.mass = totalWeight / (segmentCount + 2);
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        rb.useGravity = true;
        rb.isKinematic = isKinematic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        return rb;
    }

    void SetupLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segmentCount + 2;
        lineRenderer.startWidth = ropeWidth;
        lineRenderer.endWidth = ropeWidth;
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.black;
        lineRenderer.endColor = Color.black;
    }

    void UpdateLineRenderer()
    {
        if (lineRenderer == null || segments == null) return;

        lineRenderer.SetPosition(0, startTransform.position);
        for (int i = 0; i < segmentCount; i++)
        {
            if (segments[i] != null)
                lineRenderer.SetPosition(i + 1, segments[i].position);
        }
        lineRenderer.SetPosition(segmentCount + 1, endTransform.position);
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.transform == startTransform)
                {
                    grabbedRigidbody = startTransform.GetComponent<Rigidbody>();
                    grabbedRigidbody.isKinematic = false; // Allow dragging
                }
            }
        }

        if (Input.GetMouseButton(0) && grabbedRigidbody != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, startTransform.position.y);
            if (plane.Raycast(ray, out float distance))
            {
                Vector3 worldPos = ray.GetPoint(distance);
                grabbedRigidbody.MovePosition(worldPos);
            }
        }

        if (Input.GetMouseButtonUp(0) && grabbedRigidbody != null)
        {
            grabbedRigidbody.isKinematic = true; // Lock again after drag
            grabbedRigidbody = null;
        }
    }
}
