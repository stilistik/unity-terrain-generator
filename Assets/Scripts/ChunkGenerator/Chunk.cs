using UnityEngine;


public abstract class Chunk
{
    protected Vector2 position;
    protected Bounds bounds;
    protected Transform viewer;
    protected MeshSettings meshSettings;
    protected GameObject gameObject;


    protected bool isVisible = false;
    protected bool wasVisible = false;

    public event System.Action<Chunk, bool> OnVisibleChanged;

    public Chunk(Vector2 coordinate, MeshSettings meshSettings, Transform viewer)
    {
        this.meshSettings = meshSettings;
        this.viewer = viewer;

        gameObject = new GameObject("Chunk");
        position = coordinate * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

        SetVisible(false);

    }


    public virtual void UpdateCollider() { }
    public virtual void Load() { }

    public void Update()
    {
        UpdateChunkVisibility();
        UpdateImpl();
        NotifyChunkVisibility();
    }

    public abstract void UpdateImpl();

    protected bool UpdateChunkVisibility()
    {
        float viewerDistance = GetViewerDistanceFromEdge();
        isVisible = viewerDistance <= meshSettings.maxViewDistance;
        return isVisible;
    }

    protected void NotifyChunkVisibility()
    {
        if (isVisible != wasVisible)
        {
            SetVisible(isVisible);
            if (OnVisibleChanged != null)
            {
                OnVisibleChanged(this, isVisible);
            }
        }
    }

    protected float GetViewerDistanceFromEdge()
    {
        return Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
    }

    protected Vector2 viewerPosition
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
        wasVisible = visible;
    }
}
