using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
using Riateu;
using Riateu.Graphics;

public class PingPongGame : GameApp
{
    public const int ViewportWidth = 256;
    public const int ViewportHeight = 160;

    public PingPongGame(WindowCreateInfo windowCreateInfo, FrameLimiterSettings frameLimiterSettings, int targetTimestep = 60, bool debugMode = false) 
        : base(windowCreateInfo, frameLimiterSettings, targetTimestep, debugMode)
    {
    }

    public PingPongGame(string title, uint width, uint height, ScreenMode screenMode = ScreenMode.Windowed, bool debugMode = false) 
        : base(title, width, height, screenMode, debugMode)
    {
    }

    public override void LoadContent()
    {
        CommandBuffer cmdBuf = GraphicsDevice.AcquireCommandBuffer();
        Resource.ArcherFont = Font.Load(GraphicsDevice, cmdBuf, "Assets/font/PressStart2P-Regular.ttf");
        Resource.AtlasTexture = Texture.FromImageFile(GraphicsDevice, cmdBuf, "Assets/atlas.qoi");

        Resource.Atlas = Atlas.LoadFromFile("Assets/atlas.bin", Resource.AtlasTexture, Atlas.FileType.Bin, true);


        GraphicsDevice.Submit(cmdBuf);
    }

    public override void Initialize()
    {
        Scene = new SimpleScene(this);
    }
}
