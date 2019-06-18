using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProceduralPlanet.Scripts.Biomes
{
    class Desert 
    {
        public static float TargetTemperature { get; } = 25f;
        public static float TargetHumidity { get; } = 15f;
        // Terrain
        public static BlockType DefaultBlocktype = BlockType.sand;
        public static BlockType UnderLayerType { get; set; } = BlockType.sand;
        public static float TerrainAmplitude { get; set; } = 0.1f;
        public static BlockType TopLayerType { get; set; } = BlockType.sand;
        public static int TopLayerThickness { get; set; } = 2;
        public static bool Mountains { get; set; } = false;

        // Vegetation
        public static string TreeModel { get; set; } = null;
        public static float TreeRate { get; set; } = 0;
        public static string DecorationModel { get; set; } = "res://models/decorations/puddles1.tres";
        public static float DecorationRate { get; set; } = 10;
    }
}
