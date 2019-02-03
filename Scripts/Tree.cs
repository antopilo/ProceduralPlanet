using Godot;
using System.Collections.Generic;

public class Tree : Object
{
    public int Height;
    public Dictionary<Vector3, Voxel> Voxels = new Dictionary<Vector3, Voxel>();

    private RandomNumberGenerator rng = new RandomNumberGenerator();


    public Tree()
    {
        // Randomizing the seed.
        rng.Randomize();

        
        // Getting the height of the trunk.
        Height = rng.RandiRange(4, 8);
        //Width = rng.RandiRange(1, 3); <- TODO : Add thickness to trees.
        MakeTrunk();
        PlaceLeaves();
    }
    
    private void MakeTrunk()
    {
        // Loop up the trunk.
        for (int i = 0; i < Height; i++)
            // Check if a vox is there. Not needed but for security.
            if (!Voxels.ContainsKey(new Vector3(0, i, 0)))
            {
                // Make voxel.
                Voxel voxel = new Voxel()
                {
                    Position = new Vector3(0, i, 0),
                    type = BlockType.wood
                };

                // Add to the dictionary.
                Voxels.Add(new Vector3(0, i, 0), voxel);
            }
    }

    public void PlaceLeaves()
    {
        Voxel voxel = new Voxel()
        {
            Position = new Vector3(0, Height + 1, 0),
            type = BlockType.leaves
        };
        // Top and sides.
        Voxels.Add(voxel.Position, voxel);
        voxel.Position = new Vector3(-1, Height + 1, 0); // L
        Voxels.Add(voxel.Position, voxel);
        voxel.Position = new Vector3(1, Height + 1, 0); // R
        Voxels.Add(voxel.Position, voxel);
        voxel.Position = new Vector3(0, Height + 1, 1); // F
        Voxels.Add(voxel.Position, voxel);
        voxel.Position = new Vector3(0, Height + 1, -1); //B
        Voxels.Add(voxel.Position, voxel);
        // Corners
        voxel.Position = new Vector3(-1, Height + 1, -1); //LB
        Voxels.Add(voxel.Position, voxel);
        voxel.Position = new Vector3(1, Height + 1, 1); // RF
        Voxels.Add(voxel.Position, voxel);
        voxel.Position = new Vector3(-1, Height + 1, 1); // LF
        Voxels.Add(voxel.Position, voxel);
        voxel.Position = new Vector3(1, Height + 1, -1); // RB
        Voxels.Add(voxel.Position, voxel);

        // height - 1
        voxel.Position = new Vector3(-1, Height, 0); // L
        Voxels.Add(voxel.Position, voxel);
        voxel.Position = new Vector3(1, Height, 0); // R
        Voxels.Add(voxel.Position, voxel);
        voxel.Position = new Vector3(0, Height, 1); // F
        Voxels.Add(voxel.Position, voxel);
        voxel.Position = new Vector3(0, Height, -1); //B
        Voxels.Add(voxel.Position, voxel);
        // Corners
        voxel.Position = new Vector3(-1, Height, -1); //LB
        Voxels.Add(voxel.Position, voxel);
        voxel.Position = new Vector3(1, Height, 1); // RF
        Voxels.Add(voxel.Position, voxel);
        voxel.Position = new Vector3(-1, Height, 1); // LF
        Voxels.Add(voxel.Position, voxel);
        voxel.Position = new Vector3(1, Height, -1); // RB
        Voxels.Add(voxel.Position, voxel);


    }
}

