namespace AngryDash.Game.Event
{
    // Implement all required interface
    public interface Interface: Mod.IScript, ICollision {}

    public interface ICollision { void Collision(); }
}