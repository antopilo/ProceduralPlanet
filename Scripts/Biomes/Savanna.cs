using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProceduralPlanet.Scripts.Biomes
{
    class Savanna 
    {
        public static float TargetTemperature { get; } = 25f;
        public static float TargetHumidity { get; } = 50f;

        // Terrain
        public static BlockType DefaultBlocktype = BlockType.rock;
        public static BlockType UnderLayerType { get; set; } = BlockType.sand;
        public static float TerrainAmplitude { get; set; } = 0.1f;
        public static BlockType TopLayerType { get; set; } = BlockType.sand;
        public static int TopLayerThickness { get; set; } = 3;
        public static bool Mountains { get; set; } = false;

        // Vegetation
        public static string TreeModel { get; set; } = "res://models/trees/Palm1.tres";
        public static float TreeRate { get; set; } = 1.05f;
        public static string DecorationModel { get; set; } = "res://models/decorations/puddles1.tres";
        public static float DecorationRate { get; set; } = 1.1f;
    }
}
