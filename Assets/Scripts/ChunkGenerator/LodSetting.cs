
[System.Serializable]
public struct LODSetting
{
    public LOD lod;
    public int distanceThreshold;

    public LODSetting(LOD lod, int distanceThreshold)
    {
        this.lod = lod;
        this.distanceThreshold = distanceThreshold;
    }

    public int sqrDistanceTreshold
    {
        get
        {
            return distanceThreshold * distanceThreshold;
        }
    }
}