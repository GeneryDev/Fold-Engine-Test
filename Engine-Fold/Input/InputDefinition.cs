using FoldEngine.IO;
using FoldEngine.Resources;
using Newtonsoft.Json.Linq;

namespace FoldEngine.Input;

[Resource("input", extensions: "json")]
public class InputDefinition : Resource
{
    public JObject Root;

    public override bool CanSerialize => false;

    public override void DeserializeResource(string path)
    {
        Root = JObject.Parse(Data.In.ReadString(path));
    }
}