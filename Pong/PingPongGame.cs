using MoonWorks;
using Pong;
using Riateu;
using Riateu.Content;
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

    public override void LoadContent(AssetStorage storage)
    {
        Resource.AtlasTexture = storage.LoadTexture("Assets/atlas.qoi");

        Resource.Atlas = storage.LoadAtlas("Assets/atlas.bin", Resource.AtlasTexture, true, JsonType.Bin);

        Resource.PressStart2PFont = storage.LoadFont("Assets/font/PressStart2P.fnt", Resource.AtlasTexture, Resource.Atlas["fonts/PressStart2P_0"]);
        Resource.Animations = storage.LoadAnimations("Assets/images/animation.json", Resource.Atlas);
    }

    public override GameLoop Initialize()
    {
        return new SimpleScene(this);
    }
}