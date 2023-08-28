using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server.SS220.SupaKitchen;
public sealed class CookingMachineSystem : EntitySystem
{
    [Dependency] private readonly ContainerSystem _container = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CookingMachineComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, CookingMachineComponent component, ComponentInit ags)
    {
        component.Storage = _container.EnsureContainer<Container>(uid, "cooking_instrument_entity_container");
    }

    public static bool HasContents(CookingMachineComponent component)
    {
        return component.Storage.ContainedEntities.Any();
    }
}
