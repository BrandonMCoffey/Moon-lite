public enum TileType : byte
{
    Air = 0,

    Surface = 1,
    SurfaceUnderground = 2,
    Underground = 3,
    DeepUnderground = 4,
    UndergroundEdge = 5,

    Copper = 10, // Weak
    Lead = 11, // Default
    Cryselium = 12, // Ice/Freezing
    Voltthorn = 13, // Lightning
    Pyrokite = 14, // Fire/Incendiary
    Phaseum = 15, // Pass-through-walls
    Aethergem = 16, // Homing
    Corestone = 17, // Knockback
}