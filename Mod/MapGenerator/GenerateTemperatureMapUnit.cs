using System.Threading.Tasks;
using Godot;
using Nocturne.Core.Class;

public class GenerateTemperatureMapUnit : Unit<Image, Image>
{
    public override Image Execute(Image informationMaps)
    {
        NocturneNoise noise = new NocturneNoise();
        int range = informationMaps.GetWidth();

        Parallel.For(0, range, x => {
        Parallel.For(0, range, y =>
        {
            Vector2I pixel = new Vector2I(x, y);

            float noiseValue = noise.GetNoise2D(x, y);
            float height = informationMaps.GetPixelv(pixel).R * 10f;
            float humidity = informationMaps.GetPixelv(pixel).G * 100f;

            float baseTemperature = -20f;
            float fluctuationLatitude = 80f / (range - 1f) * y; // I hope base temperature is frome -20 to 60.
            float fluctuationHeight = 5f * height; // [0, 4] -> [0, 20]
            float fluctuationHumidity = humidity; // [0, 100] -> [0, 100]
            float temperature = baseTemperature + fluctuationLatitude - (fluctuationHeight + fluctuationHumidity) * ((noiseValue + 1f) / 2f); // [-140, 160], noise[-1, 1] -> [0, 1]

            Color originalColor = informationMaps.GetPixelv(pixel);
            float R = originalColor.R;
            float G = originalColor.G;
            float B = (temperature + 140f) / 1000f;
            Color color = new Color(R, G, B);

            informationMaps.SetPixelv(pixel, color);
            GD.Print(informationMaps.GetPixelv(pixel));
        });});

        return informationMaps;
    }
}