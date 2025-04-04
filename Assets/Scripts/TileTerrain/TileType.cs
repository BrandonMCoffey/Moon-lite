// TileType.cs
public enum TileType : byte // Use byte for memory efficiency
{
    Air = 0,
    Dirt = 1,
    Stone = 2,
    Grass = 3, // Surface dirt often has grass
    Copper = 4,
    Iron = 5
    // Add more mineral types as needed
}