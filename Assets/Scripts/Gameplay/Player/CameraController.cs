using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);

    [Header("Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private bool followOnX = true;
    [SerializeField] private bool followOnY = true;

    [Header("Deadzone")]
    [SerializeField] private bool useDeadzone = true;
    [SerializeField] private Vector2 deadzoneSize = new Vector2(3f, 2f);

    [Header("Camera Bounds")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-50f, -50f);
    [SerializeField] private Vector2 maxBounds = new Vector2(50f, 50f);

    [Header("Look Ahead")]
    [SerializeField] private bool useLookAhead = true;
    [SerializeField] private float lookAheadDistance = 2f;
    [SerializeField] private float lookAheadSpeed = 2f;

    [Header("Zoom")]
    [SerializeField] private bool useZoom = false;
    [SerializeField] private float normalSize = 5f;
    [SerializeField] private float zoomSpeed = 2f;

    [Header("Screen Shake")]
    [SerializeField] private float shakeDecay = 5f;

    private Vector3 currentVelocity;
    private Vector3 targetPosition;
    private float currentLookAhead;
    private Vector3 lastTargetPosition;
    private Camera cam;

    private float shakeIntensity = 0f;
    private float shakeTimer = 0f;

    private void Start()
    {
        cam = GetComponent<Camera>();

        // WICHTIG: Wir entfernen hier die Such-Logik. 
        // Die Kamera soll in der LateUpdate suchen, falls sie niemanden hat.

        if (cam != null && useZoom)
        {
            cam.orthographicSize = normalSize;
        }

        // Falls wir im Editor manuell ein Ziel gesetzt haben, Position übernehmen
        if (target != null)
        {
            transform.position = target.position + offset;
            lastTargetPosition = target.position;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                SetTarget(player.transform);
            }
            else
            {
                // Wenn immer noch kein Spieler da ist, abbrechen wir hier.
                // Die Kamera wartet einfach auf den Spawn.
                return;
            }
        }

        CalculateTargetPosition();


        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            1f / smoothSpeed
        );

    
        if (shakeTimer > 0f)
        {
            smoothedPosition += Random.insideUnitSphere * shakeIntensity;
            shakeTimer -= Time.deltaTime * shakeDecay;
            shakeIntensity = Mathf.Lerp(shakeIntensity, 0f, shakeDecay * Time.deltaTime);
        }


        if (useBounds)
        {
            smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minBounds.x, maxBounds.x);
            smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minBounds.y, maxBounds.y);
        }

        transform.position = smoothedPosition;
        lastTargetPosition = target.position;


        if (useZoom && cam != null)
        {
            HandleZoom();
        }
    }

    private void CalculateTargetPosition()
    {
        Vector3 desiredPosition = target.position + offset;


        if (useLookAhead)
        {
            Vector3 targetVelocity = (target.position - lastTargetPosition) / Time.deltaTime;
            float targetLookAhead = Mathf.Sign(targetVelocity.x) * lookAheadDistance;

            currentLookAhead = Mathf.Lerp(
                currentLookAhead,
                targetLookAhead,
                lookAheadSpeed * Time.deltaTime
            );

            desiredPosition.x += currentLookAhead;
        }


        if (useDeadzone)
        {
            Vector3 currentCameraPos = transform.position;

            if (followOnX)
            {
                float deltaX = desiredPosition.x - currentCameraPos.x;

                if (Mathf.Abs(deltaX) > deadzoneSize.x / 2f)
                {
                    float sign = Mathf.Sign(deltaX);
                    desiredPosition.x = currentCameraPos.x + sign * (Mathf.Abs(deltaX) - deadzoneSize.x / 2f);
                }
                else
                {
                    desiredPosition.x = currentCameraPos.x;
                }
            }

            if (followOnY)
            {
                float deltaY = desiredPosition.y - currentCameraPos.y;

                if (Mathf.Abs(deltaY) > deadzoneSize.y / 2f)
                {
                    float sign = Mathf.Sign(deltaY);
                    desiredPosition.y = currentCameraPos.y + sign * (Mathf.Abs(deltaY) - deadzoneSize.y / 2f);
                }
                else
                {
                    desiredPosition.y = currentCameraPos.y;
                }
            }
        }

        if (!followOnX) desiredPosition.x = transform.position.x;
        if (!followOnY) desiredPosition.y = transform.position.y;

        desiredPosition.z = target.position.z + offset.z;

        targetPosition = desiredPosition;
    }

    private void HandleZoom()
    {
   
        float targetSize = normalSize;

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetSize,
            zoomSpeed * Time.deltaTime
        );
    }


    public void ShakeCamera(float intensity, float duration)
    {
        shakeIntensity = intensity;
        shakeTimer = duration;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            lastTargetPosition = target.position;
        }
    }

    public void SnapToTarget()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            currentVelocity = Vector3.zero;
            lastTargetPosition = target.position;
        }
    }

    public void SetBounds(Vector2 min, Vector2 max)
    {
        minBounds = min;
        maxBounds = max;
        useBounds = true;
    }

    public void DisableBounds()
    {
        useBounds = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (useDeadzone)
        {
            Gizmos.color = Color.yellow;
            Vector3 deadzoneCenter = transform.position;
            deadzoneCenter.z = 0;

            Gizmos.DrawWireCube(
                deadzoneCenter,
                new Vector3(deadzoneSize.x, deadzoneSize.y, 0f)
            );
        }

        if (useBounds)
        {
            Gizmos.color = Color.red;
            Vector3 boundsCenter = new Vector3(
                (minBounds.x + maxBounds.x) / 2f,
                (minBounds.y + maxBounds.y) / 2f,
                0f
            );
            Vector3 boundsSize = new Vector3(
                maxBounds.x - minBounds.x,
                maxBounds.y - minBounds.y,
                0f
            );
            Gizmos.DrawWireCube(boundsCenter, boundsSize);
        }
    }
}