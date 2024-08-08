using Pong;
using Riateu;
using Riateu.Audios;
using Riateu.Content;

public class PingPongGame : GameApp
{
    public const int ViewportWidth = 256;
    public const int ViewportHeight = 160;

    public PingPongGame(WindowSettings settings, GraphicsSettings graphicsSettings) : base(settings, graphicsSettings)
    {
    }

    public override void LoadContent(AssetStorage storage)
    {
        Resource.PressStart2PFont = storage.LoadFont("Assets/font/PressStart2P-Regular.ttf", 32);
        Resource.Atlas = storage.CreateAtlas("Assets/images");
        Resource.Animations = storage.LoadAnimations("Assets/images/animation.json", Resource.Atlas);
        Resource.HitSound = storage.LoadAudioTrack("Assets/sfx/hit.ogg", AudioFormat.OGG);
        Resource.BounceSound = storage.LoadAudioTrack("Assets/sfx/bounce.ogg", AudioFormat.OGG);
    }

    public override GameLoop Initialize()
    {
        return new SimpleScene(this);
    }
}