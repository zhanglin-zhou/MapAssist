using System;
using System.Runtime.InteropServices;

namespace MapAssist.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct Inventory
    {
        // Used to see if the current player is the correct player
        // Should not be 0x0 for local player
        [FieldOffset(0x70)] public long unk1;
    }
}
