using Godot;
using System.Collections;
using System.Collections.Generic;

public enum BlockType
{
    grass = 0,
    rock = 1,
    sand = 3,
    wood = 4,
    leaves = 5,
}

// A single voxel.
public struct Voxel
{
    public BlockType Type;
    public bool Active;
}

public class Chunk : Object
{
    public static Vector3 ChunkSize = new Vector3(16, 255, 16);
    public Vector2 Offset;
    public Voxel[,,] Voxels = new Voxel[16,255,16];
    
    // Returns the highest voxels at X Z.
    public int HighestAt(int x, int z)
    {
        for (int y = (int)ChunkSize.y - 1; y > 0; y--)
        {
            //GD.Print(new Vector3(x, y, z));
            if (Voxels[x, y, z].Active)
                return y;
        }
        return 0;
    }

    public Chunk(int offsetX, int offsetZ)
    {
        // Passing in the seed.
        Offset = new Vector2(offsetX, offsetZ);

        for (int x = 0; x < ChunkSize.x; x++)
            for (int y = 0; y < ChunkSize.y; y++)
                for (int z = 0; z < ChunkSize.z; z++)
                {
                    Voxels[x, y, z] = new Voxel() { Active = false, Type = BlockType.grass};
                }
    }

   
}

