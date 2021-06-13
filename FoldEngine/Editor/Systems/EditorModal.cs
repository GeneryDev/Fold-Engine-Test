﻿using System;
using FoldEngine.Events;
using FoldEngine.Systems;
using Microsoft.Xna.Framework;

namespace FoldEngine.Editor.Systems {
    public class EditorModal : GameSystem {
        public bool ModalActive = false;
        public bool ModalVisible = true;

        public override void SubscribeToEvents() {
            Subscribe<ForceModalChangeEvent>((ref ForceModalChangeEvent evt) => {
                ModalVisible = this.GetType() == evt.ModalType;
            });
        }

        public GuiPanel NewSidebarPanel() {
            return Owner.Systems.Get<EditorBase>().Environment.Panel(new Rectangle(EditorBase.SidebarX + EditorBase.SidebarMargin * 2,
                EditorBase.SidebarMargin,
                EditorBase.SidebarWidth - EditorBase.SidebarMargin * 2 * 2,
                720 - EditorBase.SidebarMargin * 2));
        }
    }

    [Event("editor:force_modal_change", EventFlushMode.End)]
    public struct ForceModalChangeEvent {
        public Type ModalType;

        public ForceModalChangeEvent(Type modalType) {
            ModalType = modalType;
        }
    }
}