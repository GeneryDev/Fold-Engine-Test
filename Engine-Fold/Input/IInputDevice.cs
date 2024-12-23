using FoldEngine.Interfaces;

namespace FoldEngine.Input;

public interface IInputDevice
{
    bool IsBeingUsed { get; }

    void Update(InputUnit inputUnit);
    T Get<T>(string name) where T : IInputInfo;
}