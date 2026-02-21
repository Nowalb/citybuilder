using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CityBuilder.Unity
{
    /// <summary>
    /// RTS/city-builder camera controller (Cities-style): pan, rotate and zoom.
    /// Works with both the old Input Manager and the new Input System package.
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
            var horizontal = GetHorizontalAxis();
            var vertical = GetVerticalAxis();
            var mousePosition = GetMousePosition();

            if (enableEdgePan)
            {
                if (mousePosition.x <= edgePanBorder)
                {
                    horizontal -= 1f;
                }
                else if (mousePosition.x >= Screen.width - edgePanBorder)
                {
                    horizontal += 1f;
                }

                if (mousePosition.y <= edgePanBorder)
                {
                    vertical -= 1f;
                }
                else if (mousePosition.y >= Screen.height - edgePanBorder)
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
            if (IsMiddleMousePressed())
            {
                _yaw += GetMouseDeltaX() * rotationSpeed * Time.deltaTime;
                _pitch -= GetMouseDeltaY() * rotationSpeed * Time.deltaTime;
            }

            if (IsRotateLeftPressed())
            {
                _yaw -= rotationSpeed * 0.55f * Time.deltaTime;
            }
            else if (IsRotateRightPressed())
            {
                _yaw += rotationSpeed * 0.55f * Time.deltaTime;
            }

            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
        }

        private void HandleZoom()
        {
            var wheel = GetMouseScroll();
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

#if ENABLE_INPUT_SYSTEM
        private static float GetHorizontalAxis()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return 0f;
            var value = 0f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) value -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) value += 1f;
            return value;
        }

        private static float GetVerticalAxis()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return 0f;
            var value = 0f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) value -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) value += 1f;
            return value;
        }

        private static Vector2 GetMousePosition()
        {
            return Mouse.current?.position.ReadValue() ?? Vector2.zero;
        }

        private static bool IsMiddleMousePressed()
        {
            return Mouse.current?.middleButton.isPressed ?? false;
        }

        private static float GetMouseDeltaX()
        {
            return Mouse.current?.delta.ReadValue().x ?? 0f;
        }

        private static float GetMouseDeltaY()
        {
            return Mouse.current?.delta.ReadValue().y ?? 0f;
        }

        private static float GetMouseScroll()
        {
            return (Mouse.current?.scroll.ReadValue().y ?? 0f) * 0.01f;
        }

        private static bool IsRotateLeftPressed()
        {
            return Keyboard.current?.qKey.isPressed ?? false;
        }

        private static bool IsRotateRightPressed()
        {
            return Keyboard.current?.eKey.isPressed ?? false;
        }
#else
        private static float GetHorizontalAxis() => Input.GetAxisRaw("Horizontal");
        private static float GetVerticalAxis() => Input.GetAxisRaw("Vertical");
        private static Vector2 GetMousePosition() => Input.mousePosition;
        private static bool IsMiddleMousePressed() => Input.GetMouseButton(2);
        private static float GetMouseDeltaX() => Input.GetAxis("Mouse X");
        private static float GetMouseDeltaY() => Input.GetAxis("Mouse Y");
        private static float GetMouseScroll() => Input.GetAxis("Mouse ScrollWheel");
        private static bool IsRotateLeftPressed() => Input.GetKey(KeyCode.Q);
        private static bool IsRotateRightPressed() => Input.GetKey(KeyCode.E);
#endif
    }
}
