using System.Threading.Tasks;
using Godot;
using Nocturne.Core.Class;

public class GenerateHumidityMapUnit : Unit<Image, Image>
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

            float baseHumidity = (noiseValue + 1) * 50f + 5f; // noise[-1, 1] -> [0, 100]
            float fluctuation  = height * ((noiseValue + 1f) / 2f); // noise[-1, 1] -> [0, 1]
            float humidityValue = baseHumidity * fluctuation; // [0, 100] * [0, 1] -> [0, 100]

            Color originalColor = informationMaps.GetPixelv(pixel);
            float R = originalColor.R;
            float G = humidityValue / 100f;
            float B = originalColor.B;
            Color color = new Color(R, G, B);

            informationMaps.SetPixelv(pixel, color);
        });});

        return informationMaps;
    }
}