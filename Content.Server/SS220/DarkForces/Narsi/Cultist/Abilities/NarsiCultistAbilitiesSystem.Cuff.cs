using Content.Server.Cuffs;
using Content.Shared.SecretStation.DarkForces.Narsi.Abilities.Events;
using Content.Shared.SecretStation.DarkForces.Narsi.Roles;
using Content.Shared.Stunnable;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.SecretStationServer.DarkForces.Narsi.Cultist.Abilities;

public sealed partial class NarsiCultistAbilitiesSystem
{
    [Dependency] private readonly CuffableSystem _cuffableSystem = default!;

    private void InitializeCuff()
    {
        SubscribeLocalEvent<NarsiCultistComponent, NarsiCultistCuffEvent>(OnCuffEvent);
    }

    private void OnCuffEvent(EntityUid uid, NarsiCultistComponent component, NarsiCultistCuffEvent args)
    {
        if (args.Handled)
            return;

        if (!HasComp<StunnedComponent>(args.Target))
            return;

        var coords = Transform(args.Performer).Coordinates;
        var handcuffs = Spawn("HandcuffsCult", coords);

        _cuffableSystem.TryCuffing(args.Performer, args.Target, handcuffs);
        OnCultistAbility(uid, args);
        args.Handled = true;
    }
}
