using Content.Server._StationWare.Challenges.Modifiers.Components;
using Content.Shared.Gibbing;

namespace Content.Server._StationWare.Challenges.Modifiers.Systems;

public sealed partial class GibOnFailModifierSystem : EntitySystem
{
    [Dependency] private GibbingSystem _gib = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<GibOnFailModifierComponent, PlayerChallengeStateSetEvent>(OnChallengeStateSet);
    }

    private void OnChallengeStateSet(EntityUid uid, GibOnFailModifierComponent component, ref PlayerChallengeStateSetEvent args)
    {
        if (args.Won)
            return;
        if (args.Player.AttachedEntity is not { } ent)
            return;
        _gib.Gib(ent, true);
    }
}
