using System;
using System.IO;
using System.Runtime.Remoting.Messaging;
using FoldEngine.Interfaces;
using FoldEngine.Resources;
using FoldEngine.Scenes;
using FoldEngine.Serialization;

namespace FoldEngine.Commands {
    public class SaveSceneCommand : ICommand {
        public string Identifier;

        public SaveSceneCommand(string identifier) {
            Identifier = identifier;
        }

        public void Execute(IGameCore core) {
            string oldIdentifier = core.ActiveScene.Identifier;
            core.ActiveScene.Identifier = Identifier;
            core.ActiveScene.Save();
            core.ActiveScene.Identifier = oldIdentifier;

            core.ResourceIndex.Update();
        }
    }

    public class LoadSceneCommand : ICommand {
        public ResourceIdentifier Identifier;

        public LoadSceneCommand(string identifier) {
            Identifier = new ResourceIdentifier(identifier);
        }

        public void Execute(IGameCore core) {
            core.Resources.Load<Scene>(ref Identifier, s => {
                Console.WriteLine("Successfully loaded!");
                core.ActiveScene = (Scene)s;
                core.Resources.Detach(s);
            });
            // var loadOp = new LoadOperation(SourcePath);
            //
            // loadOp.Options.Set(DeserializeClearScene.Instance, true);
            // loadOp.Options.Set(DeserializeRemapIds.Instance, new EntityIdRemapper(core.ActiveScene));
            //
            // core.ActiveScene.Deserialize(loadOp);
            //
            // loadOp.Close();
            // loadOp.Dispose();
        }
    }
}