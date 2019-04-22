using Godot;
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

    private static Vector3 ChunkSize = new Vector3(16, 256, 16);
    Chunk c1;

    [Export] private int RenderDistance = 5;
    [Export] private int PreloadDistance = 6;

    // Chunks
    private Dictionary<Vector2, Chunk> LoadedChunks = new Dictionary<Vector2, Chunk>();
    private Dictionary<Vector2, Chunk> PreloadedChunks = new Dictionary<Vector2, Chunk>();


    private System.Threading.Thread[] Threads = new System.Threading.Thread[4];

    // Water
    private MeshInstance Water;
    private PlaneMesh WaterMesh;

    private List<Vector2> toRenderPos = new List<Vector2>();

    private float WaterOffset = 0.5f;
    public static int WaterLevel = 70;

    private float FloatTreshold = 0.1f;
    private static int CarpetMaxHeight = 60;

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
        VoxMaterial = ResourceLoader.Load("res://material/Grass.tres") as SpatialMaterial;
        WaterMaterial = ResourceLoader.Load("res://material/water.tres") as SpatialMaterial;

        GenerateSeed();

        // Water
        Water = new MeshInstance();
        WaterMesh = new PlaneMesh();
        WaterMesh.Size = new Vector2(ChunkSize.x * RenderDistance * 32, ChunkSize.z * RenderDistance * 32);
        WaterMesh.SetMaterial(WaterMaterial);
        Water.SetMesh(WaterMesh);
        Water.Translation = new Vector3(0, WaterLevel - WaterOffset, 0);
        AddChild(Water);

        int camX = int.Parse(Mathf.Stepify(Camera.Transform.origin.x / ChunkSize.x, 1).ToString());
        int camZ = int.Parse(Mathf.Stepify(Camera.Transform.origin.z / ChunkSize.z, 1).ToString());
        for (int x = -RenderDistance + camX / 2; x < RenderDistance + camX / 2; x++)
            for (int z = -RenderDistance + camZ / 2; z < RenderDistance + camZ / 2; z++)
                if (!PreloadedChunks.ContainsKey(new Vector2(x, z)) && !LoadedChunks.ContainsKey(new Vector2(x, z)) && !toRenderPos.Contains(new Vector2(x, z)))
                {
                    toRenderPos.Add(new Vector2(x, z));
                }



                    Threads[0] = new System.Threading.Thread(new System.Threading.ThreadStart(EndPreload));
        Threads[1] = new System.Threading.Thread(new System.Threading.ThreadStart(EndPreload));
        Threads[2] = new System.Threading.Thread(new System.Threading.ThreadStart(EndPreload));
        Threads[3] = new System.Threading.Thread(new System.Threading.ThreadStart(EndPreload));
        Threads[0].Start();
        Threads[1].Start();
        Threads[3].Start();
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
            if (toRenderPos.Count <= 0)
                continue;

// var toRenderPos2 = toRenderPos.OrderBy(s => ((s * new Vector2(ChunkSize.x, ChunkSize.z)) - new Vector2(Camera.GlobalTransform.origin.x, Camera.GlobalTransform.origin.z)).Length()).ToList();
            foreach (var c in toRenderPos)
            {
                Chunk c1 = new Chunk(c, Noise);
                c1.Offset = c;


                if (!PreloadedChunks.ContainsKey(c1.Offset))
                {
                    c1.Voxels = GetChunkData(c1.Offset);
                    c1.Generate();
                    try
                    {
                        PreloadedChunks.Add(c1.Offset, c1);
                        toRenderPos.Remove(c);
                    }
                    catch { }

                }
            }
        } 
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
                toRenderPos.Add(new Vector2(x + camX, y + camZ));

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


    public override void _Process(float delta)
    {
        int camX = int.Parse(Mathf.Stepify(Camera.Transform.origin.x / ChunkSize.x, 1).ToString());
        int camZ = int.Parse(Mathf.Stepify(Camera.Transform.origin.z / ChunkSize.z, 1).ToString());
        for (int x = -RenderDistance + camX / 2; x < RenderDistance + camX / 2; x++)
            for (int z = -RenderDistance + camZ / 2; z < RenderDistance + camZ / 2; z++)
                if (!PreloadedChunks.ContainsKey(new Vector2(x, z)) && !LoadedChunks.ContainsKey(new Vector2(x, z)) && !toRenderPos.Contains(new Vector2(x, z)))
                {
                    toRenderPos.Add(new Vector2(x, z));
                }

        if (PreloadedChunks.Count <= 0)
            return;

        var cl = PreloadedChunks.OrderBy(s => ((s.Key * new Vector2(ChunkSize.x, ChunkSize.z)) - new Vector2(Camera.GlobalTransform.origin.x, Camera.GlobalTransform.origin.z)).Length()).ToList();
        Render(cl.First().Value);
        PreloadedChunks.Remove(cl.First().Key);
            
            
        

        //!Camera.IsPositionBehind(new Vector3(c.Key.x * ChunkSize.x, Camera.GlobalTransform.origin.y, c.Key.y * ChunkSize.z)) 

        

       
    }

    

    private void Render(Chunk pChunk)
    {
        if (LoadedChunks.ContainsKey(pChunk.Offset))
            return;
        SurfaceTool = new SurfaceTool();
        SurfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        SurfaceTool.SetMaterial(VoxMaterial);
       
        foreach (Voxel vox in pChunk.Voxels.Values)
            CreateVoxel(SurfaceTool, vox, pChunk);

        SurfaceTool.Index();

        MeshInstance chunk = new MeshInstance
        {
            Mesh = SurfaceTool.Commit(),
            Name = pChunk.Offset.ToString(),
            Translation = new Vector3(pChunk.Offset.x * pChunk.ChunkSize.x , 0, pChunk.Offset.y * pChunk.ChunkSize.z )
        };

        // Collisions generation.
        
        chunk.CreateTrimeshCollision();
        chunk.AddToGroup("Chunk");
        // Pop animation

        AddChild(chunk);
        //Tween t = new Tween();
        //AddChild(t);
        // t.InterpolateProperty(chunk, "scale", new Vector3(0, 0, 0), new Vector3(1, 1, 1), 1f, Tween.TransitionType.Expo, Tween.EaseType.Out);
        //t.InterpolateProperty(chunk, "translation", chunk.Translation - new Vector3(0, 255 * 2, 0), chunk.Translation, 1f, Tween.TransitionType.Expo, Tween.EaseType.Out);
        //chunk.Scale = new Vector3(0, 0, 0);
        //t.Start();
        
        LoadedChunks.Add(pChunk.Offset, pChunk);

        SurfaceTool.Clear();
    }

    // Create each faces of a single voxel at pPositon in cPosition chunk.
    // TODO: Remove Surfacetool parameter.
    private static void CreateVoxel(SurfaceTool pSurfaceTool, Voxel pVoxel, Chunk pChunk)
    {
        Vector3 pPosition = pVoxel.Position;
        
        // Colors
        switch (pVoxel.type)
        {
            case BlockType.grass: // Grass
                pSurfaceTool.AddColor(new Color("6aff26").Darkened( pVoxel.Position.y / 256 / 50));
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

        bool left = !pChunk.Voxels.ContainsKey(pPosition + new Vector3(-1, 0, 0));
        bool right = !pChunk.Voxels.ContainsKey(pPosition + new Vector3(1, 0, 0));
        bool front = !pChunk.Voxels.ContainsKey(pPosition + new Vector3(0, 0, 1));
        bool back = !pChunk.Voxels.ContainsKey(pPosition + new Vector3(0, 0, -1));
        bool top = !pChunk.Voxels.ContainsKey(pPosition + new Vector3(0, 1, 0));
        bool bottom = !pChunk.Voxels.ContainsKey(pPosition + new Vector3(0, -1, 0));

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


    public Dictionary<Vector3, Voxel> GetChunkData(Vector2 chunkPosition)
    {
        var Voxels = new Dictionary<Vector3, Voxel>();
        var Offset = chunkPosition;

        for (int z = 0; z < ChunkSize.z; z += 1)
            for (int x = 0; x < ChunkSize.x; x += 1)
            {
                // Global position in the noise.
                Vector2 globalPosition = new Vector2((Offset.x * ChunkSize.x) + x, (Offset.y * ChunkSize.z) + z);
                float height = Mathf.Stepify(((Noise.GetNoise2dv(globalPosition) + 0.5f) * (ChunkSize.y / 2) ), 1);

                // Make new voxel .
                Voxel voxel = new Voxel()
                {
                    Position = new Vector3(x, height, z),
                    type = 0
                };

                if (height < WaterLevel)
                    voxel.type = BlockType.sand;
                for (int i = int.Parse(height.ToString()) - 5; i < height; i++)
                {
                    voxel.Position.y = i;
                    Voxels.Add(voxel.Position, voxel);
                }

                GenerateMountains(chunkPosition, x, z, height, Voxels);
            }
        return Voxels;
    }

    public void GenerateMountains(Vector2 Offset, float x, float z, float height, Dictionary<Vector3, Voxel> Voxels)
    {
        var voxel = new Voxel();
        for (int y = int.Parse(height.ToString()) - 1; y < int.Parse(ChunkSize.y.ToString()); y++)
        {
            Vector3 positionAir = new Vector3(x, y, z);

            if (Voxels.ContainsKey(positionAir)) // Skip ground 0 vox place earlier.
                continue;

            Vector3 OffsetGlobal = new Vector3(Offset.x * ChunkSize.x, 0, Offset.y * ChunkSize.z); // Global position in noise.
            float density = Noise2.GetNoise3dv(positionAir + OffsetGlobal); // Density in the noise.
            float DensityModifier = ((y / 3.33f / ChunkSize.y) * 2);
            if (density - DensityModifier >= 0 )
            {
                height += 1;
                voxel.Position = new Vector3(x, height, z);
                voxel.type = BlockType.rock;

                if (y < WaterLevel)
                    voxel.type = BlockType.sand;

                if (!Voxels.ContainsKey(voxel.Position))
                    Voxels.Add(voxel.Position, voxel);
            }
        }
    }
}
