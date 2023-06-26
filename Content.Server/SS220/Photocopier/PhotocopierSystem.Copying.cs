using Content.Server.Paper;
using Content.Shared.SS220.ButtScan;
using Robust.Shared.Serialization;

namespace Content.Server.SS220.Photocopier;

public sealed partial class PhotocopierSystem
{
    /*
     * Every component that is copyable on a photocopier should implement IPhotocopyable interface, which is 2 functions:
     * PhotocopyableData ToPhotocopyableData -> Creates own serializable class
     *
     */
}
