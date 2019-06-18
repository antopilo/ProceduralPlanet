using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProceduralPlanet.Scripts.Biomes
{
    class Savanna 
    {
        public static float TargetTemperature { get; } = 30f;
        public static float TargetHumidity { get; } = 50f;

        // Terrain
        public static BlockType DefaultBlocktype = BlockType.sand;
        public static BlockType UnderLayerType { get; set; } = BlockType.sand;
        public static float TerrainAmplitude { get; set; } = 0f;
        public static BlockType TopLayerType { get; set; } = BlockType.sand;
        public static int TopLayerThickness { get; set; } = 3;
        public static bool Mountains { get; set; } = false;

        // Vegetation
        public static string TreeModel { get; set; } = "res://models/trees/pine1.tres";
        public static int TreeRate { get; set; } = 1;
        public static string DecorationModel { get; set; } = "res://models/decorations/grass.tres";
        public static int DecorationRate { get; set; } = 0;
    }
}
