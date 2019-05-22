using Godot;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

public class Generator : Node
{
    [Export] int CurrentSeed = 0;
    // Noises
    private static OpenSimplexNoise Noise = new OpenSimplexNoise();
    private static OpenSimplexNoise Noise2 = new OpenSimplexNoise();
    private static RandomNumberGenerator rng = new RandomNumberGenerator();
    private Camera Camera;
    SurfaceTool SurfaceTool;
    private SpatialMaterial VoxMaterial;
    private SpatialMaterial WaterMaterial;
    private RichTextLabel InfoLabel;

    private static Vector3 ChunkSize = new Vector3(16, 256, 16);
    Chunk c1;

    [Export] private int RenderDistance = 2;
    [Export] private int PreloadDistance = 2;

    // Chunks
    private ConcurrentDictionary<Vector2, Chunk> LoadedChunks = new ConcurrentDictionary<Vector2, Chunk>();
    private ConcurrentDictionary<Vector2, Chunk> PreloadedChunks = new ConcurrentDictionary<Vector2, Chunk>();
    private ConcurrentQueue<Vector2> toRenderPos = new ConcurrentQueue<Vector2>();
    private ConcurrentQueue<MeshInstance> ChildQueue = new ConcurrentQueue<MeshInstance>();

    private System.Threading.Thread[] Threads = new System.Threading.Thread[4];

    // Water
    private MeshInstance Water;
    private PlaneMesh WaterMesh;

    
    
    private float WaterOffset = 0.5f;
    public static int WaterLevel = 70;

    private float FloatTreshold = 0.1f;
    private static int CarpetMaxHeight = 60;
    private int camX = 0, camZ = 0;
    private bool Preloading = true;
    float tick = 0;
    [Export] float UpdateRate = 0.01f;
    private static Vector3[] Vertices = { new Vector3(0, 0, 0),
                                   new Vector3(1, 0, 0),
                                   new Vector3(1, 0, 1),
                                   new Vector3(0, 0, 1),
                                   new Vector3(0, 1, 0),
                                   new Vector3(1, 1, 0),
                                   new Vector3(1, 1, 1),
                                   new Vector3(0, 1, 1) };


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        rng.Randomize();
        Camera = GetNode("../Camera") as Camera;
        InfoLabel = (RichTextLabel)GetNode("../info");
        VoxMaterial = ResourceLoader.Load("res://material/Grass.tres") as SpatialMaterial;
        //WaterMaterial = ResourceLoader.Load("res://material/water.tres") as SpatialMaterial;

        GenerateSeed();

        Threads[1] = new System.Threading.Thread(new System.Threading.ThreadStart(RenderThread));
        Threads[2] = new System.Threading.Thread(new System.Threading.ThreadStart(Preload));
        Threads[1].Start();
        Threads[2].Start();
        Threads[1].Priority = System.Threading.ThreadPriority.Highest;

    }

    /// <summary>
    ///  Create a random seed.
    /// </summary>
    private void GenerateSeed()
    {
        if (CurrentSeed != 0)
            return;

        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();
        CurrentSeed = rng.Seed;

        // Noise.Lacunarity = 0.25f;
        Noise.Seed = CurrentSeed;
        Noise.Octaves = 4;
        Noise.Period = 1024;
        Noise.Persistence = 0.75f;

        Noise2.Seed = CurrentSeed;
        Noise2.Octaves = 4;
        Noise2.Period = 256;
        Noise2.Persistence = 0.7f;
    }

    public void Preload()
    {
        while (true)
        {
            foreach (var chunkPosition in toRenderPos)
            {
                if (!PreloadedChunks.ContainsKey(chunkPosition) && !LoadedChunks.ContainsKey(chunkPosition))
                {
                    Chunk chunk = new Chunk((int)chunkPosition.x, (int)chunkPosition.y);
                    GetChunkData(chunk);
                    PreloadedChunks.TryAdd(chunk.Offset, chunk);
                    toRenderPos.TryDequeue(out chunk.Offset);
                    
                    //PreloadedChunks.TryRemove(chunk.Offset, out chunk);
                }
                
            }
        }
    }

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

    public void RenderThread()
    {
        while (true)
        {
            foreach (var c in PreloadedChunks)
            {
                if (!ChunkSurrounded(c.Key))
                    continue;

                Chunk c2 = c.Value;
                Render(c2);
                //PreloadedChunks.TryRemove(c.Key, out c2);
            }
        }
    }

    private void Render(Chunk pChunk)
    {
        try
        {
            if (LoadedChunks.ContainsKey(pChunk.Offset))
                return;

            SurfaceTool = new SurfaceTool();
            SurfaceTool.Begin(Mesh.PrimitiveType.Triangles);
            Material mat = VoxMaterial as Material;
            SurfaceTool.SetMaterial(mat);


            for (int y = 0; y < Chunk.ChunkSize.y; y++)
            {
                if ((y + 1) % 16 == 0 && !pChunk.GetFlag(y))
                {
                    y += 16;
                }

                for (int x = 0; x < Chunk.ChunkSize.x; x++)
                    for (int z = 0; z < Chunk.ChunkSize.z; z++)
                    {


                        if (!pChunk.Voxels[x, y, z].Active)
                            continue;
                        CreateVoxel(SurfaceTool, x, y, z, pChunk);

                    }
            }

            SurfaceTool.Index();

            MeshInstance chunk = new MeshInstance
            {
                Mesh = SurfaceTool.Commit(),
                Name = pChunk.Offset.ToString(),
                Translation = new Vector3(pChunk.Offset.x * Chunk.ChunkSize.x, 0, pChunk.Offset.y * Chunk.ChunkSize.z)
            };
            //chunk.CreateTrimeshCollision();
            chunk.AddToGroup("Chunk");

            LoadedChunks.TryAdd(pChunk.Offset, pChunk);
            AddChild(chunk);

            //Tween t = new Tween();
            //AddChild(t);
            ////t.InterpolateProperty(chunk, "scale", new Vector3(0, 0, 0), new Vector3(1, 1, 1), 1f, Tween.TransitionType.Expo, Tween.EaseType.Out);
            ////t.InterpolateProperty(chunk, "translation", chunk.Translation - new Vector3(0, 255 * 2, 0), chunk.Translation, 1f, Tween.TransitionType.Expo, Tween.EaseType.Out);
            //t.InterpolateProperty(chunk, "albedo_color", new Color(1, 1, 1, 0), new Color(1, 1, 1, 1), 2f, Tween.TransitionType.Linear, Tween.EaseType.InOut);
            ////chunk.Scale = new Vector3(0, 0, 0);
            //t.Start();
            SurfaceTool.Clear();



        }
        catch { GD.Print("ERROR IN RENDER."); }

    }

    public override void _Process(float delta)
    {
        camX = (int)Mathf.Stepify(Camera.GlobalTransform.origin.x / ChunkSize.x, 1);
        camZ = (int)Mathf.Stepify(Camera.GlobalTransform.origin.z / ChunkSize.z, 1);
        for (int x = camX - RenderDistance; x < camX + RenderDistance ; x++)
            for (int z = camZ - RenderDistance ; z < camZ + RenderDistance ; z++)
                if (!PreloadedChunks.ContainsKey(new Vector2(x, z)) && !LoadedChunks.ContainsKey(new Vector2(x, z)) && !toRenderPos.Contains(new Vector2(x, z)))
                {
                    toRenderPos.Enqueue(new Vector2(x, z));
                }

        InfoLabel.Clear();
        InfoLabel.Text += "FPS: " + Engine.GetFramesPerSecond() + "\n";
        InfoLabel.Text += "PreloadedCount: " + PreloadedChunks.Count.ToString() + "\n";
        InfoLabel.Text += "LoadedChunksCount: " + LoadedChunks.Count.ToString() + "\n";
        InfoLabel.Text += "ToRenderCount: " + toRenderPos.Count.ToString() + "\n";
        InfoLabel.Text += "CamPosition: " + "X: " + camX + " Z: " + camZ + "\n";

        //int count = 0;
        //foreach (var cl in LoadedChunks)
        //{
        //    if (count >= 8)
        //        return;
        //    if (DistanceToChunk(cl.Key) >= RenderDistance)
        //    {

        //        Chunk c2 = cl.Value;
        //        LoadedChunks.TryRemove(cl.Key, out c2);
        //        GetNode(cl.Key.ToString()).QueueFree();
        //        count++;
        //    }
        //}
    }

    
    // Create each faces of a single voxel at pPositon in cPosition chunk.
    // TODO: Remove Surfacetool parameter.
    private void CreateVoxel(SurfaceTool pSurfaceTool, int x, int y, int z, Chunk pChunk)
    {
        if (!pChunk.Voxels[x, y ,z].Active)
            return; 
            
        bool left   = x != 0 ? !pChunk.Voxels[x - 1, y, z].Active : true;
        bool right  = x != 15 ? !pChunk.Voxels[x + 1, y, z].Active : true;
        bool front  = z != 15 ? !pChunk.Voxels[x, y, z + 1].Active : true;
        bool back   = z != 0 ? !pChunk.Voxels[x, y, z - 1].Active : true;
        bool top    = y != 254 ? !pChunk.Voxels[x, y + 1, z].Active : true;
        bool bottom = y > 0   ? !pChunk.Voxels[x, y - 1, z].Active : true;

        if (left && right && front && back && top && bottom)
            return;

        bool left2 = (x == 0 && PreloadedChunks[new Vector2(pChunk.Offset - new Vector2(1, 0))].Voxels[15, y, z].Active);
        bool right2 = (x == 15 && PreloadedChunks[new Vector2(pChunk.Offset + new Vector2(1, 0))].Voxels[0, y, z].Active);
        bool back2 = (z == 0 && PreloadedChunks[new Vector2(pChunk.Offset - new Vector2(0, 1))].Voxels[x, y, 15].Active);
        bool front2 = (z == 15 && PreloadedChunks[new Vector2(pChunk.Offset + new Vector2(0, 1))].Voxels[x, y, 0].Active);

        //GD.Print(PreloadedChunks[new Vector2(pChunk.Offset - new Vector2(1, 0))].Voxels[15, y, z].Active);
        

        var type = pChunk.Voxels[x, y, z].Type;

        // Colors
        switch (type)
        {
            case BlockType.grass: // Grass
                pSurfaceTool.AddColor(new Color("6aff26").Darkened(y / 256 / 50));
                break;
            case BlockType.rock: // Rock
                pSurfaceTool.AddColor(new Color("3d475b"));
                break;
            case BlockType.sand: // Sand
                pSurfaceTool.AddColor(new Color("ffe23a"));
                break;
            case BlockType.wood: // Wood
                pSurfaceTool.AddColor(new Color("3e2731"));
                break;
            case BlockType.leaves:
                pSurfaceTool.AddColor(new Color("76db60"));
                break;
            default:
                pSurfaceTool.AddColor(new Color("76552b"));
                break;
        }

        Vector3 vertextOffset = new Vector3(x, y, z);
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


    public void GetChunkData(Chunk chunk)
    {
        var Offset = chunk.Offset;
        for (int z = 0; z < ChunkSize.z ; z += 1)
            for (int x = 0; x < ChunkSize.x ; x += 1)
            {
                // Global position in the noise.
                
                int gX = ((int)Offset.x * (int)Chunk.ChunkSize.x) + x, gZ = ((int)Offset.y * (int)Chunk.ChunkSize.z) + z;
                float height = Mathf.Stepify(((Noise.GetNoise2d(gX, gZ) + 0.5f) * (ChunkSize.y / 2) ), 1);
                BlockType type = BlockType.grass;

                if (height <= WaterLevel)
                    type = BlockType.sand;

                for (int i = 0; i <= height; i++)
                {
                    chunk.Voxels[x, i, z].Active = true;
                    chunk.Voxels[x, i, z].Type = type;
                }

                GenerateMountains(chunk, x, z, height);
            }
    }

    public void GenerateMountains(Chunk chunk, float x, float z, float height)
    {
        for (int y = (int)height; y < (int)ChunkSize.y - 1; y++)
        {
            if (chunk.Voxels[(int)x, (int)y, (int)z].Active) // Skip ground 0 vox place earlier.
                continue;

            Vector3 OffsetGlobal = new Vector3(chunk.Offset.x * ChunkSize.x, 0, chunk.Offset.y * ChunkSize.z); // Global position in noise.
            float density = Noise2.GetNoise3d(x + OffsetGlobal.x, y + OffsetGlobal.y, z + OffsetGlobal.z); // Density in the noise.
            float DensityModifier = ((y / 3.33f / ChunkSize.y) * 2);
            if (density - DensityModifier >= 0 )
            {
                height += 1;
                chunk.Voxels[(int)x, (int)height, (int)z].Type = BlockType.rock;

                if (y < WaterLevel)
                    chunk.Voxels[(int)x, (int)height, (int)z].Type  = BlockType.sand;

                if (!chunk.Voxels[(int)x, (int)height, (int)z].Active)
                    chunk.Voxels[(int)x, (int)height, (int)z].Active = true;
            }
        }
    }

    public float DistanceToChunk(Vector2 chunkPosition)
    {
        var d = (new Vector2(camX, camZ) - chunkPosition).Length();
        return d;
    }

    private void SpiralLoader()
    {
        int camX = int.Parse(Mathf.Stepify(Camera.Transform.origin.x / ChunkSize.x, 1).ToString());
        int camZ = int.Parse(Mathf.Stepify(Camera.Transform.origin.z / ChunkSize.z, 1).ToString());
        int numElements = PreloadDistance * PreloadDistance + 1;
        int x = 0;
        int y = 0;
        int dx = 1;
        int dy = 0;
        int xLimit = PreloadDistance - 0;
        int yLimit = PreloadDistance - 1;
        int counter = 0;

        int currentLength = 1;
        while (counter < numElements)
        {
            if (!PreloadedChunks.ContainsKey(new Vector2(x + camX, y + camZ)) && !LoadedChunks.ContainsKey(new Vector2(x + camX, y + camZ)) && !toRenderPos.Contains(new Vector2(x + camX, y + camZ)))
            {
                toRenderPos.Enqueue(new Vector2(x + camX, y + camZ));

            }
            x += dx;
            y += dy;

            currentLength++;
            if (dx > 0)
            {
                if (currentLength >= xLimit)
                {
                    dx = 0;
                    dy = 1;
                    xLimit--;
                    currentLength = 0;
                }
            }
            else if (dy > 0)
            {
                if (currentLength >= yLimit)
                {
                    dx = -1;
                    dy = 0;
                    yLimit--;
                    currentLength = 0;
                }
            }
            else if (dx < 0)
            {
                if (currentLength >= xLimit)
                {
                    dx = 0;
                    dy = -1;
                    xLimit--;
                    currentLength = 0;
                }
            }
            else if (dy < 0)
            {
                if (currentLength >= yLimit)
                {
                    dx = 1;
                    dy = 0;
                    yLimit--;
                    currentLength = 0;
                }
            }
            counter++;
        }
    }
}


