using System.Linq;
using Content.SecretStationServer.DarkForces.Narsi.Progress.Components;
using Content.SecretStationServer.DarkForces.Narsi.Progress.Objectives.Offering;
using Content.SecretStationServer.DarkForces.Saint.Chaplain.Components;
using Content.Shared.SecretStation.DarkForces.Narsi.Roles;
using Robust.Shared.GameObjects;

namespace Content.SecretStationServer.DarkForces.Narsi.Runes;

public sealed partial class NarsiRuneSystem
{
    private void ProcessOfferingRune(EntityUid rune)
    {
        var entities = FindHumanoidsNearRune(rune)
            .Where(entity => _mobStateSystem.IsDead(entity) && !HasComp<ChaplainComponent>(entity) && !HasComp<NarsiCultistComponent>(entity))
            .ToList();

        if (!entities.Any())
            return;

        var target = entities.First();
        if (HasComp<NarsiCultOfferingTargetComponent>(target))
        {
            var ev = new NarsiCultOfferingTargetEvent();
            RaiseLocalEvent(target, ref ev);
        }

        QueueDel(target);
    }
}
