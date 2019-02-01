using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class Generator : Node
{
    [Export] int CurrentSeed = 0;
    Thread thread = new Thread();
    // Noises
    private OpenSimplexNoise Noise = new OpenSimplexNoise();

    private Camera Camera;
    SurfaceTool SurfaceTool;
    private SpatialMaterial VoxMaterial;
    private SpatialMaterial WaterMaterial;

    private Vector3 ChunkSize = new Vector3(16, 128, 16);
<<<<<<< HEAD
    private int RenderDistance = 4;
=======
    [Export] private int RenderDistance = 4;
>>>>>>> 7139ef2323dffedcedf07472c8e695d032420a99
    private float VoxelSize = 1f;

    // Chunks
    private Dictionary VoxInChunks = new Dictionary();
    private Dictionary LoadedChunks = new Dictionary();

    // Water
    private MeshInstance Water;
    private PlaneMesh WaterMesh;

    private float WaterOffset = 0.5f;
    private int WaterLevel = 60;

    float tick = 0;
    float UpdateRate = 2f;
    private Vector3[] Vertices = { new Vector3(0, 0, 0),
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
        Camera = GetNode("../Camera") as Camera;
        VoxMaterial = ResourceLoader.Load("res://material/Grass.tres") as SpatialMaterial;
        WaterMaterial = ResourceLoader.Load("res://material/water.tres") as SpatialMaterial;

        GenerateSeed();

        Water = new MeshInstance();
        WaterMesh = new PlaneMesh();
        WaterMesh.Size = new Vector2((VoxelSize * ChunkSize.x ) * RenderDistance * 4 , (VoxelSize * ChunkSize.z) * RenderDistance * 4) ;
        WaterMesh.SetMaterial(WaterMaterial);
        Water.SetMesh(WaterMesh);
        Water.Translation = new Vector3(0, WaterLevel - WaterOffset, 0);
        AddChild(Water);

        for (int i = -RenderDistance; i < RenderDistance; i++)
            for (int j = -RenderDistance; j < RenderDistance; j++)
                LoadChunk(new Chunk(new Vector2(i, j)));
                
    }
    public override void _Process(float delta)
    {
        if (tick < UpdateRate)
        {
            tick += delta;
            return;
        }

        int camX = int.Parse(Mathf.Stepify(Camera.Transform.origin.x / ChunkSize.x, 1).ToString());
        int camZ = int.Parse(Mathf.Stepify(Camera.Transform.origin.z / ChunkSize.z, 1).ToString());
        for (int x = -RenderDistance + camX; x < RenderDistance + camX; x++)
        {
            for (int z = -RenderDistance + camZ; z < RenderDistance + camZ; z++)
            {
                if (!LoadedChunks.ContainsKey(new Vector2(x, z)))
                {
                    //GD.Print("Check : " + new Vector2(x, z).ToString());
                    LoadChunk(new Chunk(new Vector2(x, z)));
                }
            }
        }
        tick = 0;

    }



    private void GenerateSeed()
    {
        if(CurrentSeed != 0)
            return;

        var rng = new RandomNumberGenerator();
        rng.Randomize();
        CurrentSeed = rng.Seed;
        
        Noise.Seed = CurrentSeed;
        Noise.Octaves = 5;
        Noise.Period = 500;
        Noise.Persistence = 0.8f;
    }

    private void InterpolatePass()
    {
<<<<<<< HEAD
        Vector3 a = new Vector3(0, 0, 0);
        
        Vector3 aa = new Vector3(1, 0, 0);

        Vector3 bb = new Vector3(0, 0, 1);

        Vector3 b = new Vector3(1, 0, 1);
=======
        for (int z = 0; z < ChunkSize.z; z++)
            for (int x = 0; x < ChunkSize.x; x++)
            {
                var height = Mathf.Stepify((Noise.GetNoise2d(cPosition.x + x, cPosition.y + z) + 1) * ChunkSize.y / 2, 1);
                VoxInChunks.Add( new Vector3(x, height, z).ToString(), new Vector3(x, height, z));

                for (int y = 0; y < ChunkSize.y; y++)
                {
                    var pos = new Vector3(x + cPosition.x, y, z + cPosition.y);
                    if (Noise.GetNoise3dv(pos) - ((y / ChunkSize.y) / 2.5f) >= 0)
                    {
                        height += 1;
                        VoxInChunks.Add(new Vector3(x, height, z).ToString(), new Vector3(x, height, z));
                    }    
                }
            }
>>>>>>> 7139ef2323dffedcedf07472c8e695d032420a99
    }

    /// <summary>
    /// Renders a chunk.
    /// </summary>
    /// <param name="cPosition">Global Position.</param>
    private void LoadChunk(Chunk pChunk)
    {
        SurfaceTool = new SurfaceTool();
        SurfaceTool.Begin(Mesh.PrimitiveType.Triangles);
        SurfaceTool.SetMaterial(VoxMaterial);
<<<<<<< HEAD

        foreach (Vector3 voxPos in pChunk.Preload(Noise).Keys)
            CreateVoxel(SurfaceTool, voxPos, pChunk);
=======
        PreloadChunk(cPosition);

        foreach (Vector3 voxPos in VoxInChunks.Values)
            CreateVoxel(SurfaceTool, voxPos, cPosition);
>>>>>>> 7139ef2323dffedcedf07472c8e695d032420a99

        var chunk = new MeshInstance();
        chunk.Mesh = SurfaceTool.Commit();
        chunk.Name = pChunk.Offset.ToString();
        chunk.Translation = new Vector3(pChunk.Offset.x * pChunk.ChunkSize.x, 0, pChunk.Offset.y * pChunk.ChunkSize.z);
        //chunk.CreateTrimeshCollision(); // Collisions generation.
        chunk.AddToGroup("Chunk");
        AddChild(chunk);

        LoadedChunks.Add(pChunk.Offset, pChunk.Offset); // Keeping track of generated chunks.

        SurfaceTool.Clear();
        VoxInChunks.Clear();
        GD.Print("Chunk Done: " + pChunk.Offset.ToString());
    }


    // Create each faces of a single voxel at pPositon in cPosition chunk.
    // TODO: Remove Surfacetool parameter.
    private void CreateVoxel(SurfaceTool pSurfaceTool, Vector3 pPosition, Chunk pChunk)
    {
<<<<<<< HEAD
        pSurfaceTool.AddColor( new Color("66CD00").Lightened(pPosition.y / 50 ));

        if(CanCreateFace(pPosition.x, pPosition.y + 1, pPosition.z, pChunk)) // Above
=======
        // Global position of the voxel.
        var VoxelPos = new Vector3(pPosition.x + cPosition.x, pPosition.y, pPosition.z + cPosition.y);
        

        if (CanCreateFace(pPosition.x, pPosition.y + 1, pPosition.z)) // Above
>>>>>>> 7139ef2323dffedcedf07472c8e695d032420a99
        {

            pSurfaceTool.AddNormal(new Vector3(0, 1, 0));
<<<<<<< HEAD
            pSurfaceTool.AddVertex(Vertices[4] + pPosition);
            pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            pSurfaceTool.AddVertex(Vertices[7] + pPosition);
            pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            pSurfaceTool.AddVertex(Vertices[7] + pPosition);
=======
            pSurfaceTool.AddVertex(Vertices[4] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[5] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[7] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[5] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[6] + VoxelPos);
            pSurfaceTool.AddVertex(Vertices[7] + VoxelPos);
            //SurfaceTool.AddColor(new Color("7CFC00"));
>>>>>>> 7139ef2323dffedcedf07472c8e695d032420a99
        }
        if (CanCreateFace(pPosition.x + 1, pPosition.y , pPosition.z, pChunk)) // Right
        {
            pSurfaceTool.AddNormal(new Vector3(1, 0, 0));
            pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            pSurfaceTool.AddVertex(Vertices[1] + pPosition);
            pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            pSurfaceTool.AddVertex(Vertices[5] + pPosition); 
        }
        if (CanCreateFace(pPosition.x - 1, pPosition.y , pPosition.z, pChunk)) // Left
        {
            pSurfaceTool.AddNormal(new Vector3(-1, 0, 0));
            pSurfaceTool.AddVertex(Vertices[0] + pPosition);
            pSurfaceTool.AddVertex(Vertices[7] + pPosition);
            pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            pSurfaceTool.AddVertex(Vertices[0] + pPosition);
            pSurfaceTool.AddVertex(Vertices[4] + pPosition);
            pSurfaceTool.AddVertex(Vertices[7] + pPosition); 

        }
        if (CanCreateFace(pPosition.x, pPosition.y, pPosition.z + 1, pChunk)) // Front
        {
            pSurfaceTool.AddNormal(new Vector3(0, 0, 1));
            pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            pSurfaceTool.AddVertex(Vertices[6] + pPosition);
            pSurfaceTool.AddVertex(Vertices[2] + pPosition);
            pSurfaceTool.AddVertex(Vertices[3] + pPosition);
            pSurfaceTool.AddVertex(Vertices[7] + pPosition);
            pSurfaceTool.AddVertex(Vertices[6] + pPosition); 
        }
        if (CanCreateFace(pPosition.x, pPosition.y, pPosition.z - 1, pChunk)) // Above
        {
            pSurfaceTool.AddNormal(new Vector3(0, 0, -1));
            pSurfaceTool.AddVertex(Vertices[0] + pPosition);
            pSurfaceTool.AddVertex(Vertices[1] + pPosition);
            pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            pSurfaceTool.AddVertex(Vertices[5] + pPosition);
            pSurfaceTool.AddVertex(Vertices[4] + pPosition);
            pSurfaceTool.AddVertex(Vertices[0] + pPosition);
        }
    }

    // Check if there is a voxel at X Y Z position in the current preloaded chunk.
    private bool CanCreateFace(float x, float y, float z, Chunk pChunk)
    {
<<<<<<< HEAD
        //GD.Print(x + "-" + y + "-" + z);
        return !(pChunk.Voxels.ContainsKey( new Vector3(x, y, z) ));//|| 
=======
        return !( VoxInChunks.ContainsKey( new Vector3(x, y, z).ToString() )  );//|| 
>>>>>>> 7139ef2323dffedcedf07472c8e695d032420a99
            //( (x % (ChunkSize.x ) == 0 || z % (ChunkSize.z) == 0 || x <= 1 || z <= 1 ) && VoxInChunks.ContainsKey(new Vector3(x, y + 1, z).ToString()) );
    }
}
