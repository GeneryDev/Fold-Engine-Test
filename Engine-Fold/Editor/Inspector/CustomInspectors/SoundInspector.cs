using FoldEngine.Audio;
using FoldEngine.Graphics;
using FoldEngine.Gui;

namespace FoldEngine.Editor.Inspector.CustomInspectors;

[CustomInspector(typeof(Sound))]
public class SoundInspector : CustomInspector<Sound>
{
    private SoundInstance _playingInstance;
    private string _playingSound = null;

    protected override void RenderInspectorBefore(Sound obj, GuiPanel panel)
    {
        panel.Element<GuiLabel>().Text("Length: " + (obj.Effect.Length / (ulong)obj.Effect.LengthTimeunit) + " ms ")
            .FontSize(9).TextAlignment(-1);

        if (_playingInstance != null && !_playingInstance.Playing)
        {
            _playingInstance?.Stop();
            _playingSound = null;
            _playingInstance = null;
        }

        bool stop = _playingSound == obj.Identifier;
        var button = panel.Element<GuiButton>()
                .Text(stop ? "Stop" : "Play")
                .FontSize(14)
            ;
        if (stop)
        {
            button.Icon(panel.Environment.EditorResources.Get<Texture>(ref EditorIcons.Pause));
        }
        else
        {
            button.Icon(panel.Environment.EditorResources.Get<Texture>(ref EditorIcons.Play));
        }

        if (button.IsPressed())
        {
            _playingInstance?.Stop();
            _playingSound = null;
            if (!stop)
            {
                _playingInstance = panel.Environment.Core.AudioUnit.CreateInstance(obj);
                _playingInstance.PlayOnce();
                _playingSound = obj.Identifier;
            }
        }
    }
}