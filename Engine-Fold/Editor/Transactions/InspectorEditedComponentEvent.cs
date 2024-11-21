using System;
using FoldEngine.Events;
using FoldEngine.Scenes;

namespace FoldEngine.Editor.Transactions;

[Event("fold:editor.inspector_edited_component", EventFlushMode.AfterSystem)]
public struct InspectorEditedComponentEvent
{
    [EntityId] public long EntityId;
    public Type ComponentType;
    public string FieldName;
}