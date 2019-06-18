


/*

     100%   +-------------------------------------------+
            |          |          |         |           |
            |          |          |         |           |
            |          |          |         |           |
    H  75%  |          |          +---------+           |
    U       |          |          |         |           |
    M       |          |          |         +-----------+
    I       |          |          |         |           |
    D  50%  |          +--------------------+           |
    I       |          |                    |           |
    T       |          |                    +-----------+
    Y       |          |                    |           |
   (%) 25%  +----------+--------------------+           |
            |          |                    |           |
            |          |                    |           |
            |          |                    |           |
        0%  +-------------------------------------------+
           -10c        0c         10c       20c         30c
                           Temperature(celcius)

 */

using Godot;
using ProceduralPlanet.Scripts.Biomes;
using System;
using System.Collections.Generic;
using System.Linq;

public class BiomeManager
{
    public static float BiomeAcceptanceThreshold = 0.7f;

    public static Biomes GetBiome(float temperature, float humidity)
    {
        bool isTundra = temperature >= -10 && temperature < 0;
        bool isBorealForest = temperature >= 0 && temperature < 10 && humidity >= 50 && humidity < 100;
        bool isWoodlands = temperature >= 0 && temperature < 20 && humidity >= 25 && humidity < 50;
        bool isColdDesert = temperature >= 0 && temperature < 20 && humidity >= 0 && humidity < 25;
        bool isDesert = temperature >= -10 && temperature < 0 && humidity >= 0 && humidity < 25;
        bool isSavanna = temperature >= 20 && temperature < 30 && humidity >= 25 && humidity < 75;
        bool isRainForest = temperature >= 10 && temperature < 20 && humidity >= 75 && humidity < 100;
        bool isTropicRainForest = temperature >= 20 && temperature < 30 && humidity >= 75 && humidity < 100;
        bool isForest = temperature >= 10 && temperature < 20 && humidity >= 50 && humidity < 75;

        if (isTundra)
            return Biomes.Tundra;
        if (isBorealForest)
            return Biomes.BorealForest;
        else if (isWoodlands)
            return Biomes.Woodlands;
        else if (isColdDesert)
            return Biomes.ColdDesert;
        else if (isDesert)
            return Biomes.Desert;
        else if (isSavanna)
            return Biomes.Savanna;
        else if (isRainForest)
            return Biomes.RainForest;
        else if (isTropicRainForest)
            return Biomes.TropicRainForest;
        else
            return Biomes.Forest; // Default 
    }



    public static BiomeSettings GetBiomeSettings(Biomes biome)
    {
        var newBiomeSettings = new BiomeSettings();
        switch (biome)
        {
            case Biomes.Tundra:
                newBiomeSettings.TargetHumidity = Tundra.TargetHumidity;
                newBiomeSettings.TargetTemperature = Tundra.TargetTemperature;
                newBiomeSettings.DefaultBlocktype = Tundra.DefaultBlocktype;
                newBiomeSettings.TopLayerType = Tundra.TopLayerType;
                newBiomeSettings.UnderLayerType = Tundra.UnderLayerType;
                newBiomeSettings.Mountains = Tundra.Mountains;
                newBiomeSettings.TerrainAmplitude = Tundra.TerrainAmplitude;
                newBiomeSettings.TopLayerThickness = Tundra.TopLayerThickness;
                newBiomeSettings.TreeModel = Tundra.TreeModel;
                newBiomeSettings.TreeRate = Tundra.TreeRate;
                newBiomeSettings.DecorationModel = Tundra.DecorationModel;
                newBiomeSettings.DecorationRate = Tundra.DecorationRate;
                break;
            case Biomes.BorealForest:
                newBiomeSettings.TargetHumidity = BorealForest.TargetHumidity;
                newBiomeSettings.TargetTemperature = BorealForest.TargetTemperature;
                newBiomeSettings.DefaultBlocktype = BorealForest.DefaultBlocktype;
                newBiomeSettings.TopLayerType = BorealForest.TopLayerType;
                newBiomeSettings.UnderLayerType = BorealForest.UnderLayerType;
                newBiomeSettings.Mountains = BorealForest.Mountains;
                newBiomeSettings.TerrainAmplitude = BorealForest.TerrainAmplitude;
                newBiomeSettings.TopLayerThickness = BorealForest.TopLayerThickness;
                newBiomeSettings.TreeModel = BorealForest.TreeModel;
                newBiomeSettings.TreeRate = BorealForest.TreeRate;
                newBiomeSettings.DecorationModel = BorealForest.DecorationModel;
                newBiomeSettings.DecorationRate = BorealForest.DecorationRate;
                break;
            case Biomes.Woodlands:
                newBiomeSettings.TargetHumidity = Woodlands.TargetHumidity;
                newBiomeSettings.TargetTemperature = Woodlands.TargetTemperature;
                newBiomeSettings.DefaultBlocktype = Woodlands.DefaultBlocktype;
                newBiomeSettings.TopLayerType = Woodlands.TopLayerType;
                newBiomeSettings.UnderLayerType = Woodlands.UnderLayerType;
                newBiomeSettings.Mountains = Woodlands.Mountains;
                newBiomeSettings.TerrainAmplitude = Woodlands.TerrainAmplitude;
                newBiomeSettings.TopLayerThickness = Woodlands.TopLayerThickness;
                newBiomeSettings.TreeModel = Woodlands.TreeModel;
                newBiomeSettings.TreeRate = Woodlands.TreeRate;
                newBiomeSettings.DecorationModel = Woodlands.DecorationModel;
                newBiomeSettings.DecorationRate = Woodlands.DecorationRate;
                break;
            case Biomes.ColdDesert:
                newBiomeSettings.TargetHumidity = ColdDesert.TargetHumidity;
                newBiomeSettings.TargetTemperature = ColdDesert.TargetTemperature;
                newBiomeSettings.DefaultBlocktype = ColdDesert.DefaultBlocktype;
                newBiomeSettings.TopLayerType = ColdDesert.TopLayerType;
                newBiomeSettings.UnderLayerType = ColdDesert.UnderLayerType;
                newBiomeSettings.Mountains = ColdDesert.Mountains;
                newBiomeSettings.TerrainAmplitude = ColdDesert.TerrainAmplitude;
                newBiomeSettings.TopLayerThickness = ColdDesert.TopLayerThickness;
                newBiomeSettings.TreeModel = ColdDesert.TreeModel;
                newBiomeSettings.TreeRate = ColdDesert.TreeRate;
                newBiomeSettings.DecorationModel = ColdDesert.DecorationModel;
                newBiomeSettings.DecorationRate = ColdDesert.DecorationRate;
                break;
            case Biomes.Desert:
                newBiomeSettings.TargetHumidity = Desert.TargetHumidity;
                newBiomeSettings.TargetTemperature = Desert.TargetTemperature;
                newBiomeSettings.DefaultBlocktype = Desert.DefaultBlocktype;
                newBiomeSettings.TopLayerType = Desert.TopLayerType;
                newBiomeSettings.UnderLayerType = Desert.UnderLayerType;
                newBiomeSettings.Mountains = Desert.Mountains;
                newBiomeSettings.TerrainAmplitude = Desert.TerrainAmplitude;
                newBiomeSettings.TopLayerThickness = Desert.TopLayerThickness;
                newBiomeSettings.TreeModel = Desert.TreeModel;
                newBiomeSettings.TreeRate = Desert.TreeRate;
                newBiomeSettings.DecorationModel = Desert.DecorationModel;
                newBiomeSettings.DecorationRate = Desert.DecorationRate;
                break;
            case Biomes.Savanna:
                newBiomeSettings.TargetHumidity = Savanna.TargetHumidity;
                newBiomeSettings.TargetTemperature = Savanna.TargetTemperature;
                newBiomeSettings.DefaultBlocktype = Savanna.DefaultBlocktype;
                newBiomeSettings.TopLayerType = Savanna.TopLayerType;
                newBiomeSettings.UnderLayerType = Savanna.UnderLayerType;
                newBiomeSettings.Mountains = Savanna.Mountains;
                newBiomeSettings.TerrainAmplitude = Savanna.TerrainAmplitude;
                newBiomeSettings.TopLayerThickness = Savanna.TopLayerThickness;
                newBiomeSettings.TreeModel = Savanna.TreeModel;
                newBiomeSettings.TreeRate = Savanna.TreeRate;
                newBiomeSettings.DecorationModel = Savanna.DecorationModel;
                newBiomeSettings.DecorationRate = Savanna.DecorationRate;
                break;
            case Biomes.RainForest:
                newBiomeSettings.TargetHumidity = RainForest.TargetHumidity;
                newBiomeSettings.TargetTemperature = RainForest.TargetTemperature;
                newBiomeSettings.DefaultBlocktype = RainForest.DefaultBlocktype;
                newBiomeSettings.TopLayerType = RainForest.TopLayerType;
                newBiomeSettings.UnderLayerType = RainForest.UnderLayerType;
                newBiomeSettings.Mountains = RainForest.Mountains;
                newBiomeSettings.TerrainAmplitude = RainForest.TerrainAmplitude;
                newBiomeSettings.TopLayerThickness = RainForest.TopLayerThickness;
                newBiomeSettings.TreeModel = RainForest.TreeModel;
                newBiomeSettings.TreeRate = RainForest.TreeRate;
                newBiomeSettings.DecorationModel = RainForest.DecorationModel;
                newBiomeSettings.DecorationRate = RainForest.DecorationRate;
                break;
            case Biomes.TropicRainForest:
                newBiomeSettings.TargetHumidity = TropicRainForest.TargetHumidity;
                newBiomeSettings.TargetTemperature = TropicRainForest.TargetTemperature;
                newBiomeSettings.DefaultBlocktype = TropicRainForest.DefaultBlocktype;
                newBiomeSettings.TopLayerType = TropicRainForest.TopLayerType;
                newBiomeSettings.UnderLayerType = TropicRainForest.UnderLayerType;
                newBiomeSettings.Mountains = TropicRainForest.Mountains;
                newBiomeSettings.TerrainAmplitude = TropicRainForest.TerrainAmplitude;
                newBiomeSettings.TopLayerThickness = TropicRainForest.TopLayerThickness;
                newBiomeSettings.TreeModel = TropicRainForest.TreeModel;
                newBiomeSettings.TreeRate = TropicRainForest.TreeRate;
                newBiomeSettings.DecorationModel = TropicRainForest.DecorationModel;
                newBiomeSettings.DecorationRate = TropicRainForest.DecorationRate;
                break;
            case Biomes.Forest:
                newBiomeSettings.TargetHumidity = Forest.TargetHumidity;
                newBiomeSettings.TargetTemperature = Forest.TargetTemperature;
                newBiomeSettings.DefaultBlocktype = Forest.DefaultBlocktype;
                newBiomeSettings.TopLayerType = Forest.TopLayerType;
                newBiomeSettings.UnderLayerType = Forest.UnderLayerType;
                newBiomeSettings.Mountains = Forest.Mountains;
                newBiomeSettings.TerrainAmplitude = Forest.TerrainAmplitude;
                newBiomeSettings.TopLayerThickness = Forest.TopLayerThickness;
                newBiomeSettings.TreeModel = Forest.TreeModel;
                newBiomeSettings.TreeRate = Forest.TreeRate;
                newBiomeSettings.DecorationModel = Forest.DecorationModel;
                newBiomeSettings.DecorationRate = Forest.DecorationRate;
                break;
        }

        return newBiomeSettings;

    }

    public static Biomes FindMatchingBiome(float temp, float humidity)
    {
        string message = "Current temp: " + temp + " hum: " + humidity;
        float[] results = new float[Enum.GetValues(typeof(Biomes)).Length];
        for (int i = 0; i < results.Length; i++)
        {
            results[i] = GetMatch(temp, humidity, GetBiomeSettings((Biomes)i));
            message += ((Biomes)i).ToString() + ": " + results[i] + "% \n";
        }


        // Pick top 2
        int bestMatch = 0, secondMatch = 0;
        float bestValue = 0f, secondValue = 0f;
        for (int i = 0; i < results.Length; i++)
        {
            if (results[i] > bestValue)
            {
                secondMatch = bestMatch;
                secondValue = results[bestMatch];

                bestMatch = i;
                bestValue = results[i];
            }

            if (results[i] > secondValue && results[i] != bestValue)
            {
                secondMatch = i;
                secondValue = results[secondMatch];
            }
        }

        return (Biomes)bestMatch;
    }


    public static BiomeSettings BestMatch(float temp, float humidity)
    {
        // Calculate match for each biome.
        string message = "Current temp: " + temp + " hum: " + humidity;
        float[] results = new float[Enum.GetValues(typeof(Biomes)).Length];
        for (int i = 0; i < results.Length; i++)
        {
            results[i] = GetMatch(temp, humidity, GetBiomeSettings((Biomes)i));
            message += ((Biomes)i).ToString() + ": " + results[i] + "% \n";
        }
 
        
        // Pick top 2
        int bestMatch = 0, secondMatch = 0;
        float bestValue = 0f, secondValue = 0f;
        for (int i = 0; i < results.Length; i++)
        {
            if (results[i] > bestValue)
            {
                secondMatch = bestMatch;
                secondValue = results[bestMatch];

                bestMatch = i;
                bestValue = results[i];
            }

            if (results[i] > secondValue && results[i] != bestValue)
            {
                secondMatch = i;
                secondValue = results[secondMatch];
            }
        }


        //GD.Print("Best is " + (Biomes)bestMatch + " with: " + bestValue);
        //GD.Print("2nd is " + (Biomes)secondMatch + " with: " + secondValue);
        var BestSetting = GetBiomeSettings((Biomes)bestMatch);
        var SecondSetting = GetBiomeSettings((Biomes)secondMatch);


        float strengh = 1 - (BiomeAcceptanceThreshold - bestValue);
        return InterpolateBiome(BestSetting, strengh, SecondSetting, 1 - strengh);



        //if (bestValue > BiomeAcceptanceThreshold)
        //{
        //    return BestSetting;
        //}
        //else
        //{
        //    return InterpolateBiome(BestSetting, bestValue, SecondSetting, secondValue);
        //}

    }

    private static BiomeSettings InterpolateBiome(BiomeSettings BestSetting, float best, BiomeSettings SecondSetting, float second)
    {
        var newSetting = new BiomeSettings();

        newSetting.TerrainAmplitude = BestSetting.TerrainAmplitude * best + SecondSetting.TerrainAmplitude * second;
        newSetting.TopLayerThickness = (int)(BestSetting.TopLayerThickness * best + SecondSetting.TopLayerThickness * second);
        newSetting.TreeRate = BestSetting.TreeRate * best + SecondSetting.TreeRate * second;

        // TODO: Make decoration rate a float.
        newSetting.DecorationRate = (int)(BestSetting.DecorationRate * best + SecondSetting.DecorationRate * second);

        if(best < second )
        {
            newSetting.DefaultBlocktype = BestSetting.DefaultBlocktype;
            newSetting.TopLayerType = BestSetting.TopLayerType;
            newSetting.UnderLayerType = BestSetting.UnderLayerType;

            newSetting.TreeModel = BestSetting.TreeModel;
            newSetting.DecorationModel = BestSetting.DecorationModel;
        }
        else
        {
            newSetting.DefaultBlocktype = SecondSetting.DefaultBlocktype;
            newSetting.TopLayerType = SecondSetting.TopLayerType;
            newSetting.UnderLayerType = SecondSetting.UnderLayerType;

            newSetting.TreeModel = SecondSetting.TreeModel;
            newSetting.DecorationModel = SecondSetting.DecorationModel;
        }

        return newSetting;
    }

    private static float GetMatch(float temp, float humidity, BiomeSettings biome)
    {
        float realTemp, realHum, realTargetTemp, realTargetHum;

        float targetTemp = biome.TargetTemperature;
        float targetHum = biome.TargetHumidity;

        realTemp = (TemperatureManager.GetOriginalTemperature(temp) - 1f) / 2;
        realHum = humidity / 100f;

        realTargetTemp = (TemperatureManager.GetOriginalTemperature(targetTemp) - 1f) / 2;
        realTargetHum = targetHum / 100f;

        float diffTemp = realTemp > realTargetTemp ? realTargetTemp - realTemp : realTemp - realTargetTemp;
        float diffHum = realHum > realTargetHum ? realTargetHum - realHum : realHum - realTargetHum;
        return 1f + (diffTemp + diffHum);
    }
}