using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Range(0f, 1f)]
    public float xMultiplier = 0.5f;

    [Range(0f, 1f)]
    public float zMultiplier = 0f; // nur hintere Layer nutzen das

    private float spriteWidth;

    private void OnEnable()
    {
        CameraScroller.OnCameraMoved += HandleCameraMove;
    }

    private void OnDisable()
    {
        CameraScroller.OnCameraMoved -= HandleCameraMove;
    }

    private void Start()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        spriteWidth = sr.bounds.size.x;
    }

    private void HandleCameraMove(Vector3 delta)
    {
        // X Parallax
        transform.position += new Vector3(delta.x * xMultiplier, 0f, 0f);

        // Z Depth Parallax (nur hintere Layer)
        if (zMultiplier != 0f)
        {
            transform.position += new Vector3(0f, 0f, delta.x * zMultiplier);
        }

        Loop();
    }

    private void Loop()
    {
        if (Camera.main.transform.position.x - transform.position.x > spriteWidth)
        {
            transform.position += Vector3.right * spriteWidth;
        }
    }
}
