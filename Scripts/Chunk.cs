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
    public BlockType  type; 
    public Vector3 Position;
}

public class Chunk : Object
{
    public static RandomNumberGenerator RandomGenerator = new RandomNumberGenerator();
    public Vector3 ChunkSize = new Vector3(16, 255, 16);
    public Vector2 Offset; // Chunk position from X:0, Y:0

    // All the voxels into the chunk.
    public Dictionary<Vector3, Voxel> Voxels = new Dictionary<Vector3, Voxel>();

    // Generation settings
    // TODO: Move generation settings to another class.
    private float FloatTreshold = 0.2f;
    private int CarpetMaxHeight = 128;

    public Chunk(Vector2 pOffset, Dictionary<Vector2, Chunk> pQueue, OpenSimplexNoise pNoise)
    {
        if (pQueue.ContainsKey(pOffset))
            return;
        RandomGenerator.Seed = pNoise.Seed;
        Offset = pOffset;

        // Generation
        Generate(pNoise);
        pQueue.Add(Offset, this);
    }

    public void Generate(OpenSimplexNoise pNoise)
    {
        // Terrain generation(noise2D and noise3D).
        for (int z = 0; z < ChunkSize.z; z += 1)
            for (int x = 0; x < ChunkSize.x; x += 1)
            {
                // Global position in the noise.
                Vector2 globalPosition = new Vector2((Offset.x * ChunkSize.x) + x, (Offset.y * ChunkSize.z) + z);

                // Height from 3D noise.
                float height = Mathf.Stepify( ( pNoise.GetNoise2dv(globalPosition) + 1 ) * CarpetMaxHeight, 1);

                // Make new voxel with height.
                Voxel voxel = new Voxel()
                {
                    Position = new Vector3(x, height, z),
                    type = 0
                };
                
                Voxels.Add(voxel.Position, voxel);

                #region 3D Noise very expensive.
                for (int y = 0; y < ChunkSize.y; y += 1)
                {
                    Vector3 positionAir = new Vector3(x, y, z);
                    if (Voxels.ContainsKey(positionAir)) // Skip ground 0 vox place earlier.
                        continue;

                    Vector3 OffsetGlobal = new Vector3(Offset.x * ChunkSize.x, y, Offset.y * ChunkSize.z); // Global position in noise.
                    float HalfHeighOffset = y / ChunkSize.y / 3; // Compasenting for the waves.
                    float density = pNoise.GetNoise3dv(positionAir + OffsetGlobal) ; // Density in the noise.

                    if (density >= 0)
                    {
                        Vector3 voxelPosition;
                        //if (density >= FloatTreshold) // Make floating bubbles and caves.
                        //{
                        //    voxelPosition = new Vector3(x, y, z);
                        //}
                        //else // Pull the bubbles down on the terrain.
                        //{
                        height += 1;
                        voxelPosition = new Vector3(new Vector3(x, height, z));
                        //}

                        if (!Voxels.ContainsKey(voxelPosition))
                        {
                            // while pulling down the bubles pos might be on existing vox.
                            Voxel vox = new Voxel()
                            {
                                Position = voxelPosition,
                                type = BlockType.rock
                            };
                            //Security
                            if(!Voxels.ContainsKey(voxel.Position))
                                Voxels.Add(voxel.Position, voxel);
                        }
                    }
                } 
                #endregion // Disabled for now.
            }

        // Vegetation pass.
        PlaceTrees();
    }
        
    
    // Generate a randome tree.
    public void PlaceTrees()
    {
        // Select random vox.
        int x = RandomGenerator.RandiRange(0, 15);
        int y;
        int z = RandomGenerator.RandiRange(0, 15);

        Tree tree = new Tree();
        Vector3 rootHeightOffset;
        // Get highest vox at x, y. Starting from the sky until it finds something.
        for (int i = int.Parse(ChunkSize.y.ToString()); i > 0; i--) // From the sky 
            if (Voxels.ContainsKey( new Vector3(x, i - tree.Height, z) ))
            {
                rootHeightOffset = new Vector3(0, i - tree.Height, 0);
                foreach (Voxel voxel in tree.Voxels.Values) // Then place the voxels of the tree.// ofc always security check.
                    if (!Voxels.ContainsKey( voxel.Position + rootHeightOffset ))// Place the voxel finally :). Lucky one.
                    {
                        var vox2 = voxel;
                        var pos = vox2.Position + new Vector3(0, rootHeightOffset.y, 0);
                        vox2.Position += new Vector3(0, rootHeightOffset.y, 0);
                        Voxels.Add(pos, vox2);
                    } 
                         
                return;
            }
    }
}

