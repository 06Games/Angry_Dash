namespace AngryDash.Extensibility
{
    public interface IScript
    {
        string Name { get; }
        string Description { get; }
        void Start();
        void Close();
    }
}