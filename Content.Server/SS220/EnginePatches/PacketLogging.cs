using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using Lidgren.Network;

namespace Content.Server.SS220.EnginePatches;

/// <summary>
/// This is a workaround for the issue when any client can easily DOS the server with IO operations of logger.
/// It simply removes logging calls by modifying IL code via Harmony transplier so there is no overhead.
///
/// IMPORTANT: Call indexes must be re-verified every time the engine is updated.
/// </summary>
[HarmonyPatch(typeof(NetPeer))]
[HarmonyPatch("ReceiveSocketData")]
public static class NetPeer_ReceiveSocketData_CheckForErrors_Patch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);

        // Cut out the warning message construction & log call.
        for (var i = 242; i <= 284; i++)
        {
            codes[i].opcode = OpCodes.Nop;
        }

        // Cut out the error message construction & ThrowOrLog call.
        for (var i = 300; i <= 308; i++)
        {
            codes[i].opcode = OpCodes.Nop;
        }

        return codes.AsEnumerable();
    }
}
