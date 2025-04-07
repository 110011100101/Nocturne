using Godot;
using Godot.Collections;
using Nocturne.Core.Class;

public class GenerateHeightMapUnit : Unit<int, Array<Image>>
{
    public override Array<Image> Execute(int plateSize)
    {
        Image heightMap = Image.CreateEmpty(plateSize, plateSize, false, Image.Format.Rgba8);
        Array<Image> informationMaps = new();

        FastNoiseLite noise = new FastNoiseLite()
        {
            Seed = (int)GD.Randi(),                             // 随机种子
            NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin,
            Frequency = 0.0495f,                                // 这个值越小地图越大块
            FractalOctaves = 3,                                 // 增加分形层数
            FractalLacunarity = 2.7f,                           // 增强层间差异(值越大层间边缘越破碎)
            FractalGain = 0.27f,                                // 控制振幅衰减(值越大地图越稀碎)
        };
        
        for(int x = 0; x < plateSize; x++)
        for(int y = 0; y < plateSize; y++)
        {
            float noiseValue = noise.GetNoise2D(x, y);
            int height = (int)((noiseValue + 1.0f) * 2.0f);       // [-1,1] => [0.00, 4.00]

            heightMap.SetPixelv(new Vector2I(x, y), new Color(height, height, height));
        }
        
        informationMaps.Add(heightMap);

        return informationMaps;
    }
}