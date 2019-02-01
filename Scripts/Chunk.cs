using System;
using Godot;
using Godot.Collections;

public class Chunk
{
    public Vector2 Offset;
    public Vector3 ChunkSize = new Vector3(16, 255, 16);
    public Dictionary Voxels = new Dictionary();

    public Chunk(Vector2 pOffset)
    {
        Offset = pOffset;
    }

    public Dictionary Preload(OpenSimplexNoise pNoise)
    {
        for (int z = 0; z < ChunkSize.z; z += 1)
        {
            for (int x = 0; x < ChunkSize.x; x += 1)
            {
                var height = Mathf.Stepify((pNoise.GetNoise2d((Offset.x * ChunkSize.x) + x, (Offset.y * ChunkSize.z) + z) + 1) * ChunkSize.y / 2, 1);
                var pos = new Vector3(x, height, z);

                Voxels.Add(pos, pos);
                Voxels.Add(pos - new Vector3(0, 1, 0), pos - new Vector3(0, 1, 0));
                Voxels.Add(pos - new Vector3(0, 2, 0), pos - new Vector3(0, 2, 0));

                for (int y = 0; y < ChunkSize.y; y += 1)
                {
                    if (pNoise.GetNoise3dv(pos + new Vector3((Offset.x * ChunkSize.x),0,(Offset.y * ChunkSize.z))) - ((y / ChunkSize.y) / 4) >= 0)
                    {
                        height += 1;
                        var val = new Vector3(x , height, z);
                        Voxels.Add(val, val);
                    }
                }
            }
        }
        return Voxels;
    }
}

