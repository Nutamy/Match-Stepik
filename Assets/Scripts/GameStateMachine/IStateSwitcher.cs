namespace GameStateMachine
{
    public interface IStateSwitcher
    {
        void SwichState<T>() where T : IState;
    }
}