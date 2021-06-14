using System.IO;
using FoldEngine.Interfaces;

namespace FoldEngine.Commands {
    public class SaveSceneCommand : ICommand {
        public string TargetPath;

        public SaveSceneCommand(string targetPath) {
            TargetPath = targetPath;
        }

        public void Execute(IGameCore core) {
            Directory.GetParent(TargetPath).Create();
            core.ActiveScene.Save(TargetPath);
        }
    }
    public class LoadSceneCommand : ICommand {
        public string SourcePath;

        public LoadSceneCommand(string sourcePath) {
            SourcePath = sourcePath;
        }

        public void Execute(IGameCore core) {
            core.ActiveScene.Load(SourcePath);
        }
    }
}