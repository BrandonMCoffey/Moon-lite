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
    Phaseum = 14, // Pass-through-walls
    Pyrokite = 15, // Fire/Incendiary
    Aethergem = 16, // Homing
    Corestone = 17, // Knockback
    Explosive = 18,
    Moon = 19,
}