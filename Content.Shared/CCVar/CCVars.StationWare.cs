using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Do we delete bodies once they die?
    /// </summary>
    public static readonly CVarDef<bool> StationWareMobsDeleteDeadBodies =
        CVarDef.Create("stationware.mobs.delete_dead_bodies", true, CVar.SERVERONLY);
}
