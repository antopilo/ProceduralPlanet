using Godot;
using ProceduralPlanet.Scripts.Biomes;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public class Generator : Node
{
    private static Vector3 ChunkSize = new Vector3(16, 256, 16);
    private static int CurrentSeed = 0;
    private static OpenSimplexNoise Noise = new OpenSimplexNoise();
    private static OpenSimplexNoise Noise2 = new OpenSimplexNoise();

    public static OpenSimplexNoise ColorNoise = new OpenSimplexNoise();
    private static RandomNumberGenerator rng = new RandomNumberGenerator();

    private static int camX = 0, camZ = 0;
    private Camera Camera;
    private SurfaceTool SurfaceTool;
    private SpatialMaterial VoxMaterial;
    private RichTextLabel InfoLabel;
    private int RenderDistance = 8;


    // Memory
    private static ConcurrentDictionary<Vector2, Chunk> LoadedChunks = new ConcurrentDictionary<Vector2, Chunk>();
    private static ConcurrentDictionary<Vector2, Chunk> PreloadedChunks = new ConcurrentDictionary<Vector2, Chunk>();
    private static ConcurrentQueue<Vector2> toRenderPos = new ConcurrentQueue<Vector2>();

    // Threads array
    private System.Threading.Thread[] Threads = new System.Threading.Thread[4];

    // Vertices of a cube.
    private static Vector3[] Vertices = { new Vector3(0, 0, 0),
                                   new Vector3(1, 0, 0),
                                   new Vector3(1, 0, 1),
                                   new Vector3(0, 0, 1),
                                   new Vector3(0, 1, 0),
                                   new Vector3(1, 1, 0),
                                   new Vector3(1, 1, 1),
                                   new Vector3(0, 1, 1) };


    // Mesh and models
    private ShaderMaterial GrassMaterial;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        // Getting references to other nodes
        Camera = GetNode("../Camera") as Camera;
        InfoLabel = (RichTextLabel)GetNode("../info");
        VoxMaterial = ResourceLoader.Load("res://material/Grass.tres") as SpatialMaterial;

        // Loading models

        GrassMaterial = (ShaderMaterial)ResourceLoader.Load("res://Shaders/grassMaterial.tres");


        // Generating a random seed.
        rng.Randomize();
        GenerateSeed();

        // Starting threads
        Threads[0] = new System.Threading.Thread(new System.Threading.ThreadStart(RenderThread));
        Threads[1] = new System.Threading.Thread(new System.Threading.ThreadStart(Preload));
        Threads[0].Start();
        Threads[1].Start();
        Threads[0].Priority = System.Threading.ThreadPriority.Highest;
    }


    public override void _Process(float delta)
    {
        int camX = int.Parse(Mathf.Stepify(Camera.Transform.origin.x / ChunkSize.x, 1).ToString());
        int camZ = int.Parse(Mathf.Stepify(Camera.Transform.origin.z / ChunkSize.z, 1).ToString());
        var list = new List<Vector2>();
        for (int x = -RenderDistance; x <= RenderDistance; x++)
        {
            for (int y = -RenderDistance; y <= RenderDistance; y++)
            {
                var pos = new Vector2(x + camX, y + camZ);
                if (!PreloadedChunks.ContainsKey(pos) && !LoadedChunks.ContainsKey(pos) && !toRenderPos.Contains(pos))
                    toRenderPos.Enqueue(pos);
            }
        }

        InfoLabel.Clear();
        //InfoLabel.Text += "FPS: " + Engine.GetFramesPerSecond() + "\n";
        InfoLabel.Text += "PreloadedCount: " + PreloadedChunks.Count.ToString() + "\n";
        InfoLabel.Text += "LoadedChunksCount: " + LoadedChunks.Count.ToString() + "\n";
        InfoLabel.Text += "ToRenderCount: " + toRenderPos.Count.ToString() + "\n";
        InfoLabel.Text += "CamPosition: " + "X: " + camX + " Z: " + camZ + "\n";
        InfoLabel.Text += "Current Temperature: " + TemperatureManager.GetTemperature((int)(camX),
                                                                            (int)(camZ)) + "\n";
        InfoLabel.Text += "Current Humidity: " + TemperatureManager.GetHumidity((int)(camX),
                                                                    (int)(camZ)) + "% \n";
        if (LoadedChunks.ContainsKey(new Vector2(camX, camZ)))
            InfoLabel.Text += "Current biome: " + LoadedChunks[new Vector2(camX, camZ)].Biome.ToString();
    }


    // Create a random seed.
    private void GenerateSeed()
    {
        if (CurrentSeed != 0)
            return;

        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();
        CurrentSeed = rng.Seed;

        // Noise.Lacunarity = 0.25f;
        Noise.Seed = CurrentSeed;
        Noise.Octaves = 3;
        Noise.Period = 720;
        Noise.Persistence = 0.75f;

        Noise2.Seed = CurrentSeed;
        Noise2.Octaves = 3;
        Noise2.Period = 512;
        Noise2.Persistence = 0.8f;

        TemperatureManager.TemperatureMap.Seed = CurrentSeed;
        TemperatureManager.TemperatureMap.Octaves = 1;
        TemperatureManager.TemperatureMap.Period = 128;
        TemperatureManager.HumidityMap.Seed = CurrentSeed;
    }


    // Preloading loop.
    public void Preload()
    {
        while (true)
        {
            Vector2 current;
            toRenderPos.TryDequeue(out current);
            if (!PreloadedChunks.ContainsKey(current) && !LoadedChunks.ContainsKey(current))
            {

                Chunk chunk = new Chunk((int)current.x, (int)current.y);
                GetChunkData(chunk);
                PreloadedChunks.TryAdd(chunk.Offset, chunk);
                //PreloadedChunks.TryRemove(chunk.Offset, out chunk);
            }

        }
    }


    // Rendering loop.
    public void RenderThread()
    {
        while (true)
        {
            if (PreloadedChunks.Count <= 1)
                continue;

            foreach (var c in PreloadedChunks)
            {
                if (!ChunkSurrounded(c.Key))
                    continue;

                Chunk c2 = c.Value;
                Render(c2);
            }
        }
    }


    private void Render(Chunk pChunk)
    {
        try
        {
            // If chunk is already loaded.
            if (LoadedChunks.ContainsKey(pChunk.Offset))
                return;

            SurfaceTool = new SurfaceTool();
            SurfaceTool.Begin(Mesh.PrimitiveType.Triangles);

            // Adding material
            Material mat = VoxMaterial.Duplicate() as Material;
            SurfaceTool.SetMaterial(mat);

            // Creating the mesh. Voxel by voxel
            for (int y = 0; y < Chunk.ChunkSize.y; y++)
                for (int x = 0; x < Chunk.ChunkSize.x; x++)
                    for (int z = 0; z < Chunk.ChunkSize.z; z++)
                    {
                        if (!pChunk.Voxels[x, y, z].Active)
                            continue;

                        CreateVoxel(SurfaceTool, x, y, z, pChunk);

                    }

            // Reduces vertex size
            SurfaceTool.Index();

            // Creating instance
            MeshInstance chunk = new MeshInstance
            {
                Mesh = SurfaceTool.Commit(),
                Name = pChunk.Offset.ToString(),
                Translation = new Vector3(pChunk.Offset.x * Chunk.ChunkSize.x, 0, pChunk.Offset.y * Chunk.ChunkSize.z)
            };

            // Creating collisions
            // chunk.CreateTrimeshCollision();

            // Tagging the chunk
            chunk.AddToGroup("Chunk");

            // Chunk is now loaded. Adding to the scene.
            LoadedChunks.TryAdd(pChunk.Offset, pChunk);
            this.CallDeferred("add_child", chunk);

            // Fade in animation.
            //var t2 = new Tween();
            //chunk.AddChild(t2);
            //t2.InterpolateProperty(mat, "albedo_color", new Color(1, 1, 1, 0f), new Color(1, 1, 1, 1f), 1f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
            //t2.Start();

            SurfaceTool.Clear();
        }
        catch
        {
            GD.Print("ERROR IN RENDER.");
        }

    }


    // Create each faces of a single voxel at pPositon in cPosition chunk.
    private void CreateVoxel(SurfaceTool pSurfaceTool, int x, int y, int z, Chunk pChunk)
    {
        // If no voxel is at position, skip
        if (!pChunk.Voxels[x, y, z].Active)
            return;

        // If block is next to each faces
        bool left = x != 0 ? !pChunk.Voxels[x - 1, y, z].Active : true;
        bool right = x != 15 ? !pChunk.Voxels[x + 1, y, z].Active : true;
        bool front = z != 15 ? !pChunk.Voxels[x, y, z + 1].Active : true;
        bool back = z != 0 ? !pChunk.Voxels[x, y, z - 1].Active : true;
        bool top = y != 254 ? !pChunk.Voxels[x, y + 1, z].Active : true;
        bool bottom = y > 0 ? !pChunk.Voxels[x, y - 1, z].Active : true;

        // If voxel is completly surrounded
        if (left && right && front && back && top && bottom)
            return;

        // If the voxel is on the side of a chunk. Check in the neighbor chunk. 
        bool left2 = (x == 0 && PreloadedChunks[new Vector2(pChunk.Offset - new Vector2(1, 0))].Voxels[15, y, z].Active);
        bool right2 = (x == 15 && PreloadedChunks[new Vector2(pChunk.Offset + new Vector2(1, 0))].Voxels[0, y, z].Active);
        bool back2 = (z == 0 && PreloadedChunks[new Vector2(pChunk.Offset - new Vector2(0, 1))].Voxels[x, y, 15].Active);
        bool front2 = (z == 15 && PreloadedChunks[new Vector2(pChunk.Offset + new Vector2(0, 1))].Voxels[x, y, 0].Active);

        // Set right type
        var type = pChunk.Voxels[x, y, z].Type;

        // Local position of the voxel.
        Vector3 vertextOffset = new Vector3(x, y, z);

        // Get colors
        var color = GetVoxelColor(pChunk, vertextOffset, type);
        pSurfaceTool.AddColor(color);

        if (top) // Above
        {
            pSurfaceTool.AddNormal(new Vector3(0, 1, 0));
            pSurfaceTool.AddVertex(Vertices[4] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[5] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[7] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[5] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[6] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[7] + vertextOffset);
        }
        if (right && !right2) // Right
        {
            pSurfaceTool.AddNormal(new Vector3(1, 0, 0));
            pSurfaceTool.AddVertex(Vertices[2] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[5] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[1] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[2] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[6] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[5] + vertextOffset);

        }
        if (left && !left2) // Left
        {
            pSurfaceTool.AddNormal(new Vector3(-1, 0, 0));
            pSurfaceTool.AddVertex(Vertices[0] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[7] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[3] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[0] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[4] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[7] + vertextOffset);
        }
        if (front && !front2) // Front
        {
            pSurfaceTool.AddNormal(new Vector3(0, 0, 1));
            pSurfaceTool.AddVertex(Vertices[3] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[6] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[2] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[3] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[7] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[6] + vertextOffset);
        }
        if (back && !back2) // Above
        {
            pSurfaceTool.AddNormal(new Vector3(0, 0, -1));
            pSurfaceTool.AddVertex(Vertices[0] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[1] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[5] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[5] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[4] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[0] + vertextOffset);
        }
        if (bottom && y != 0)
        {
            pSurfaceTool.AddNormal(new Vector3(0, 1, 0));
            pSurfaceTool.AddVertex(Vertices[1] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[3] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[2] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[1] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[0] + vertextOffset);
            pSurfaceTool.AddVertex(Vertices[3] + vertextOffset);
        }
    }

    public Color GetVoxelColor(Chunk chunk, Vector3 position, BlockType type)
    {
        Color resultColor;
        var temp = ColorNoise.GetNoise3d((chunk.Offset.x * ChunkSize.x) + position.x,
                                        position.y,
                                        (chunk.Offset.y * ChunkSize.z) + position.z);
        switch (type)
        {
            case BlockType.grass: // Grass
                resultColor = new Color("2bc117");
                break;
            case BlockType.rock: // Rock
                resultColor = new Color("555b5b");
                break;
            case BlockType.sand: // Sand
                resultColor = new Color("fffc4f");
                break;
            case BlockType.wood: // Wood
                resultColor = new Color("514230");
                break;
            case BlockType.leaves:
                resultColor = new Color("76db60");
                break;
            case BlockType.snow:
                resultColor = new Color("c0d5f9");
                break;
            default:
                resultColor = new Color("76552b");
                break;
        }
        if (temp > 0)
            resultColor = resultColor.LinearInterpolate(new Color(1, 1, 0), temp);
        else
            resultColor = resultColor.LinearInterpolate(new Color(1, 1, 1), Mathf.Abs(temp));


        return resultColor;
    }

    public void GetChunkData(Chunk chunk)
    {
        // Chunk global position.
        var Offset = chunk.Offset;

        // Settings !
        BiomeManager.SetBiomeSettings(chunk.Biome);

        bool placedTree = false;


        for (int z = 0; z < ChunkSize.z; z += 1)
            for (int x = 0; x < ChunkSize.x; x += 1)
            {
                // Global position of the cube.
                int gX = ((int)Offset.x * (int)Chunk.ChunkSize.x) + x;
                int gZ = ((int)Offset.y * (int)Chunk.ChunkSize.z) + z;
                float noiseResult = (Noise.GetNoise2d(gX, gZ) + 1f) * ChunkSize.y;
                float height = Mathf.Clamp(Mathf.Stepify(noiseResult * BiomeSettings.TerrainAmplitude, 1), 0, 254);

                // Default type
                BlockType type = BiomeSettings.DefaultBlocktype;

                // Filling under the chunk too.
                for (int i = 0; i <= height; i++)
                {
                    chunk.Voxels[x, i, z].Active = true;
                    chunk.Voxels[x, i, z].Type = type;
                }

                if (BiomeSettings.Mountains)
                    GenerateMountains(chunk, x, z, (int)Mathf.Clamp(height, 0f, 254f));

                // Add 6 layers of grass on top of the generated rock.
                var pos = chunk.HighestAt(x, z);
                for (int i = 0; i < BiomeSettings.TopLayerThickness; i++)
                {
                    // Adding some dirt under top layer.
                    var newType = BiomeSettings.UnderLayerType;

                    // if highest block, its grass!
                    if (i == 0)
                        newType = BiomeSettings.TopLayerType;

                    // Placing block. Making sure its under 255 height.
                    chunk.Voxels[x, Mathf.Clamp(pos - i, 0, 254), z].Type = newType;
                }

                // Placing decoration
                int decorationChance = rng.RandiRange(1, 100);
                if (decorationChance < BiomeSettings.DecorationRate)
                {
                    // Placing point
                    int dy = chunk.HighestAt(x, z) + 1;

                    // Creating node and mesh.
                    var meshInstance = new MeshInstance();
                    meshInstance.Mesh = (ArrayMesh)ResourceLoader.Load(BiomeSettings.DecorationModel);

                    // Adding next frame. Let godot handle lock.
                    CallDeferred("add_child", meshInstance);

                    // Moving next frame because not in tree yet
                    meshInstance.SetDeferred("translation",new Vector3(x + (Offset.x * ChunkSize.x), dy,
                                                                            z + (Offset.y * ChunkSize.z)));

                    // Applying wind shader.
                    meshInstance.SetDeferred("material_override", GrassMaterial);

                    // Scaling down because its a decoration
                    meshInstance.Scale = meshInstance.Scale /= 8;
                    continue;
                }

                if (placedTree)
                    continue;

            }
    }
    
    // TODO: Make BiomeSettings non static for multiple generation pass...
    // TODO: Make Biome interpolation. Best biome match interp.
    // TODO: Place trees into the chunk array.
    public void GenerateVegetation(Chunk chunk)
    {
        bool placedTree = false;
        for (int x = 0; x < ChunkSize.x; x++)
        {
            for (int z = 0; z < ChunkSize.z; z++)
            {

                // Placing trees
                float treeChance = rng.RandfRange(1f, 100f);
                if (treeChance < BiomeSettings.TreeRate)
                {
                    var file = (ArrayMesh)ResourceLoader.Load(BiomeSettings.TreeModel);
                    foreach (var item in file.GetMetaList())
                    {
                        if (item == "voxel_size")
                            continue;

                        int ty = chunk.HighestAt(x, z) + 1;

                        var array = item.Split(",");
                        var result = new Vector3(Mathf.Abs(int.Parse(array[0].Right(1))),
                                                Mathf.Abs(int.Parse(array[1])),
                                                Mathf.Abs(int.Parse(array[2].Left(array[2].Length - 1))));

                        var voxelPosition = result + new Vector3(x, ty, z);

                        if ((voxelPosition.x < 0 || voxelPosition.x >= 16) ||
                            (voxelPosition.y < 0 || voxelPosition.y >= 255) ||
                            (voxelPosition.z < 0 || voxelPosition.z >= 16))

                            continue;
                        GD.Print(voxelPosition);
                        chunk.Voxels[(int)voxelPosition.x, (int)voxelPosition.y, (int)voxelPosition.z].Active = true;
                    }
                    //// Placing point.

                    //// Creating and setting the mesh.
                    //var meshInstance = new MeshInstance();
                    //meshInstance.Mesh = 

                    //// Adding it next frame.(safe with mutex lock)
                    //CallDeferred("add_child", meshInstance);
                    //// Moving it next frame(not in the tree yet :))
                    //meshInstance.SetDeferred("translation", new Vector3(x + (Offset.x * ChunkSize.x) -10, ty,
                    //                                                        z + (Offset.y * ChunkSize.z + 10)));
                    placedTree = true;
                }
            }
        }
    }


    public void GenerateMountains(Chunk chunk, int x, int z, int height)
    {
        var previousDensity = 0f;
        for (int y = (int)height; y < (int)ChunkSize.y - 1; y++)
        {
            if (chunk.Voxels[(int)x, (int)y, (int)z].Active) // Skip ground 0 vox place earlier.
                continue;

            int gX = (int)(chunk.Offset.x * Chunk.ChunkSize.x), gZ = (int)(chunk.Offset.y * Chunk.ChunkSize.z);
            var density = Noise2.GetNoise3d(x + gX, y, z + gZ);
            //var densityAbove = Noise2.GetNoise3d(x + gX, y + 1, z + gZ);
            //var modifier = (y - height) / 32;
            if (density >= 0.25 )
            {
                height++;
                chunk.Voxels[x, height, z].Type = BlockType.rock;
               

                if (!chunk.Voxels[x, height, z].Active)
                    chunk.Voxels[x, height, z].Active = true;
            }
            previousDensity = density;
        }
    }


    public static float DistanceToChunk(Vector2 chunkPosition)
    {
        var d = (new Vector2(camX, camZ) - chunkPosition).Length();
        return d;
    }


    // Returns true if the chunk is surrounded by loaded chunks.
    public bool ChunkSurrounded(Vector2 position)
    {
        try
        {
            var left = PreloadedChunks.ContainsKey(position + new Vector2(1, 0)) || LoadedChunks.ContainsKey(position + new Vector2(1, 0));
            var right = PreloadedChunks.ContainsKey(position - new Vector2(1, 0)) || LoadedChunks.ContainsKey(position - new Vector2(1, 0));
            var behind = PreloadedChunks.ContainsKey(position + new Vector2(0, 1)) || LoadedChunks.ContainsKey(position + new Vector2(0, 1));
            var front = PreloadedChunks.ContainsKey(position - new Vector2(0, 1)) || LoadedChunks.ContainsKey(position - new Vector2(0, 1));
            return left && right && behind && front;
        }
        catch
        {
            return false;
        }
    }
}


