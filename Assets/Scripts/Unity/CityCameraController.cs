using UnityEngine;

namespace CityBuilder.Unity
{
    /// <summary>
    /// RTS/city-builder camera controller (Cities-style): pan, rotate and zoom.
    /// Attach to Main Camera.
    /// </summary>
    public sealed class CityCameraController : MonoBehaviour
    {
        [Header("Pan")]
        [SerializeField] private float panSpeed = 35f;
        [SerializeField] private float edgePanBorder = 12f;
        [SerializeField] private bool enableEdgePan = true;

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 250f;
        [SerializeField] private float minDistance = 20f;
        [SerializeField] private float maxDistance = 120f;

        [Header("Rotation")]
        [SerializeField] private float rotationSpeed = 130f;
        [SerializeField] private float minPitch = 25f;
        [SerializeField] private float maxPitch = 80f;

        [Header("Bounds")]
        [SerializeField] private Vector2 xBounds = new(-20f, 80f);
        [SerializeField] private Vector2 zBounds = new(-20f, 80f);

        [Header("Target")]
        [SerializeField] private Vector3 pivot = new(25f, 0f, 25f);

        private float _yaw = 45f;
        private float _pitch = 55f;
        private float _distance = 55f;

        private void Start()
        {
            var euler = transform.rotation.eulerAngles;
            _yaw = euler.y;
            _pitch = NormalizePitch(euler.x);
            _distance = Mathf.Clamp(Vector3.Distance(transform.position, pivot), minDistance, maxDistance);
            ApplyTransform();
        }

        private void Update()
        {
            HandlePan();
            HandleRotate();
            HandleZoom();
            ClampPivot();
            ApplyTransform();
        }

        private void HandlePan()
        {
            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");

            if (enableEdgePan)
            {
                if (Input.mousePosition.x <= edgePanBorder)
                {
                    horizontal -= 1f;
                }
                else if (Input.mousePosition.x >= Screen.width - edgePanBorder)
                {
                    horizontal += 1f;
                }

                if (Input.mousePosition.y <= edgePanBorder)
                {
                    vertical -= 1f;
                }
                else if (Input.mousePosition.y >= Screen.height - edgePanBorder)
                {
                    vertical += 1f;
                }
            }

            if (Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f))
            {
                return;
            }

            var flattenedForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            var flattenedRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

            var move = (flattenedRight * horizontal + flattenedForward * vertical).normalized;
            var scale = panSpeed * Time.deltaTime * Mathf.Lerp(0.55f, 1.25f, Mathf.InverseLerp(maxDistance, minDistance, _distance));

            pivot += move * scale;
        }

        private void HandleRotate()
        {
            if (Input.GetMouseButton(2))
            {
                _yaw += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
                _pitch -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.Q))
            {
                _yaw -= rotationSpeed * 0.55f * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.E))
            {
                _yaw += rotationSpeed * 0.55f * Time.deltaTime;
            }

            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }

        private void HandleZoom()
        {
            var wheel = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Approximately(wheel, 0f))
            {
                return;
            }

            _distance -= wheel * zoomSpeed * Time.deltaTime;
            _distance = Mathf.Clamp(_distance, minDistance, maxDistance);
        }

        private void ApplyTransform()
        {
            var rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            var offset = rotation * new Vector3(0f, 0f, -_distance);

            transform.position = pivot + offset;
            transform.rotation = rotation;
        }

        private void ClampPivot()
        {
            pivot.x = Mathf.Clamp(pivot.x, xBounds.x, xBounds.y);
            pivot.z = Mathf.Clamp(pivot.z, zBounds.x, zBounds.y);
        }

        private static float NormalizePitch(float pitch)
        {
            return pitch > 180f ? pitch - 360f : pitch;
        }
    }
}
