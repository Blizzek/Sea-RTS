using UnityEngine;

namespace SeaRTS.Core
{
    /// <summary>
    /// RTS-style camera controller with panning, zooming, and edge scrolling.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float panSpeed = 20f;
        [SerializeField] private float panBorderThickness = 10f;
        [SerializeField] private bool useEdgeScrolling = true;

        [Header("Zoom Settings")]
        [SerializeField] private float zoomSpeed = 10f;
        [SerializeField] private float minZoom = 10f;
        [SerializeField] private float maxZoom = 80f;

        [Header("Bounds")]
        [SerializeField] private Vector2 minBounds = new Vector2(-100f, -100f);
        [SerializeField] private Vector2 maxBounds = new Vector2(100f, 100f);

        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private bool allowRotation = true;

        private Camera mainCamera;
        private Vector3 lastMousePosition;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            HandleMovement();
            HandleZoom();
            HandleRotation();
            ClampPosition();
        }

        private void HandleMovement()
        {
            Vector3 movement = Vector3.zero;

            // Keyboard input
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                movement += transform.forward;
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                movement -= transform.forward;
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                movement -= transform.right;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                movement += transform.right;
            }

            // Edge scrolling
            if (useEdgeScrolling)
            {
                Vector3 mousePos = Input.mousePosition;

                if (mousePos.y >= Screen.height - panBorderThickness)
                {
                    movement += transform.forward;
                }
                if (mousePos.y <= panBorderThickness)
                {
                    movement -= transform.forward;
                }
                if (mousePos.x >= Screen.width - panBorderThickness)
                {
                    movement += transform.right;
                }
                if (mousePos.x <= panBorderThickness)
                {
                    movement -= transform.right;
                }
            }

            // Middle mouse drag
            if (Input.GetMouseButton(2))
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                movement -= transform.right * delta.x * 0.01f;
                movement -= transform.forward * delta.y * 0.01f;
            }

            movement.y = 0;
            transform.position += movement.normalized * panSpeed * Time.deltaTime;

            lastMousePosition = Input.mousePosition;
        }

        private void HandleZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll != 0)
            {
                Vector3 position = transform.position;
                position.y -= scroll * zoomSpeed;
                position.y = Mathf.Clamp(position.y, minZoom, maxZoom);
                transform.position = position;
            }
        }

        private void HandleRotation()
        {
            if (!allowRotation) return;

            // Q and E for rotation
            if (Input.GetKey(KeyCode.Q))
            {
                transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime, Space.World);
            }
            if (Input.GetKey(KeyCode.E))
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            }
        }

        private void ClampPosition()
        {
            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
            position.z = Mathf.Clamp(position.z, minBounds.y, maxBounds.y);
            transform.position = position;
        }

        /// <summary>
        /// Moves the camera to focus on a specific position.
        /// </summary>
        public void FocusOn(Vector3 position)
        {
            Vector3 newPosition = position;
            newPosition.y = transform.position.y;
            transform.position = newPosition;
        }

        /// <summary>
        /// Sets the camera bounds.
        /// </summary>
        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
        }

        /// <summary>
        /// Enables or disables edge scrolling.
        /// </summary>
        public void SetEdgeScrolling(bool enabled)
        {
            useEdgeScrolling = enabled;
        }
    }
}
