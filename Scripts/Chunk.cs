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
    public BlockType type; 
    public Vector3 Position;
}

public class Chunk : Object
{
    public static RandomNumberGenerator RandomGenerator = new RandomNumberGenerator();
    public Vector3 ChunkSize = new Vector3(16, 255, 16);
    public Vector2 Offset; // Chunk position from X:0, Y:0

    // All the voxels into the chunk.
    public Dictionary<Vector3, Voxel> Voxels;

    // Generation settings
    // TODO: Move generation settings to another class.
    private float FloatTreshold = 0.1f;
    private int CarpetMaxHeight = 128;

    // Trees
    private int TreeChance = 10;
    private int MaxTree = 4;

    // Rocks
    private int RockChance = 5;

    private Dictionary<Vector2, Chunk> Queue;

    public Chunk(Vector2 pOffset, OpenSimplexNoise pNoise)
    {

        // Passing in the seed.
        RandomGenerator.Randomize();
        Offset = pOffset;
 


        

        // When the preloading is done, add it to the queue.
        // pQueue.Add(Offset, this);
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

        // Boulder pass.
        RandomGenerator.Randomize();

        // can only place 1 boulder per chunk.
        if (RandomGenerator.RandiRange(1, 100) < RockChance)
        {
            int x = RandomGenerator.RandiRange(0, int.Parse(ChunkSize.x.ToString()) - 1);
            int z = RandomGenerator.RandiRange(0, int.Parse(ChunkSize.z.ToString()) - 1);

            // Find ground height
            for (int y = int.Parse(ChunkSize.y.ToString()); y > 0; y--)
            {
                if (Voxels.ContainsKey(new Vector3(x, y, z)) && Voxels[new Vector3(x, y, z)].type == BlockType.grass)
                {
                    MakeBoulder(new Vector3(x, y, z));
                    return;
                }
            }
        }
    }


    /// <summary>
    /// Place a random tree at a arandom position.
    /// </summary>
    public void PlaceTrees()
    {
        
        Tree tree = new Tree(RandomGenerator);

        // Select random Position
        RandomGenerator.Randomize();
        int x = RandomGenerator.RandiRange(0, 15);
        RandomGenerator.Randomize();
        int z = RandomGenerator.RandiRange(0, 15);

        // Get highest vox at x, y. Starting from the sky until it finds something.
        for (int i = int.Parse(ChunkSize.y.ToString()); i > 0; i--)
        {
            Vector3 scanPosition = new Vector3(x, i - tree.Height, z);
            if (Voxels.ContainsKey(scanPosition) && Voxels[scanPosition].type == BlockType.grass)
            {
                if(scanPosition.y < Generator.WaterLevel)
                    return;
                // Then place the voxels of the tree.// ofc always security check.
                foreach (Voxel voxel in tree.Voxels.Values)
                {
                    if (!Voxels.ContainsKey(voxel.Position + scanPosition))
                    {
                        // Make our block.
                        Voxel newVoxel = new Voxel()
                        {
                            Position = voxel.Position + new Vector3(0, scanPosition.y, 0),
                            type = voxel.type
                        };

                        if (!Voxels.ContainsKey(newVoxel.Position))
                            Voxels.Add(newVoxel.Position, newVoxel);
                    }
                }
                return;
            }
        }
    }


    /// <summary>
    /// Adds a rock boulder at pOffset in the chunk.
    /// </summary>
    /// <param name="pOffset"></param>
    public void MakeBoulder(Vector3 pOffset)
    {
        // Random width of the boulder
        int boulderWidth = RandomGenerator.RandiRange(3, 12);
        for (int x = -boulderWidth; x < boulderWidth; x++)
            for (int y = -boulderWidth; y < boulderWidth; y++)
                for (int z = -boulderWidth; z < boulderWidth; z++)
                {
                    // If outside the sphere skip.
                    if ((new Vector3(x, y, z) - new Vector3(0, 0, 0)).Length() >= boulderWidth)
                        continue;

                    Vector3 voxelPosition = new Vector3(x + pOffset.x, y + pOffset.y, z + pOffset.z);

                    // Check if there is space for the new voxe.
                    if (Voxels.ContainsKey(voxelPosition))
                        continue;

                    // Create new Voxel.
                    var newVoxel = new Voxel()
                    {
                        Position = voxelPosition,
                        type = BlockType.rock
                    };
                    Voxels.Add(voxelPosition, newVoxel);
                }

             
    }
}

