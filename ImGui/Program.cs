using System;
using System.Collections.Generic;
using System.Numerics;
using Riateu;
using Riateu.Content;
using Riateu.Graphics;
using Riateu.ImGuiRend;
using Riateu.Inputs;

namespace ImGui;

public class ImGuiGame : GameApp
{
    private Atlas atlas;
    public ImGuiGame(WindowSettings settings, GraphicsSettings graphicsSettings) : base(settings, graphicsSettings)
    {
    }

    public override GameLoop Initialize()
    {
        return new ImGuiGameLoop(this, atlas);
    }

    public override void LoadContent(AssetStorage storage)
    {
        atlas = storage.CreateAtlas("Assets/Textures");
    }

    public static void Main() 
    {
        ImGuiGame game = new ImGuiGame(
            new WindowSettings("ImGui App", 1024, 640),
            GraphicsSettings.Vsync
        );
        game.Run();
    }
}

public class ImGuiGameLoop : GameLoop
{
    private struct ImEntity(int x, int y, Vector2 vel)
    {
        public Vector2 Pos = new Vector2(x, y);
        public Vector2 Vel = vel;
    }
    private ImGuiRenderer renderer;
    private RenderTarget canvas;
    private Batch batch;
    private Atlas atlas;
    private Camera camera;

    private int num;
    private float triangleRotation;
    private IntPtr imgui_canvas;
    private List<ImEntity> imEntities = new List<ImEntity>();
    private Color[] colors = new Color[32];
    private Color currentColor;
    private float timer;
    private int inc;

    public ImGuiGameLoop(GameApp gameApp, Atlas atlas) : base(gameApp)
    {
        this.atlas = atlas;
        batch = new Batch(gameApp.GraphicsDevice, 512, 320);
        renderer = new ImGuiRenderer(gameApp.GraphicsDevice, gameApp.MainWindow, 1024, 640);
        canvas = new RenderTarget(GraphicsDevice, 512, 320);
        camera = new Camera(512, 302);

        Random random = new Random();

        for (int i = 0; i < 15; i++) 
        {
            imEntities.Add(new ImEntity(
                random.Next() % (512 - 48), random.Next() % (320 - 48), 
                new Vector2(random.Next() % 2 == 0 ? 1 : -1, random.Next() % 2 == 0 ? 1 : -1)));
        }

        RerollColor(random);
    }

    public override void Begin() 
    {
        imgui_canvas = renderer.BindTexture(canvas);
    }
    public override void End() {}

    public override void Update(double delta)
    {
        renderer.Update(GameInstance.InputDevice, ImGuiRender);
        timer += Time.Delta;

        if (Input.Keyboard.IsPressed(KeyCode.Left)) 
        {
            num--;
        }
        else if (Input.Keyboard.IsPressed(KeyCode.Right)) 
        {
            num++;
        }

        num = MathUtils.Wrap(num, 0, 4);

        triangleRotation += MathUtils.Radians * 20 * Time.Delta;

        if (num == 2) 
        {
            for (int i = 0; i < imEntities.Count; i++) 
            {
                var entity = imEntities[i];
                if (entity.Pos.X < 0) 
                {
                    entity.Vel.X *= -1;
                }
                if (entity.Pos.Y < 0) 
                {
                    entity.Vel.Y *= -1;
                }

                if (entity.Pos.X > 512 - 48) 
                {
                    entity.Vel.X *= -1;
                }
                if (entity.Pos.Y > 320 - 48) 
                {
                    entity.Vel.Y *= -1;
                }

                entity.Pos += entity.Vel;
                imEntities[i] = entity;
            }
        }

        if (num == 3) 
        {
            currentColor = Color.Lerp(currentColor, colors[inc], Time.Delta);
        }
        if (timer > 0.5f) 
        {
            inc++;
            if (inc == 31) 
            {
                RerollColor(new Random());
            }
            inc = MathUtils.Wrap(inc, 0, 32);
            timer = 0;
        }
    }

    private void RerollColor(Random random) 
    {
        for (int i = 0; i < colors.Length; i++) 
        {
            colors[i] = new Color((byte)(random.Next() % 255), (byte)(random.Next() % 255), (byte)(random.Next() % 255), 255);
        }
    }

    public override void Render(RenderTarget swapchainTarget)
    {
        batch.Begin(atlas, DrawSampler.PointClamp);
        Color clearColor = Color.CornflowerBlue;
        switch (num) 
        {
        case 0:
            batch.Draw(atlas["pixeltriangle"], new Vector2(((512 / 2) - 48 / 2) + 24, ((320 / 2) - 48 / 2) + 24), Color.White, 
                Vector2.One * 2, new Vector2(24, 24), triangleRotation, 1);
            break;
        case 1:
            batch.Draw(atlas["iloveimgui"], new Vector2((512 / 2) - 92 / 2, (320 / 2) - 15 / 2.0f), Color.White);
            break;
        case 2:
            foreach (var entity in imEntities) 
            {
                batch.Draw(atlas["pixeltriangle"], entity.Pos + new Vector2(24), Color.White, Vector2.One, new Vector2(24), 
                    triangleRotation * entity.Vel.X + entity.Vel.Y, 1);
            }
            break;
        case 3:
            clearColor = currentColor;
            break;
        }

        batch.End();

        RenderPass canvasPass = GraphicsDevice.BeginTarget(canvas, clearColor, true);
        batch.Render(canvasPass);
        GraphicsDevice.EndTarget(canvasPass);

        RenderPass pass = GraphicsDevice.BeginTarget(swapchainTarget, Color.Black, true);
        renderer.Render(pass);
        GraphicsDevice.EndTarget(pass);
    }

    public void ImGuiRender() 
    {
        ImGuiNET.ImGui.ShowDemoWindow();

        ImGuiNET.ImGui.Begin("The Canvas of your Majesty");
        ImGuiNET.ImGui.Image(imgui_canvas, new Vector2(512, 320));
        ImGuiNET.ImGui.End();
    }
}