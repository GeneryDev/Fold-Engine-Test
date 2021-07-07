using System;
using System.Collections.Generic;
using System.Reflection;
using EntryProject.Util;
using FoldEngine.Components;
using FoldEngine.Editor.Gui.Fields.Transactions;
using FoldEngine.Editor.Transactions;
using FoldEngine.Editor.Views;
using FoldEngine.Graphics;
using FoldEngine.Gui;
using FoldEngine.Input;
using FoldEngine.Interfaces;
using FoldEngine.Text;
using FoldEngine.Util.Transactions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace FoldEngine.Editor.Gui.Fields {
    public class TextField : GuiElement {
        private const int FontSize = 9;
        
        private TextRenderer _textRenderer = new TextRenderer();
        
        public readonly Document Document = new Document();
        public readonly Caret Caret;
        
        public readonly TransactionManager<TextField> Transactions;

        public override bool Focusable => true;

        private PooledValue<IGuiAction> _editedAction;

        private int _parentWidthOccupied = 0;
        private int _fieldsInRow = 1;

        public TextField() {
            Caret = new Caret(this);
            Transactions = new TransactionManager<TextField>(this);

            Document.Text = "Hello World";
        }

        public TextField FieldSpacing(int parentWidthOccupied, int fieldsInRow = 1) {
            _parentWidthOccupied = parentWidthOccupied;
            _fieldsInRow = fieldsInRow;
            return this;
        }

        public override void Reset(GuiPanel parent) {
            _editedAction.Free();
            _parentWidthOccupied = 0;
            _fieldsInRow = 1;
        }

        public override void AdjustSpacing(GuiPanel parent) {
            Bounds.Width = (int)Math.Ceiling((float)(parent.Bounds.Width - _parentWidthOccupied) / _fieldsInRow);
            Bounds.Height = 12 * Document.GraphicalLines + 6;
            Margin = 4;
        }

        private Point TextRenderingStartPos => new Point(Bounds.X + 4, Bounds.Y + FontSize + 5);


        public override void Render(IRenderingUnit renderer, IRenderingLayer layer) {
            if(Bounds.Contains(Environment.MousePos)) {
                Environment.HoverTarget.Element = this;
            }

            layer.Surface.Draw(new DrawRectInstruction() {
                Texture = renderer.WhiteTexture,
                Color = new Color(63, 63, 70),
                DestinationRectangle = Bounds
            });

            if(Document.Dirty) {
                _textRenderer.Start(renderer.Fonts["default"], "", FontSize);
                Document.RebuildModel(_textRenderer);

                if(Focused) {
                    _editedAction.Value?.Perform(this, default);
                }
            }

            if(Pressed(MouseEvent.LeftButton)) {
                Caret.DotIndex = Document.ViewToModel(Environment.MousePos - TextRenderingStartPos);
            }
            
            
            var textRenderingStartPos = TextRenderingStartPos;

            Caret.PreRender(renderer, layer, textRenderingStartPos);
            
            _textRenderer.DrawOnto(layer.Surface, textRenderingStartPos, Focused ? Color.White : Color.LightGray);

            Caret.PostRender(renderer, layer, textRenderingStartPos);
        }

        public override void OnMousePressed(ref MouseEvent e) {
            base.OnMousePressed(ref e);
            Caret.Dot = Document.ViewToModel(e.Position - TextRenderingStartPos);
        }

        public override void OnKeyTyped(ref KeyboardEvent e) {
            base.OnKeyTyped(ref e);

            if(e.Character == '\b') {
                Transactions.InsertTransaction(new DeletionEdit(this, KeyModifiersExt.GetKeyModifiers().Has(KeyModifiers.Control)));
            } else if(e.Character == 127) {
                Transactions.InsertTransaction(new DeletionEdit(this, KeyModifiersExt.GetKeyModifiers().Has(KeyModifiers.Control), true));
            } else {
                Transactions.InsertTransaction(new InsertionEdit(new char[] {e.Character}, this));
            }
        }

        public override void OnInput(ControlScheme controls) {
            Caret.OnInput(controls);

            if(controls.Get<ButtonAction>("editor.undo").Consume()) {
                Transactions.Undo();
            }
            if(controls.Get<ButtonAction>("editor.redo").Consume()) {
                Transactions.Redo();
            }
            
            if(controls.Get<ButtonAction>("editor.field.caret.debug").Consume()) {
                // Console.WriteLine(_document.GetLogicalLineForIndex(_dot));
            }
        }

        public override void OnMouseReleased(ref MouseEvent e) {
            base.OnMouseReleased(ref e);
        }

        public override void Displace(ref Point layoutPosition) {
            layoutPosition += new Point(Bounds.Width, 0);
        }

        public override void OnFocusGained() {
            Caret.OnFocusGained();
        }

        public TextField Value(string value) {
            if(!Focused) {
                Document.Text = value;
            }

            return this;
        }

        public TextField EditedAction(IGuiAction action) {
            _editedAction.Value = action;
            return this;
        }

        public T EditedAction<T>() where T : IGuiAction, new() {
            var action = Parent.Environment.ActionPool.Claim<T>();
            _editedAction.Value = action;
            return action;
        }
    }
    
    public class SetFieldAction : IGuiAction {
        private long _id;
        private FieldInfo _fieldInfo;
        private ComponentSet _set;
        private int _index;

        public SetFieldAction Id(long id) {
            _id = id;
            return this;
        }
        
        public SetFieldAction FieldInfo(FieldInfo fieldInfo) {
            _fieldInfo = fieldInfo;
            _index = 0;
            return this;
        }

        public SetFieldAction ComponentSet(ComponentSet set) {
            _set = set;
            return this;
        }

        public SetFieldAction Index(int index) {
            _index = index;
            return this;
        }

        public IObjectPool Pool { get; set; }
        
        public void Perform(GuiElement element, MouseEvent e) {
            object oldValue = _set.GetFieldValue((int) _id, _fieldInfo);;
            object newValue = null;

            if(_fieldInfo.FieldType == typeof(int)) {
                if(!int.TryParse(((TextField) element).Document.Text, out int parsed)) return;
                newValue = parsed;
            } else if(_fieldInfo.FieldType == typeof(long)) {
                if(!long.TryParse(((TextField) element).Document.Text, out long parsed)) return;
                newValue = parsed;
            } else if(_fieldInfo.FieldType == typeof(float)) {
                if(!float.TryParse(((TextField) element).Document.Text, out float parsed)) return;
                newValue = parsed;
            } else if(_fieldInfo.FieldType == typeof(double)) {
                if(!double.TryParse(((TextField) element).Document.Text, out double parsed)) return;
                newValue = parsed;
            } else if(_fieldInfo.FieldType == typeof(Vector2)) {
                if(!float.TryParse(((TextField) element).Document.Text, out float parsed)) return;
                Vector2 newVector = (Vector2) oldValue;

                switch(_index) {
                    case 0: newVector.X = parsed;
                        break;
                    case 1: newVector.Y = parsed;
                        break;
                }

                newValue = newVector;
            } else if(_fieldInfo.FieldType == typeof(Vector3)) {
                if(!float.TryParse(((TextField) element).Document.Text, out float parsed)) return;
                Vector3 newVector = (Vector3) oldValue;

                switch(_index) {
                    case 0: newVector.X = parsed;
                        break;
                    case 1: newVector.Y = parsed;
                        break;
                    case 2: newVector.Y = parsed;
                        break;
                }

                newValue = newVector;
            } else if(_fieldInfo.FieldType == typeof(Color)) {
                if(!byte.TryParse(((TextField) element).Document.Text, out byte parsed)) return;
                Color newColor = (Color) oldValue;

                switch(_index) {
                    case 0: newColor.R = parsed;
                        break;
                    case 1: newColor.G = parsed;
                        break;
                    case 2: newColor.B = parsed;
                        break;
                    case 3: newColor.A = parsed;
                        break;
                }

                newValue = newColor;
            } else {
                Console.WriteLine("Unsupported SetFieldAction field type " + _fieldInfo.FieldType);
                return;
            }
            
            ((EditorEnvironment) element.Parent.Environment).TransactionManager.InsertTransaction(new SetComponentFieldTransaction() {
                ComponentType = _set.ComponentType,
                EntityId = _id,
                FieldInfo = _fieldInfo,
                OldValue = oldValue,
                NewValue = newValue
            });
        }
    }
}