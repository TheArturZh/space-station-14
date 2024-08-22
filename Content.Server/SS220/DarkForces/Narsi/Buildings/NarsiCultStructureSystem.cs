using Content.SecretStationServer.DarkForces.Narsi.Progress.Objectives.Building;
using Robust.Shared.GameObjects;

namespace Content.SecretStationServer.DarkForces.Narsi.Buildings;

public sealed class NarsiCultStructureSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NarsiCultStructureComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, NarsiCultStructureComponent component, ComponentInit args)
    {
        RaiseLocalEvent(new NarsiBuildingEvent(component.Building));
    }
}
