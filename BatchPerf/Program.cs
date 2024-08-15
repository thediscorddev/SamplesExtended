using System;
using System.Numerics;
using Riateu;
using Riateu.Content;
using Riateu.Graphics;
using Riateu.Inputs;

public class BatchPerf : GameApp
{
    public static Texture TeuriaLogo;
    public static SpriteFont Font;

    public BatchPerf(WindowSettings settings, GraphicsSettings graphicsSettings) : base(settings, graphicsSettings)
    {
    }

    public override void LoadContent(AssetStorage storage)
    {
        TeuriaLogo = storage.LoadTexture("Assets/teuria-logo.png");
        Font = storage.LoadFont("Assets/PressStart2P-Regular.ttf", 32);
    }

    public override GameLoop Initialize()
    {
        return new TestLoop(this);
    }

    public static void Main() 
    {
        BatchPerf batchPerf = new BatchPerf(
            new WindowSettings("Batch Performance", 1024, 640),
            GraphicsSettings.Vsync
        );
        batchPerf.Run();
    }
}

public class TestLoop : GameLoop
{
    private const int MAX_TEURIA = 1_000_000;
    private const int INC_DEC_COUNT = 5_000;
    private const int INITIAL_COUNT = 10_000;
    private const int BatchSize = 32768;

    private Batch batch;
    private Teuria[] Teurias = new Teuria[MAX_TEURIA];
    private Camera camera = new Camera(512, 320);
    private double FPS;
    private int count = INITIAL_COUNT;

    public TestLoop(GameApp gameApp) : base(gameApp)
    {
        batch = new Batch(gameApp.GraphicsDevice, 1024, 640);
    }

    public override void Begin()
    {
        Random random = new Random();
        Span<Teuria> teurias = Teurias.AsSpan();
        for (int i = 0; i < INITIAL_COUNT; i++) 
        {
            teurias[i].Position = new Vector2(random.Next() % (512 - 32), random.Next() % (320 - 32));
            teurias[i].Velocity = new Vector2(random.Next() % 2 == 0 ? -1 : 1, random.Next() % 2 == 0 ? -1 : 1);
            teurias[i].Color = new Color((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), 1);
        }
    }

    public override void End()
    {
    }

    public override void Update(double delta)
    {
        if (Input.Keyboard.IsPressed(KeyCode.Up)) 
        {
            Random random = new Random();
            for (int i = 0; i < INC_DEC_COUNT; i++) 
            {
                if (count >= MAX_TEURIA)
                {
                    break;
                }

                Teurias[count].Position = new Vector2(random.Next() % (512 - 32), random.Next() % (320 - 32));
                Teurias[count].Velocity = new Vector2(random.Next() % 2 == 0 ? -1 : 1, random.Next() % 2 == 0 ? -1 : 1);
                Teurias[count].Color = new Color((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), 1);
                count++;
            }
        }
        else if (Input.Keyboard.IsPressed(KeyCode.Down)) 
        {
            count = Math.Max(0, count - INC_DEC_COUNT);
        }
        Vector2 gameSize = new Vector2(512, 320);
        Span<Teuria> teurias = Teurias.AsSpan();
        for (int i = 0; i < count; i++) 
        {
            ref Teuria teuria = ref teurias[i];
            teuria.Position += teuria.Velocity;

            if (teuria.Position.X + 32 > gameSize.X || teuria.Position.X < 0) 
            {
                teuria.Velocity.X *= -1;
            }
            if (teuria.Position.Y + 32 > gameSize.Y || teuria.Position.Y < 0) 
            {
                teuria.Velocity.Y *= -1;
            }
        }
        FPS = Time.FPS;
    }

    public override void Render(RenderTarget backbuffer)
    {
        ReadOnlySpan<Teuria> teurias = Teurias.AsSpan();
        for (int i = 0; i < count; i += BatchSize)
        {
            batch.Begin(BatchPerf.TeuriaLogo, DrawSampler.PointClamp, camera);
            var c = Math.Min(count - i, BatchSize);

            for (int j = 0; j < c; j++) 
            {
                Teuria teuria = teurias[j + i];
                batch.Draw(teuria.Position, teuria.Color);
            }

            // Creates new draw call
            batch.End();
        }
        batch.Begin(BatchPerf.Font.Texture, DrawSampler.PointClamp, camera);
        batch.Draw(BatchPerf.Font, $"FPS: {FPS}", Vector2.Zero, Color.White, new Vector2(0.2f));
        batch.Draw(BatchPerf.Font, $"Object Count: {count}", new Vector2(0, 20), Color.White, new Vector2(0.2f));
        batch.End();

        RenderPass renderPass = GraphicsDevice.BeginTarget(backbuffer, Color.CornflowerBlue, true);
        batch.Render(renderPass);
        GraphicsDevice.EndTarget(renderPass);
    }
}

public struct Teuria 
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Color Color;
}