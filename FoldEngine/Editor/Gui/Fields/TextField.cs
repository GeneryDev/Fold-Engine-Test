﻿using System;
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

        public TextField() {
            Caret = new Caret(this);
            Transactions = new TransactionManager<TextField>(this);

            Document.Text = "Hello World";
        }

        public override void Reset(GuiPanel parent) {
            _editedAction.Free();
        }

        public override void AdjustSpacing(GuiPanel parent) {
            Bounds.Width = parent.Bounds.Width;
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

        public override void OnFocusGained() {
            Caret.OnFocusGained();
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
    
    public class SetStringFieldAction : IGuiAction {
        private long _id;
        private FieldInfo _fieldInfo;
        private ComponentSet _set;

        public SetStringFieldAction Id(long id) {
            _id = id;
            return this;
        }
        
        public SetStringFieldAction FieldInfo(FieldInfo fieldInfo) {
            _fieldInfo = fieldInfo;
            return this;
        }

        public SetStringFieldAction ComponentSet(ComponentSet set) {
            _set = set;
            return this;
        }

        public IObjectPool Pool { get; set; }
        
        public void Perform(GuiElement element, MouseEvent e) {
            
            string oldValue = (string) _set.GetFieldValue((int) _id, _fieldInfo);
            string newValue = ((TextField) element).Document.Text;
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