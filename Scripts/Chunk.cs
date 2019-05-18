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
    public static RandomNumberGenerator RandomGenerator = new RandomNumberGenerator();
    public Vector3 ChunkSize = new Vector3(16, 255, 16);
    public Vector2 Offset; // Chunk position from X:0, Y:0

    // All the voxels into the chunk.
    public Voxel[,,] Voxels = new Voxel[16,255,16];
    //public Dictionary<Vector3, Voxel> Voxels;

    // Trees
    private int TreeChance = 10;
    private int MaxTree = 4;
    private int RockChance = 5;


    public Chunk(Vector2 pOffset, OpenSimplexNoise pNoise)
    {
        // Passing in the seed.
        RandomGenerator.Randomize();
        Offset = pOffset;

        for (int x = 0; x < ChunkSize.x; x++)
            for (int y = 0; y < ChunkSize.y; y++)
                for (int z = 0; z < ChunkSize.z; z++)
                {
                    Voxels[x, y, z] = new Voxel() { Active = false, Type = BlockType.grass};
                }
    }

    /// <summary>
    /// Generate the terrain of the chunk.
    /// </summary>
    /// <param name="pNoise"></param>
    public void Generate()
    {
        // Vegetation pass.
        if (RandomGenerator.RandiRange(1, 100) < TreeChance)
        {
            // Choose a random number of tree in the chunk.
            int numberTree = RandomGenerator.RandiRange(1, MaxTree);

            // Place each tree.
            for (int i = 0; i < numberTree; i++)
                PlaceTrees();
        }

        // can only place 1 boulder per chunk.
        //if (RandomGenerator.RandiRange(1, 100) < RockChance)
        //{
        //    int x = RandomGenerator.RandiRange(0, int.Parse(ChunkSize.x.ToString()) - 1);
        //    int z = RandomGenerator.RandiRange(0, int.Parse(ChunkSize.z.ToString()) - 1);

        //    // Find ground height
        //    for (int y = int.Parse(ChunkSize.y.ToString()); y > 0; y--)
        //    {
        //        if (Voxels.ContainsKey(new Vector3(x, y, z)) && Voxels[new Vector3(x, y, z)].type == BlockType.grass)
        //        {
        //            MakeBoulder(new Vector3(x, y, z));
        //            return;
        //        }
        //    }
        //}
    }

    public void PlaceTrees()
    {
        Tree tree = new Tree(RandomGenerator);

        // Select random Position
        int x = RandomGenerator.RandiRange(0, 15), z = RandomGenerator.RandiRange(0, 15);

        // Get highest vox at x, y. Starting from the sky until it finds something.
        for (int i = (int)ChunkSize.y; i > 0; i--)
        {
            int x2 = x, y = i - tree.Height, z2 = z;

            if (!Voxels[x2, y, z2].Active && Voxels[x2, y, z2].Type == BlockType.grass)
            {
                if (y < Generator.WaterLevel)
                    return;

                // Then place the voxels of the tree.// ofc always security check.
                foreach (Voxel voxel in tree.Voxels.Values)
                {
                    if (!Voxels[x, y, z].Active)
                    {
                        Voxels[x, y, z].Active = true;
                        Voxels[x, y, z].Type = voxel.Type;
                    }
                }
                return;
            }
        }
    }

    //public void MakeBoulder(Vector3 pOffset)
    //{
    //    // Random width of the boulder
    //    int boulderWidth = RandomGenerator.RandiRange(3, 12);
    //    for (int x = -boulderWidth; x < boulderWidth; x++)
    //        for (int y = -boulderWidth; y < boulderWidth; y++)
    //            for (int z = -boulderWidth; z < boulderWidth; z++)
    //            {
    //                // If outside the sphere skip.
    //                if ((new Vector3(x, y, z) - new Vector3(0, 0, 0)).Length() >= boulderWidth)
    //                    continue;

    //                Vector3 voxelPosition = new Vector3(x + pOffset.x, y + pOffset.y, z + pOffset.z);

    //                // Check if there is space for the new voxe.
    //                if (Voxels.ContainsKey(voxelPosition))
    //                    continue;

    //                // Create new Voxel.
    //                var newVoxel = new Voxel()
    //                {
    //                    Position = voxelPosition,
    //                    type = BlockType.rock
    //                };
    //                Voxels.Add(voxelPosition, newVoxel);
    //            }


    //}
}

