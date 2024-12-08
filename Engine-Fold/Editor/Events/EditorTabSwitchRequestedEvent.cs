using System.Collections.Generic;
using FoldEngine.Events;

namespace FoldEngine.Editor.Events;

[Event("fold:editor.tab_switch_requested")]
public struct EditorTabSwitchRequestedEvent
{
    public string TabName; // TODO
}
[Event("fold:editor.inspector_requested.entity")]
public struct EntityInspectorRequestedEvent
{
    public List<long> Entities;
}
[Event("fold:editor.inspector_requested.object")]
public struct ObjectInspectorRequestedEvent
{
    public object Object;
}