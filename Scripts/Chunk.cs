using Godot;
using Godot.Collections;

public class Chunk : Object
{
    private float FloatTreshold = 0.1f;
    private int CarpetMaxHeight = 128;
    public Vector2 Offset;
    public Vector3 ChunkSize = new Vector3(16, 255, 16);
    public Dictionary Voxels = new Dictionary();

    public Chunk(Vector2 pOffset, Dictionary pQueue, OpenSimplexNoise pNoise)
    {
        if (pQueue.ContainsKey(pOffset)) return;
        Offset = pOffset;
        Preload(pNoise);
        pQueue.Add(Offset, this);
    }

    public Dictionary Preload(OpenSimplexNoise pNoise)
    {
        for (int z = 0; z < ChunkSize.z; z += 1)
        {
            for (int x = 0; x < ChunkSize.x; x += 1)
            {
                var heightMap = int.Parse(Mathf.Stepify(pNoise.GetNoise2d((Offset.x * ChunkSize.x) + x, (Offset.y * ChunkSize.z) + z) * CarpetMaxHeight, 1).ToString());
                var voxHeight = new Vector3(x, float.Parse(heightMap.ToString()), z);
                Voxels.Add(new Vector3(x,0,z), new Vector3(x, 0, z));
                
                for (int y = 0; y < ChunkSize.y; y += 1)
                {
                    var positionAir = new Vector3(x, y, z);
                    if (Voxels.ContainsKey(positionAir)) // Skip ground 0 vox place earlier.
                        continue;

                    var OffsetGlobal = new Vector3(Offset.x * ChunkSize.x, y, Offset.y * ChunkSize.z); // Global position in noise.
                    var HalfHeighOffset = (y / ChunkSize.y / 3); // Compasenting for the waves.
                    float density = pNoise.GetNoise3dv(positionAir + OffsetGlobal) - HalfHeighOffset; // Density in the noise.
                    if (density >= 0)
                    {
                        Vector3 val;
                        if (density >= FloatTreshold) // Make floating bubbles and caves.
                        {
                            val = new Vector3(x, y, z);
                        } 
                        else // Pull the bubbles down on the terrain.
                        {
                            heightMap += 1;
                            val = voxHeight;
                        }
                            
                        if (!Voxels.ContainsKey(val)) // while pulling down the bubles pos might be on existing vox.
                            Voxels.Add(val, val);
                    }
                }
            }
        }

        return Voxels;
    }
}

