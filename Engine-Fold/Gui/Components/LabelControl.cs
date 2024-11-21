using FoldEngine.Components;
using FoldEngine.Editor.Inspector;
using FoldEngine.Gui.Components;
using FoldEngine.Gui.Components.Traits;
using FoldEngine.Gui.Events;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Serialization;
using FoldEngine.Text;
using Microsoft.Xna.Framework;

namespace FoldEngine.Gui.Components
{
    [Component("fold:control.label", traits: [typeof(Control), typeof(MousePickable)])]
    [ComponentInitializer(typeof(LabelControl), nameof(InitializeComponent))]
    public struct LabelControl
    {
        public string Text;
        public int FontSize;
        public Color Color;
        public Alignment Alignment;

        [DoNotSerialize] [HideInInspector] public RenderedText RenderedText;
    
        public LabelControl()
        {
            Text = "";
            FontSize = 14;
            Color = Color.White;
            Alignment = Alignment.Begin;
        }
    
        /// <summary>
        ///     Returns an initialized label component with all its correct default values.
        /// </summary>
        public static LabelControl InitializeComponent(Scene scene, long entityId)
        {
            return new LabelControl();
        }

        public bool UpdateRenderedText(IRenderingUnit renderer)
        {
            if (RenderedText.HasValue && RenderedText.Text == Text && RenderedText.Size == FontSize)
            {
                // already up to date
                return false;
            }
            renderer.Fonts["default"].RenderString(Text, out RenderedText, FontSize);
            return true;
        }
    }
}

namespace FoldEngine.Gui.Systems
{
    public partial class ControlRenderer
    {
        private void RenderLabel(IRenderingUnit renderer, IRenderingLayer layer, ref Transform transform,
            ref Control control, ref LabelControl label)
        {
            var bounds = new Rectangle(transform.Position.ToPoint(), control.Size.ToPoint());

            if (label.UpdateRenderedText(renderer))
            {
                control.ComputedMinimumSize = new Vector2(label.RenderedText.Width, label.RenderedText.Height);
                control.RequestLayout = true;
            }

            ref RenderedText renderedText = ref label.RenderedText;
            if (!renderedText.HasValue) return;

            float textWidth = renderedText.Width;

            int totalWidth = (int)textWidth;

            int x;
            switch (label.Alignment)
            {
                case Alignment.Begin:
                    x = bounds.X;
                    break;
                case Alignment.Center:
                    x = bounds.Center.X - totalWidth / 2;
                    break;
                case Alignment.End:
                    x = bounds.X + bounds.Width - totalWidth;
                    break;
                default:
                    x = bounds.X;
                    break;
            }

            Point offset = Point.Zero;

            renderedText.DrawOnto(layer.Surface,
                new Point(x, bounds.Center.Y - renderedText.Height / 2 + label.FontSize) + offset,
                label.Color, z: -control.ZOrder);
            // layer.Surface.Draw(new DrawRectInstruction
            // {
            //     Texture = renderer.WhiteTexture,
            //     Color = Color.White,
            //     DestinationRectangle = new Rectangle(new Point(x - 2, bounds.Center.Y - renderedText.Height / 2 + label.FontSize), new Point(2, 2)),
            //     Z = control.ZOrder
            // });
        }

        private void SubscribeToLabelEvents()
        {
            Subscribe((ref MinimumSizeRequestedEvent evt) =>
            {
                if (evt.EntityId != -1 && Scene.Components.HasComponent<LabelControl>(evt.EntityId) &&
                    Scene.Components.HasComponent<Control>(evt.EntityId))
                {
                    ref var label = ref Scene.Components.GetComponent<LabelControl>(evt.EntityId);
                    ref var control = ref Scene.Components.GetComponent<Control>(evt.EntityId);

                    if (label.UpdateRenderedText(Scene.Core.RenderingUnit))
                    {
                        control.ComputedMinimumSize = new Vector2(label.RenderedText.Width, label.RenderedText.Height);
                        control.RequestLayout = true;
                    }
                }
            });
        }
    }
}