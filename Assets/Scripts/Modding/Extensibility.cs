namespace AngryDash.Mod
{
    public interface IScript
    {
        string Name { get; }
        string Description { get; }
        void Start();
    }
}