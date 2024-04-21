using System.Reflection;
using HarmonyLib;

namespace Content.Server.SS220.EnginePatches;

public sealed class PatchManager
{
    public static void Patch(ILogManager logMan)
    {
        var sawmill = logMan.GetSawmill("Harmony");
        sawmill.Info("Applying harmony patches...");
        var harmony = new Harmony("net.ss220.server.enginepatch");
        var assembly = Assembly.GetExecutingAssembly();
        harmony.PatchAll(assembly);
    }
}
