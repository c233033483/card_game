using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Sensitivity")]
    public float sensitivity = 2f;

    [Header("Limits")]
    public float maxLookUp = 30f;
    public float maxLookDown = 30f;
    public float maxLookLeft = 60f;
    public float maxLookRight = 60f;

    [Header("Options")]
    public bool lockCursor = true;
    public bool returnToCenterOnRelease = false;

    [Header("Zoom")] 
    public Transform pileZoomPoint;
    public float zoomSpeed =5f;
    private Vector3 _startPosition;
    private bool _zoomedIn;
    
    private float _yaw;
    private float _pitch;
    private float _targetYaw;
    private float _targetPitch;
    private Quaternion _startRotation;
    private bool _movingCamera;
    
    void Start()
    {
        _startRotation = transform.localRotation;
        _startPosition = transform.position;
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(1))
            SetLooking(true);
        else if (Input.GetMouseButtonUp(1))
            SetLooking(false);
        
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        if (_movingCamera)
        {
            _targetYaw += mouseX;
            _targetPitch -= mouseY;

            _targetYaw = Mathf.Clamp(_targetYaw, -maxLookLeft, maxLookRight);
            _targetPitch = Mathf.Clamp(_targetPitch, -maxLookUp, maxLookDown);
        }
        else if (returnToCenterOnRelease)
        {
            _targetYaw = Mathf.Lerp(_targetYaw, 0f, 8 * Time.deltaTime);
            _targetPitch = Mathf.Lerp(_targetPitch, 0f, 8 * Time.deltaTime);
        }

        _yaw = _targetYaw;
        _pitch = _targetPitch;

        transform.localRotation = _startRotation * Quaternion.Euler(_pitch, _yaw, 0f);
        
        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0f) _zoomedIn = true;
        else if (scroll < 0f) _zoomedIn = false;

        Vector3 targetPos = (_zoomedIn && pileZoomPoint != null) ? pileZoomPoint.position : _startPosition;
        transform.position = Vector3.Lerp(transform.position, targetPos, zoomSpeed * Time.deltaTime);
    }
    
    private void SetLooking(bool on)
    {
        _movingCamera = on;
        Cursor.lockState = on ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !on;
    }
    
    private void OnDisable()
    {
        if (_movingCamera) SetLooking(false);
    }
}
