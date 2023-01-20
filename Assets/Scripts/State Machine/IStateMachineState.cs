
public interface IStateMachineState
{
    void Tick();
    void OnEnter();
    void OnExit();
}
