using Content.SecretStationServer.DarkForces.Narsi.Cultist.Roles;
using Content.SecretStationServer.GameRules.Vampire.Role.Components;
using Content.Server.Roles;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using RatvarRoleComponent = Content.SecretStationServer.DarkForces.Ratvar.Righteous.Progress.Roles.RatvarRoleComponent;
using VampireTrallRoleComponent = Content.SecretStationServer.GameRules.Vampire.Role.Trall.VampireTrallRoleComponent;

namespace Content.SecretStationServer.Roles;

public sealed class SecretRoleSystem : EntitySystem
{
    [Dependency] private readonly RoleSystem _roleSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        _roleSystem.SubscribeAntagEvents<NarsiCultRoleComponent>();
        _roleSystem.SubscribeAntagEvents<VampireRoleComponent>();
        _roleSystem.SubscribeAntagEvents<VampireTrallRoleComponent>();
        _roleSystem.SubscribeAntagEvents<RatvarRoleComponent>();
    }
}
