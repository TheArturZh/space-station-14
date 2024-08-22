using Content.Client.SecretStation.DarkForces.Narsi;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;
using static Content.Shared.SecretStation.Cult.Runes.SharedNarsiRuneComponent;

namespace Content.SecretStationClient.DarkForces.Narsi;

public sealed class NarsiReviveRuneVisualizerSystem : VisualizerSystem<NarsiRuneVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, NarsiRuneVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (AppearanceSystem.TryGetData<RuneState>(uid, RuneStatus.Status, out var status, args.Component))
        {
            args.Sprite.LayerSetVisible(NarsiRuneVisualLayers.Idle, status == RuneState.Idle);
            args.Sprite.LayerSetVisible(NarsiRuneVisualLayers.Active, status == RuneState.Active);
        }
    }
}
