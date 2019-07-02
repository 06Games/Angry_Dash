namespace AngryDash.Game.Event
{
    public interface Interface: Mod.IScript, ICollision
    {
    }

    public interface ICollision { void collision(); }
}