namespace FoldEngine.Input {
    public interface IInputDevice {
        bool IsBeingUsed { get; }

        void Update();
        T Get<T>(string name) where T : IInputInfo;
    }
}