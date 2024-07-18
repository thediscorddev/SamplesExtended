﻿using System;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Math.Float;
using Riateu;
using Riateu.Content;
using Riateu.Graphics;

public class BatchPerf : GameApp
{
    public static Texture TeuriaLogo;
    public static Texture FontTexture;
    public static SpriteFont Font;

    public BatchPerf(string title, uint width, uint height, ScreenMode screenMode = ScreenMode.Windowed, bool debugMode = false) : base(title, width, height, screenMode, debugMode)
    {
    }

    public override void LoadContent(AssetStorage storage)
    {
        TeuriaLogo = storage.LoadTexture("Assets/teuria-logo.png");
        FontTexture = storage.LoadTexture("Assets/PressStart2P_0.png");
        Font = storage.LoadFont("Assets/PressStart2P.fnt", FontTexture);
    }

    public override void Initialize()
    {
        Scene = new TestLoop(this);
    }

    public static void Main() 
    {
        BatchPerf batchPerf = new BatchPerf("Batch Performance", 1024, 640);
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
        if (Input.InputSystem.Keyboard.IsPressed(MoonWorks.Input.KeyCode.Up)) 
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
        else if (Input.InputSystem.Keyboard.IsPressed(MoonWorks.Input.KeyCode.Down)) 
        {
            count = Math.Max(0, count - INC_DEC_COUNT);
        }
        Vector2 gameSize = new Vector2(512, 320);
        Span<Teuria> teurias = Teurias.AsSpan();
        for (int i = 0; i < count; i++) 
        {
            teurias[i].Position += teurias[i].Velocity;

            if (teurias[i].Position.X + 32 > gameSize.X || teurias[i].Position.X < 0) 
            {
                teurias[i].Velocity.X *= -1;
            }
            if (teurias[i].Position.Y + 32 > gameSize.Y || teurias[i].Position.Y < 0) 
            {
                teurias[i].Velocity.Y *= -1;
            }
        }
        FPS = Time.FPS;
    }

    public override void Render(BackbufferTarget backbuffer)
    {
        batch.Begin(BatchPerf.TeuriaLogo, DrawSampler.PointClamp, camera);
        ReadOnlySpan<Teuria> teurias = Teurias.AsSpan();
        for (int i = 0; i < count; i += BatchSize)
        {
            var c = Math.Min(count - i, BatchSize);

            for (int j = 0; j < c; j++) 
            {
                Teuria teuria = teurias[j + i];
                batch.Draw(teuria.Position, teuria.Color);
            }

            // Creates new draw call
            batch.Compose(BatchPerf.TeuriaLogo, DrawSampler.PointClamp);
        }
        batch.Compose(BatchPerf.FontTexture, DrawSampler.PointClamp);
        batch.Draw(BatchPerf.Font, $"FPS: {FPS}", Vector2.Zero, Color.White, new Vector2(0.2f));
        batch.Draw(BatchPerf.Font, $"Object Count: {count}", new Vector2(0, 20), Color.White, new Vector2(0.2f));
        batch.End();

        backbuffer.BeginRendering(Color.CornflowerBlue);
        backbuffer.Render(batch);
        backbuffer.EndRendering();
    }
}

public struct Teuria 
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Color Color;
}