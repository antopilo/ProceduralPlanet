﻿using Godot;
using System.Collections;
using System.Collections.Generic;

public class Chunk : Spatial
{
    public static Vector3 ChunkSize = new Vector3(16, 255, 16);

    public Voxel[,,] Voxels = new Voxel[16,255,16];
    public Vector2 Offset;
    public float AverageTemperature = 0;
    public float AverageHumidity = 0;

    public Biomes Biome;

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

        // Filling the chunk with empty voxels.
        for (int x = 0; x < ChunkSize.x; x++)
            for (int y = 0; y < ChunkSize.y; y++)
                for (int z = 0; z < ChunkSize.z; z++)
                {
                    Voxels[x, y, z] = new Voxel()
                    {
                        Active = false, 
                        Type = BlockType.grass
                    };
                }
        
        
        var globalPosition = new Vector2(offsetX * ChunkSize.x, offsetZ * ChunkSize.z);

        // Get Temperature and humidity in chunk.
        AverageTemperature = TemperatureManager.GetTemperature((int)globalPosition.x, (int)globalPosition.y);
        AverageHumidity = TemperatureManager.GetHumidity((int)globalPosition.x, (int)globalPosition.y);

        // GetBiome from temperature and humidity.
        Biome = BiomeManager.GetBiome(AverageTemperature, AverageHumidity);
    }
}

