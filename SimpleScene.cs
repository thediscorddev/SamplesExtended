using MoonWorks.Graphics;
using MoonWorks.Input;
using MoonWorks.Math.Float;
using Riateu;
using Riateu.Graphics;

public class SimpleScene : Scene
{
    private Camera camera;
    private Ball ball;
    private int[] scores;
    private DynamicText scoreText;

    public SimpleScene(GameApp game) : base(game) {}

    public override void Begin()
    {
        scores = new int[2];
        var player1 = new Paddle(KeyCode.W, KeyCode.S);
        player1.PosY = (PingPongGame.ViewportHeight * 0.5f) - 12;
        Add(player1);
        var player2 = new Paddle(KeyCode.Up, KeyCode.Down, false);
        player2.PosX = PingPongGame.ViewportWidth - 4;
        player2.PosY = (PingPongGame.ViewportHeight * 0.5f) - 12;
        Add(player2);

        ball = new Ball();
        ball.PosX = (PingPongGame.ViewportWidth * 0.5f) - 8;
        ball.PosY = (PingPongGame.ViewportHeight * 0.5f) - 4;
        Add(ball);
        ball.Velocity = new Vector2(-1, 0);
        camera = new Camera(512, 320);

        scoreText = new DynamicText(GameInstance.GraphicsDevice, Resource.PressStart2PFont, "0     0", 16);
    }

    public override void Update(double delta) 
    {
        if (ball.PosX < -30) 
        {
            ball.Position = new Vector2((PingPongGame.ViewportWidth * 0.5f) - 8, (PingPongGame.ViewportHeight * 0.5f) - 4);
            ball.Velocity = new Vector2(-1, 0);
            scores[1] += 1;
            scoreText.Text = $"{scores[0]}     {scores[1]}";
        }
        else if (ball.PosX > PingPongGame.ViewportWidth + 30) 
        {
            ball.Position = new Vector2((PingPongGame.ViewportWidth * 0.5f) - 8, (PingPongGame.ViewportHeight * 0.5f) - 4);
            ball.Velocity = new Vector2(1, 0);
            scores[0] += 1;
            scoreText.Text = $"{scores[0]}     {scores[1]}";
        }
    }

    public override void Draw(CommandBuffer buffer, Texture backbuffer, Batch batch)
    {
        batch.Add(SceneCanvas.CanvasTexture, GameContext.GlobalSampler, Vector2.Zero, Matrix3x2.Identity);
        scoreText.Draw(batch, new Vector2((PingPongGame.ViewportWidth) - (scoreText.Bounds.Width * 0.5f), 0));
        batch.FlushVertex(buffer);

        buffer.BeginRenderPass(new ColorAttachmentInfo(backbuffer, Color.Black));
        buffer.BindGraphicsPipeline(GameContext.DefaultPipeline);
        batch.PushMatrix(camera);
        batch.Draw(buffer);
        batch.PopMatrix();

        buffer.EndRenderPass();
    }

    public override void End()
    {
    }
}