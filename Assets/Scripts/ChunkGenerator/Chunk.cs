using UnityEngine;


public abstract class Chunk
{
    protected Vector2 position;
    protected Bounds bounds;
    protected Transform viewer;
    protected MeshSettings meshSettings;
    protected GameObject gameObject;
    protected bool _isVisible = false;
    protected bool wasVisible = false;

    public event System.Action<Chunk> OnLoad;

    public Chunk(Vector2 coordinate, MeshSettings meshSettings, Transform parent, Transform viewer)
    {
        this.meshSettings = meshSettings;
        this.viewer = viewer;

        position = coordinate * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

        gameObject = new GameObject("Chunk");
        gameObject.transform.parent = parent;
        gameObject.transform.position = new Vector3(position.x, 2, position.y);

        SetVisible(false);

    }

    public virtual void UpdateCollider() { }
    public virtual void Load() { }

    protected void NotifyLoaded()
    {
        if (OnLoad != null)
        {
            OnLoad(this);
        }
    }

    public abstract void Update();

    public float GetViewerDistanceFromEdge()
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

    public bool isVisible
    {
        get
        {
            return _isVisible;
        }
    }

    public void SetVisible(bool visible)
    {
        _isVisible = visible;
        gameObject.SetActive(visible);
        Update();
    }

    public Vector2 worldPosition
    {
        get
        {
            return position;
        }
    }
}
