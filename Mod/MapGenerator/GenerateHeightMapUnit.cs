using System.Threading.Tasks;
using Godot;
using Nocturne.Core.Class;

public class GenerateHeightMapUnit : Unit<int, Image>
{
    public override Image Execute(int plateSize)
    {
        Image informationMaps = Image.CreateEmpty(plateSize, plateSize, false, Image.Format.Rgba8);
        NocturneNoise noise = new NocturneNoise();
        int range = informationMaps.GetWidth();

        Parallel.For(0, range, x =>{
        Parallel.For(0, range, y =>
        {
            Vector2I pixel = new Vector2I(x, y);

            float noiseValue = noise.GetNoise2D(x, y);

            float height = (int)((noiseValue + 1f) * 2f); // noise[-1,1] -> [0.00, 4.00]

            Color originalColor = informationMaps.GetPixelv(pixel);
            float R = height / 10f;
            float G = originalColor.G;
            float B = originalColor.B;
            Color color = new Color(R, G, B);

            informationMaps.SetPixelv(pixel, color);
        });});

        return informationMaps;
    }
}