using System;
using Godot;

public static class TemperatureManager
{
    public static OpenSimplexNoise TemperatureMap = new OpenSimplexNoise();
    public static OpenSimplexNoise HumidityMap = new OpenSimplexNoise();

    // Get the temperature from global coordinates.
    public static float GetTemperature(int x, int z)
    {
        // Range from -1 to 1.
        var actual = TemperatureMap.GetNoise2d(x, z);

        // Return range from -10 to 30
        return (((actual + 1f) / 2f) * 40f) - 10;
    }

    // Get the humidity from global coordinates.
    public static float GetHumidity(int x, int z)
    {
        // Range -1 to 1
        float actual = HumidityMap.GetNoise2d(x, z);

        // Real humidity range 0 to 100.
        return ((actual + 1f) / 2f) * 100f;
    }

    // Convert temperature ranging from [-10, 30] to [-1, 1]
    public static float GetOriginalTemperature(float temp)
    {
        return (((temp + 10) / 40f) * 2f) + 1f;
    }
}
    

