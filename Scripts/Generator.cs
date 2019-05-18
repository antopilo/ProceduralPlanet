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

        // Water
        //Water = new MeshInstance();
        //WaterMesh = new PlaneMesh();
        //WaterMesh.Size = new Vector2(ChunkSize.x * RenderDistance * 32, ChunkSize.z * RenderDistance * 32);
        //WaterMesh.SetMaterial(WaterMaterial);
        //Water.SetMesh(WaterMesh);
        //Water.Translation = new Vector3(0, WaterLevel - WaterOffset, 0);
        //AddChild(Water);

        //camX = (int)Mathf.Stepify(Camera.Transform.origin.x / ChunkSize.x, 1);
        //camZ = (int)Mathf.Stepify(Camera.Transform.origin.z / ChunkSize.z, 1);
        //for (int x = -RenderDistance + camX; x < RenderDistance + camX ; x++)
        //    for (int z = -RenderDistance + camZ ; z < RenderDistance + camZ ; z++)
        //        if (!PreloadedChunks.ContainsKey(new Vector2(x, z)) && !LoadedChunks.ContainsKey(new Vector2(x, z)) && !toRenderPos.Contains(new Vector2(x, z)))
        //        {
        //            toRenderPos.Enqueue(new Vector2(x, z));
        //        }



        Threads[0] = new System.Threading.Thread(new System.Threading.ThreadStart(EndPreload));
        Threads[2] = new System.Threading.Thread(new System.Threading.ThreadStart(RenderThread));
        Threads[0].Start();
        Threads[2].Start();
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

    public void EndPreload()
    {
        while (true)
        {
            foreach (var c in toRenderPos)
            {
                Chunk c1 = new Chunk(c, Noise);
                if (!PreloadedChunks.ContainsKey(c1.Offset) && !LoadedChunks.ContainsKey(c1.Offset))
                {
                    GetChunkData(c1);
                    toRenderPos.TryDequeue(out c1.Offset);
                    PreloadedChunks.TryAdd(c1.Offset, c1);
                    
                }
            }
        }
    }

    public void RenderThread()
    {
        while (true)
        {
            foreach (var c in PreloadedChunks.OrderBy(c=> DistanceToChunk(c.Key)))
            {
                Chunk c2 = c.Value;
                Render(c2);
                PreloadedChunks.TryRemove(c.Key, out c2);
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
            Material mat = VoxMaterial.Duplicate() as Material;
            SurfaceTool.SetMaterial(mat);

            for (int x = 0; x < pChunk.ChunkSize.x; x++)
                for (int y = 0; y < pChunk.ChunkSize.y; y++)
                    for (int z = 0; z < pChunk.ChunkSize.z; z++)
                        CreateVoxel(SurfaceTool, new Vector3(x, y, z), pChunk);
            SurfaceTool.Index();

            MeshInstance chunk = new MeshInstance
            {
                Mesh = SurfaceTool.Commit(),
                Name = pChunk.Offset.ToString(),
                Translation = new Vector3(pChunk.Offset.x * pChunk.ChunkSize.x, 0, pChunk.Offset.y * pChunk.ChunkSize.z)
            };
            chunk.CreateTrimeshCollision();
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

        //foreach (var cl in LoadedChunks.OrderByDescending(c => DistanceToChunk(c.Key)))
        //{
        //    if (DistanceToChunk(cl.Key) >= RenderDistance)
        //    {
        //        unloadChunk
        //        Chunk c2 = cl.Value;
        //        LoadedChunks.TryRemove(cl.Key, out c2);
        //        GetNode(cl.Key.ToString()).QueueFree();
        //    }
        //}
    }

    
    // Create each faces of a single voxel at pPositon in cPosition chunk.
    // TODO: Remove Surfacetool parameter.
    private static void CreateVoxel(SurfaceTool pSurfaceTool, Vector3 pPosition, Chunk pChunk)
    {
        if (!pChunk.Voxels[(int)pPosition.x, (int)pPosition.y, (int)pPosition.z].Active)
            return;

        var type = pChunk.Voxels[(int)pPosition.x, (int)pPosition.y, (int)pPosition.z].Type;
        // Colors
        switch (type)
        {
            case BlockType.grass: // Grass
                pSurfaceTool.AddColor(new Color("6aff26").Darkened(pPosition.y / 256 / 50));
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

        bool left = pPosition.x != 0 ? !pChunk.Voxels[(int)pPosition.x - 1, (int)pPosition.y, (int)pPosition.z].Active : true;
        bool right = pPosition.x != 15 ? !pChunk.Voxels[(int)pPosition.x + 1, (int)pPosition.y, (int)pPosition.z].Active : true;
        bool front = pPosition.z != 15 ? !pChunk.Voxels[(int)pPosition.x, (int)pPosition.y, (int)pPosition.z + 1].Active : true;
        bool back = pPosition.z != 0 ? !pChunk.Voxels[(int)pPosition.x, (int)pPosition.y, (int)pPosition.z - 1].Active : true;
        bool top = pPosition.y != 0 ?  !pChunk.Voxels[(int)pPosition.x, (int)pPosition.y + 1, (int)pPosition.z].Active : true;
        bool bottom = pPosition.y != 254 ? !pChunk.Voxels[(int)pPosition.x, (int)pPosition.y - 1, (int)pPosition.z ].Active : true;

        // If block is hidden.
        //if (left && right && front && back && top && bottom || pPosition.y < 0)
        //  return;

        if (top) // Above
        {
            pSurfaceTool.AddNormal(new Vector3(0, 1, 0));
            pSurfaceTool.AddVertex(Vertices[4] + pPosition);
            pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            pSurfaceTool.AddVertex(Vertices[7] + pPosition);
            pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            pSurfaceTool.AddVertex(Vertices[7] + pPosition);
        }
        if (right) // Right
        {
            pSurfaceTool.AddNormal(new Vector3(1, 0, 0));
            pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            pSurfaceTool.AddVertex(Vertices[1] + pPosition);
            pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            //if (pPosition.x != ChunkSize.x - 1)
            //{
            //        pSurfaceTool.AddNormal(new Vector3(1, 0, 0));
            //        pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[1] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            //}
            //else if(PreloadedChunks.ContainsKey(pChunk.Offset + new Vector2(1, 0)))
            //{
            //    Chunk nb = PreloadedChunks[pChunk.Offset + new Vector2(1, 0)] as Chunk;
            //    if (!nb.Voxels.ContainsKey(new Vector3(0, pPosition.y, pPosition.z)))
            //    {
            //        pSurfaceTool.AddNormal(new Vector3(1, 0, 0));
            //        pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[1] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            //    }
            //}
        }
        if (left) // Left
        {
            pSurfaceTool.AddNormal(new Vector3(-1, 0, 0));
            pSurfaceTool.AddVertex(Vertices[0] + pPosition);
            pSurfaceTool.AddVertex(Vertices[7] + pPosition);
            pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            pSurfaceTool.AddVertex(Vertices[0] + pPosition);
            pSurfaceTool.AddVertex(Vertices[4] + pPosition);
            pSurfaceTool.AddVertex(Vertices[7] + pPosition);
        }
        if (front) // Front
        {
            pSurfaceTool.AddNormal(new Vector3(0, 0, 1));
            pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            pSurfaceTool.AddVertex(Vertices[7] + pPosition);
            pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            //if (pPosition.z != ChunkSize.z - 1)
            //{
            //    pSurfaceTool.AddNormal(new Vector3(0, 0, 1));
            //    pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            //    pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            //    pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            //    pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            //    pSurfaceTool.AddVertex(Vertices[7] + pPosition);
            //    pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            //}
            //else if (PreloadedChunks.ContainsKey(pChunk.Offset + new Vector2(0, 1)))
            //{
            //    Chunk nb = PreloadedChunks[pChunk.Offset + new Vector2(0, 1)] as Chunk;
            //    if (!nb.Voxels.ContainsKey(new Vector3(pPosition.x, pPosition.y, 0)))
            //    {
            //        pSurfaceTool.AddNormal(new Vector3(0, 0, 1));
            //        pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[7] + pPosition);
            //        pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            //    }
            //}
        }
        if (back) // Above
        {
            pSurfaceTool.AddNormal(new Vector3(0, 0, -1));
            pSurfaceTool.AddVertex(Vertices[0] + pPosition);
            pSurfaceTool.AddVertex(Vertices[1] + pPosition);
            pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            pSurfaceTool.AddVertex(Vertices[4] + pPosition);
            pSurfaceTool.AddVertex(Vertices[0] + pPosition);
        }
        if (bottom)
        {
            pSurfaceTool.AddNormal(new Vector3(0, 1, 0));
            pSurfaceTool.AddVertex(Vertices[1] + pPosition);
            pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            pSurfaceTool.AddVertex(Vertices[1] + pPosition);
            pSurfaceTool.AddVertex(Vertices[0] + pPosition);
            pSurfaceTool.AddVertex(Vertices[3] + pPosition);
        }
    }


    public void GetChunkData(Chunk chunk)
    {
        var Offset = chunk.Offset;

        for (int z = 0; z < ChunkSize.z ; z += 1)
            for (int x = 0; x < ChunkSize.x ; x += 1)
            {
                // Global position in the noise.
                Vector2 globalPosition = new Vector2((Offset.x * ChunkSize.x) + x, (Offset.y * ChunkSize.z) + z);
                float height = Mathf.Stepify(((Noise.GetNoise2dv(globalPosition) + 0.5f) * (ChunkSize.y / 2) ), 1);
                BlockType type = BlockType.grass;
                if (height <= WaterLevel)
                    type = BlockType.sand;
                for (int i = (int)height - 5; i <= height; i++)
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
            Vector3 positionAir = new Vector3(x, y, z);
            if (chunk.Voxels[(int)x, (int)y, (int)z].Active) // Skip ground 0 vox place earlier.
                continue;

            Vector3 OffsetGlobal = new Vector3(chunk.Offset.x * ChunkSize.x, 0, chunk.Offset.y * ChunkSize.z); // Global position in noise.
            float density = Noise2.GetNoise3dv(positionAir + OffsetGlobal); // Density in the noise.
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


