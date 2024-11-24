using System;
using FoldEngine.Editor.ImmediateGui.Fields.Transactions;
using FoldEngine.Graphics;
using FoldEngine.ImmediateGui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Text;
using FoldEngine.Util;
using FoldEngine.Util.Transactions;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.ImmediateGui.Fields.Text;

public class TextField : GuiElement, IInspectorField
{
    private const int FontSize = 9;
    public readonly Caret Caret;

    public readonly Document Document = new Document();

    public readonly TransactionManager<TextField> Transactions;

    private PooledValue<IGuiAction> _editedAction;
    private int _fieldsInRow = 1;

    private int _parentWidthOccupied;

    private readonly TextRenderer _textRenderer = new TextRenderer();

    public TextField()
    {
        Caret = new Caret(this);
        Transactions = new TransactionManager<TextField>(this);

        Document.Text = "Hello World";
    }

    public override bool Focusable => true;

    private Point TextRenderingStartPos => new Point(Bounds.X + 4, Bounds.Y + FontSize + 5);

    public bool EditValueForType(Type type, ref object value, int index)
    {
        if (type == typeof(string))
        {
            value = Document.Text;
        }
        else if (type == typeof(ResourceIdentifier))
        {
            value = new ResourceIdentifier(Document.Text);
        }
        else if (type == typeof(int))
        {
            if (!int.TryParse(Document.Text, out int parsed)) return false;
            value = parsed;
        }
        else if (type == typeof(long))
        {
            if (!long.TryParse(Document.Text, out long parsed)) return false;
            value = parsed;
        }
        else if (type == typeof(float))
        {
            if (!float.TryParse(Document.Text, out float parsed)) return false;
            value = parsed;
        }
        else if (type == typeof(double))
        {
            if (!double.TryParse(Document.Text, out double parsed)) return false;
            value = parsed;
        }
        else if (type == typeof(Vector2))
        {
            if (!float.TryParse(Document.Text, out float parsed)) return false;
            var newVector = (Vector2)value;

            switch (index)
            {
                case 0:
                    newVector.X = parsed;
                    break;
                case 1:
                    newVector.Y = parsed;
                    break;
            }

            value = newVector;
        }
        else if (type == typeof(Vector3))
        {
            if (!float.TryParse(Document.Text, out float parsed)) return false;
            var newVector = (Vector3)value;

            switch (index)
            {
                case 0:
                    newVector.X = parsed;
                    break;
                case 1:
                    newVector.Y = parsed;
                    break;
                case 2:
                    newVector.Y = parsed;
                    break;
            }

            value = newVector;
        }
        else if (type == typeof(Color))
        {
            if (!byte.TryParse(Document.Text, out byte parsed)) return false;
            var newColor = (Color)value;

            switch (index)
            {
                case 0:
                    newColor.R = parsed;
                    break;
                case 1:
                    newColor.G = parsed;
                    break;
                case 2:
                    newColor.B = parsed;
                    break;
                case 3:
                    newColor.A = parsed;
                    break;
            }

            value = newColor;
        }
        else
        {
            throw new ArgumentException("Unsupported text field type " + type);
        }

        return true;
    }

    public TextField FieldSpacing(int parentWidthOccupied, int fieldsInRow = 1)
    {
        _parentWidthOccupied = parentWidthOccupied;
        _fieldsInRow = fieldsInRow;
        return this;
    }

    public override void Reset(GuiPanel parent)
    {
        _editedAction.Free();
        _parentWidthOccupied = 0;
        _fieldsInRow = 1;
    }

    public override void AdjustSpacing(GuiPanel parent)
    {
        Bounds.Width = (int)Math.Ceiling((float)(parent.Bounds.Width - _parentWidthOccupied) / _fieldsInRow);
        Bounds.Height = 12 * Document.GraphicalLines + 6;
        Margin = 4;
    }


    public override void Render(IRenderingUnit renderer, IRenderingLayer layer, Point offset = default)
    {
        if (Bounds.Contains(Environment.MousePos)) Environment.HoverTarget.Element = this;

        layer.Surface.Draw(new DrawRectInstruction
        {
            Texture = renderer.WhiteTexture,
            Color = new Color(63, 63, 70),
            DestinationRectangle = Bounds.Translate(offset)
        });

        if (Document.Dirty)
        {
            _textRenderer.Start(renderer.Fonts["default"], "", FontSize);
            Document.RebuildModel(_textRenderer);

            if (Focused) _editedAction.Value?.Perform(this, default);
        }

        if (Pressed(MouseEvent.LeftButton))
            Caret.DotIndex = Document.ViewToModel(Environment.MousePos - TextRenderingStartPos);


        Point textRenderingStartPos = TextRenderingStartPos;

        Caret.PreRender(renderer, layer, textRenderingStartPos);

        _textRenderer.DrawOnto(layer.Surface, textRenderingStartPos, Focused ? Color.White : Color.LightGray);

        Caret.PostRender(renderer, layer, textRenderingStartPos);
    }

    public override void OnMousePressed(ref MouseEvent e)
    {
        base.OnMousePressed(ref e);
        Caret.Dot = Document.ViewToModel(e.Position - TextRenderingStartPos);
    }

    public override void OnKeyTyped(ref KeyboardEvent e)
    {
        base.OnKeyTyped(ref e);

        if (e.Character == '\b')
            Transactions.InsertTransaction(new DeletionEdit(this,
                KeyModifiersExt.GetKeyModifiers().Has(KeyModifiers.Control)));
        else if (e.Character == 127)
            Transactions.InsertTransaction(new DeletionEdit(this,
                KeyModifiersExt.GetKeyModifiers().Has(KeyModifiers.Control), true));
        else
            Transactions.InsertTransaction(new InsertionEdit(new[] { e.Character }, this));
    }

    public override void OnInput(ControlScheme controls)
    {
        Caret.OnInput(controls);

        if (controls.Get<ButtonAction>("editor.undo").Consume()) Transactions.Undo();
        if (controls.Get<ButtonAction>("editor.redo").Consume()) Transactions.Redo();

        if (controls.Get<ButtonAction>("editor.field.caret.debug").Consume())
        {
            // Console.WriteLine(_document.GetLogicalLineForIndex(_dot));
        }
    }

    public override void OnMouseReleased(ref MouseEvent e)
    {
        base.OnMouseReleased(ref e);
    }

    public override void Displace(ref Point layoutPosition)
    {
        layoutPosition += new Point(Bounds.Width, 0);
    }

    public override void OnFocusGained()
    {
        Caret.OnFocusGained();
    }

    public TextField Value(string value)
    {
        if (!Focused) Document.Text = value;

        return this;
    }

    public TextField EditedAction(IGuiAction action)
    {
        _editedAction.Value = action;
        return this;
    }

    public T EditedAction<T>() where T : IGuiAction, new()
    {
        var action = Parent.Environment.ActionPool.Claim<T>();
        _editedAction.Value = action;
        return action;
    }
}