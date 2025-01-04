using FoldEngine.Util;

namespace FoldEngine.Scenes.Prefabs;

public class LoadAsPrefab : Field<LoadAsPrefab>
{
    public static readonly LoadAsPrefab Instance = new();
    
    public long OwnerEntityId;
    public PrefabLoadMode LoadMode;
}