using UnityEngine;

public class BulletTrail : MonoBehaviour
{
    [Header("Line Renderer References")]
    private LineRenderer lineRenderer;
    private Transform muzzlePosition;

    [Header("Visual Settings")]
    private float trailDuration = 0.02f;
    private float trailWidthStart = 0.05f;
    private float trailWidthEnd = 0.03f;
    [SerializeField] private Gradient lineGradient;

    public void Initialized()
    {
        InitializeLineRenderer();
    }

    private void InitializeLineRenderer()
    {
        lineRenderer = GetComponent<LineRenderer>();
        muzzlePosition = transform;

        lineRenderer.startWidth = trailWidthStart;
        lineRenderer.endWidth = trailWidthEnd;
        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }

    public void ShowTrail(Vector3 hitPoint)
    {
        if (lineRenderer == null || muzzlePosition == null) return;

        lineRenderer.SetPosition(0, muzzlePosition.position);
        lineRenderer.SetPosition(1, hitPoint);

        lineRenderer.colorGradient = lineGradient;

        lineRenderer.enabled = true;
        StartCoroutine(HideTrailAfterDelay());
    }

    private System.Collections.IEnumerator HideTrailAfterDelay()
    {
        yield return new WaitForSeconds(trailDuration);

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    public void SetTrailSettings(float duration, float startWidth, float endWidth)
    {
        trailDuration = duration;
        trailWidthStart = startWidth;
        trailWidthEnd = endWidth;

        if (lineRenderer != null)
        {
            lineRenderer.startWidth = startWidth;
            lineRenderer.endWidth = endWidth;
        }
    }
}
