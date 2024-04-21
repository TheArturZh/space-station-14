using System.Reflection;
using HarmonyLib;

namespace Content.Server.SS220.EnginePatches;

public sealed class Patcher
{
    public static void Patch(ILogManager logMan)
    {
        Harmony.DEBUG = true;
        var sawmill = logMan.GetSawmill("Harmony");
        sawmill.Info("Applying Harmony patches...");
        var harmony = new Harmony("net.ss220.server.enginepatch");
        var assembly = Assembly.GetExecutingAssembly();
        harmony.PatchAll(assembly);
    }
}
