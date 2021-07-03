using System.Collections.Generic;
using System.IO;
using FoldEngine.Interfaces;
using FoldEngine.Scenes;
using FoldEngine.Serialization;

namespace FoldEngine.Commands {
    public class SaveSceneCommand : ICommand {
        public string TargetPath;

        public SaveSceneCommand(string targetPath) {
            TargetPath = targetPath;
        }

        public void Execute(IGameCore core) {
            Directory.GetParent(TargetPath).Create();
            var saveOp = new SaveOperation(TargetPath);
            saveOp.Options.Set(SerializeOnlyEntities.Instance, new List<long>() {1, 4});
            
            core.ActiveScene.Save(saveOp);
            
            saveOp.Close();
            saveOp.Dispose();
        }
    }
    public class LoadSceneCommand : ICommand {
        public string SourcePath;

        public LoadSceneCommand(string sourcePath) {
            SourcePath = sourcePath;
        }

        public void Execute(IGameCore core) {
            var loadOp = new LoadOperation(SourcePath);
            
            // loadOp.Options.Set(DeserializeClearScene.Instance, true);
            loadOp.Options.Set(DeserializeRemapIds.Instance, new EntityIdRemapper(core.ActiveScene));
            
            core.ActiveScene.Load(loadOp);
            
            loadOp.Close();
            loadOp.Dispose();
        }
    }
}