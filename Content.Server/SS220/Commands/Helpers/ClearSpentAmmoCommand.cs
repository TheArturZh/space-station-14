using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Console;

namespace Content.Server.SS220.Commands.Helpers;

[AdminCommand(AdminFlags.Admin)]
public sealed class ClearSpentAmmoCommand : IConsoleCommand
{
    [Dependency] private readonly IEntityManager _entManager = default!;

    // ReSharper disable once StringLiteralTypo
    public string Command => "clearspentammo";
    public string Description => "Удаляет все отстрелянные патроны в игре";
    public string Help => $"Usage: {Command}";

    public void Execute(IConsoleShell shell, string argsOther, string[] args)
    {
        var processed = 0;
        var query = _entManager.AllEntityQueryEnumerator<CartridgeAmmoComponent>();
        while (query.MoveNext(out var entity, out var comp))
        {
            if (comp.Spent)
            {
                _entManager.QueueDeleteEntity(entity);
                processed++;
            }
        }

        shell.WriteLine($"Удалено {processed} энтити.");
    }
}
