using UnityEngine;
using UnityEngine.EventSystems;
 
public class RTSCameraController : MonoBehaviour
{
    public static RTSCameraController instance;
 
    [Header("General")]
    [SerializeField] Transform cameraTransform;
    public Transform followTransform;
    Vector3 newPosition;
    Vector3 dragStartPosition;
    Vector3 dragCurrentPosition;
    float defaultY; // Stores the parent rig's baseline Y coordinate

    [Header("Collision & Terrain Bounds")]
    [Tooltip("Select BOTH your Terrain layer and your 'terrain assets' layer here.")]
    [SerializeField] LayerMask obstacleLayers; 
    [SerializeField] float cameraCollisionRadius = 1.5f;
    [SerializeField] float minHeightAboveTerrain = 5f;
    [SerializeField] float maxRaycastHeight = 200f;
 
    [Header("Optional Functionality")]
    [SerializeField] bool moveWithKeyboad;
    [SerializeField] bool moveWithEdgeScrolling;
    [SerializeField] bool moveWithMouseDrag;
 
    [Header("Keyboard Movement")]
    [SerializeField] float fastSpeed = 0.05f;
    [SerializeField] float normalSpeed = 0.01f;
    [SerializeField] float movementSensitivity = 1f;
    float movementSpeed;
 
    [Header("Edge Scrolling Movement")]
    [SerializeField] float edgeSize = 50f;
    bool isCursorSet = false;
    public Texture2D cursorArrowUp;
    public Texture2D cursorArrowDown;
    public Texture2D cursorArrowLeft;
    public Texture2D cursorArrowRight;
 
    CursorArrow currentCursor = CursorArrow.DEFAULT;
    enum CursorArrow
    {
        UP,
        DOWN,
        LEFT,
        RIGHT,
        DEFAULT
    }
 
    private void Start()
    {
        instance = this;
        newPosition = transform.position;
        defaultY = transform.position.y; // Record the initial Y level to return to
        movementSpeed = normalSpeed;
    }
 
    private void Update()
    {
        // Allow Camera to follow Target
        if (followTransform != null)
        {
            newPosition = followTransform.position;
            ApplyCollisionAndTerrainBounds();
            transform.position = newPosition;
        }
        // Let us control Camera
        else
        {
            HandleCameraMovement();
        }
 
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            followTransform = null;
        }
    }
 
    void HandleCameraMovement()
    {
        // Reset target Y to the baseline height each frame so it settles back down when space opens up
        newPosition.y = defaultY;

        // Mouse Drag
        if (moveWithMouseDrag)
        {
            HandleMouseDragInput();
        }
 
        // Keyboard Control
        if (moveWithKeyboad)
        {
            if (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.LeftShift))
            {
                movementSpeed = fastSpeed;
            }
            else
            {
                movementSpeed = normalSpeed;
            }
 
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                newPosition += (transform.forward * movementSpeed);
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                newPosition += (transform.forward * -movementSpeed);
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                newPosition += (transform.right * movementSpeed);
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                newPosition += (transform.right * -movementSpeed);
            }
        }
 
        // Edge Scrolling
        if (moveWithEdgeScrolling)
        {
            if (Input.mousePosition.x > Screen.width - edgeSize)
            {
                newPosition += (transform.right * movementSpeed);
                ChangeCursor(CursorArrow.RIGHT);
                isCursorSet = true;
            }
            else if (Input.mousePosition.x < edgeSize)
            {
                newPosition += (transform.right * -movementSpeed);
                ChangeCursor(CursorArrow.LEFT);
                isCursorSet = true;
            }
            else if (Input.mousePosition.y > Screen.height - edgeSize)
            {
                newPosition += (transform.forward * movementSpeed);
                ChangeCursor(CursorArrow.UP);
                isCursorSet = true;
            }
            else if (Input.mousePosition.y < edgeSize)
            {
                newPosition += (transform.forward * -movementSpeed);
                ChangeCursor(CursorArrow.DOWN);
                isCursorSet = true;
            }
            else
            {
                if (isCursorSet)
                {
                    ChangeCursor(CursorArrow.DEFAULT);
                    isCursorSet = false;
                }
            }
        }

        // Push up over geometry if needed, or let it drop back down to defaultY
        ApplyCollisionAndTerrainBounds();
 
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementSensitivity);
 
        Cursor.lockState = CursorLockMode.Confined;
    }

    private void ApplyCollisionAndTerrainBounds()
    {
        // 1. Horizontal Obstacle Check (Prevents passing horizontally through rocks, trees, and steep hills)
        Vector3 moveVector = newPosition - transform.position;
        float moveDistance = moveVector.magnitude;

        if (moveDistance > 0.001f)
        {
            if (Physics.SphereCast(transform.position, cameraCollisionRadius, moveVector.normalized, out RaycastHit hit, moveDistance, obstacleLayers, QueryTriggerInteraction.Ignore))
            {
                newPosition = transform.position + (moveVector.normalized * hit.distance);
            }
        }

        // 2. Vertical Geometry Check (Pushes Y upward over terrain/assets, otherwise stays at defaultY)
        Vector3 rayOrigin = new Vector3(newPosition.x, newPosition.y + maxRaycastHeight, newPosition.z);
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit groundHit, maxRaycastHeight * 2f, obstacleLayers, QueryTriggerInteraction.Ignore))
        {
            float minimumY = groundHit.point.y + minHeightAboveTerrain;
            if (newPosition.y < minimumY)
            {
                newPosition.y = minimumY;
            }
        }
    }
 
    private void ChangeCursor(CursorArrow newCursor)
    {
        if (currentCursor != newCursor)
        {
            switch (newCursor)
            {
                case CursorArrow.UP:
                    Cursor.SetCursor(cursorArrowUp, Vector2.zero, CursorMode.Auto);
                    break;
                case CursorArrow.DOWN:
                    Cursor.SetCursor(cursorArrowDown, new Vector2(cursorArrowDown.width, cursorArrowDown.height), CursorMode.Auto);
                    break;
                case CursorArrow.LEFT:
                    Cursor.SetCursor(cursorArrowLeft, Vector2.zero, CursorMode.Auto);
                    break;
                case CursorArrow.RIGHT:
                    Cursor.SetCursor(cursorArrowRight, new Vector2(cursorArrowRight.width, cursorArrowRight.height), CursorMode.Auto);
                    break;
                case CursorArrow.DEFAULT:
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    break;
            }
 
            currentCursor = newCursor;
        }
    }
 
    private void HandleMouseDragInput()
    {
        if (Input.GetMouseButtonDown(2) && EventSystem.current.IsPointerOverGameObject() == false)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
 
            if (plane.Raycast(ray, out float entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }
        if (Input.GetMouseButton(2) && EventSystem.current.IsPointerOverGameObject() == false)
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
 
            if (plane.Raycast(ray, out float entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);
                newPosition = transform.position + dragStartPosition - dragCurrentPosition;
                newPosition.y = defaultY; // Ensure mouse drag preserves the baseline Y target
            }
        }
    }
}