using MoonWorks.Graphics;
using MoonWorks.Input;
using MoonWorks.Math;
using MoonWorks.Math.Float;
using Riateu;
using Riateu.ECS;
using Riateu.ECS.Components;
using Riateu.Graphics;

namespace Pong.ECS;

public class SimpleWorld : Screen
{
    private Batch batch;
    private Camera camera;
    private int scoreLeft;
    private int scoreRight;
    private string scoreText = "0     0";
    private CollisionSystem collisionSystem;
    private RendererSystem rendererSystem;
    private PaddleSystem paddleSystem;
    private BallSystem ballSystem;

    public ComponentQuery PaddleUpdate;
    public ComponentQuery BallUpdate;


    public SimpleWorld(GameApp gameApp) : base(gameApp)
    {
        collisionSystem = new CollisionSystem(World);
        rendererSystem = new RendererSystem(World);
        paddleSystem = new PaddleSystem(World, collisionSystem);
        ballSystem = new BallSystem(World);

        batch = new Batch(gameApp.GraphicsDevice, 512, 320);

        camera = new Camera(PingPongGame.ViewportWidth, PingPongGame.ViewportHeight);
    }


    public override void Begin()
    {
        Quad paddleQuad = Resource.Atlas["pong/paddle"];

        EntityID paddleLeft = World.CreateEntity();
        World.AddComponent(paddleLeft, new Paddle(KeyCode.W, KeyCode.S, true));
        World.AddComponent(paddleLeft, new SpriteRenderer(paddleQuad));
        World.AddComponent(paddleLeft, new Vector2(0, (PingPongGame.ViewportHeight * 0.5f) - 12));
        World.AddComponent(paddleLeft, new Hitbox(new Rectangle(0, 0, 4, 24)));

        paddleQuad.FlipUV(FlipMode.Horizontal);
        EntityID paddleRight = World.CreateEntity();
        World.AddComponent(paddleRight, new Paddle(KeyCode.Up, KeyCode.Down, true));
        World.AddComponent(paddleRight, new SpriteRenderer(paddleQuad));
        World.AddComponent(paddleRight, new Vector2(PingPongGame.ViewportWidth - 4, (PingPongGame.ViewportHeight * 0.5f) - 12));
        World.AddComponent(paddleRight, new Hitbox(new Rectangle(0, 0, 4, 24)));

        EntityID ball = World.CreateEntity();
        World.AddComponent(ball, new Ball(new Vector2(-1, 0), 100.0f));
        World.AddComponent(ball, new SpriteRenderer(Resource.Atlas["pong/ball/0"]));
        World.AddComponent(ball, new Vector2((PingPongGame.ViewportWidth * 0.5f) - 8, (PingPongGame.ViewportHeight * 0.5f) - 4));
        World.AddComponent(ball, new Hitbox(new Rectangle(0, 0, 8, 8)));

        collisionSystem.OnEntitiesAdded();
    }

    public override void End()
    {
    }

    public override void Update(double delta) 
    {
        collisionSystem.Update(delta);
        paddleSystem.Update(delta);
        ballSystem.Update(delta);

        if (!World.IsEmpty<ScoreMessage>()) 
        {
            ScoreMessage message = World.Receive<ScoreMessage>();
            if (message.left) 
            {
                scoreLeft++;
            }
            else 
            {
                scoreRight++;
            }
            scoreText = $"{scoreLeft}     {scoreRight}";
        }

        base.Update(delta);
    }


    public override void Render(CommandBuffer buffer, Texture backbuffer)
    {
        batch.Begin(Resource.AtlasTexture, DrawSampler.PointClamp);
        rendererSystem.Draw(buffer, batch);
        batch.Draw(Resource.PressStart2PFont, scoreText, new Vector2(PingPongGame.ViewportWidth * 0.5f, 0), Color.White, new Vector2(0.2f), Alignment.Center);
        batch.End(buffer);

        RenderPass renderPass = buffer.BeginRenderPass(new ColorAttachmentInfo(backbuffer, true, Color.Black));
        batch.BindUniformMatrix(buffer, camera, 0);
        batch.Render(renderPass);
        buffer.EndRenderPass(renderPass);
    }
}