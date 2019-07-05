namespace AngryDash.Game.Event
{
    // Implement all required interface
    public interface Interface: Mod.IScript, ICollision {
        Event _Event { get; set; }
    }

    public interface ICollision { void Collision(); }
}