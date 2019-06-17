using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProceduralPlanet.Scripts.Biomes
{
    public static class BiomeSettings
    {
        public static BlockType DefaultBlocktype { get; set; }
        public static BlockType UnderLayerType { get; set; } 
        public static float TerrainAmplitude { get; set; }
        public static BlockType TopLayerType { get; set; }
        public static int TopLayerThickness { get; set; } 
        public static bool Mountains { get; set; } = true;

        // Vegetation
        public static string TreeModel { get; set; }
        public static float TreeRate { get; set; }

        public static string DecorationModel { get; set; } 
        public static int DecorationRate { get; set; }
    }
}
