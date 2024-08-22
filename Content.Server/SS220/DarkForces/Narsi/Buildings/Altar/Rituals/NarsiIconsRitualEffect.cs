using Content.SecretStationServer.DarkForces.Narsi.Buildings.Altar.Rituals.Base;
using Content.Shared.SecretStation.DarkForces.Narsi.Buildings.Altar;
using Robust.Shared.GameObjects;

namespace Content.SecretStationServer.DarkForces.Narsi.Buildings.Altar.Rituals;

public sealed partial class NarsiIconsRitualEffect : NarsiRitualEffect
{

    public override void MakeRitualEffect(EntityUid altar, EntityUid perfomer, NarsiAltarComponent component, IEntityManager entityManager)
    {
        var netEvent = new NarsiIconsRitualFinishedEvent();
        entityManager.EntityNetManager.SendSystemNetworkMessage(netEvent);
    }
}
