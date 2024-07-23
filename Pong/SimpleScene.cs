using MoonWorks.Graphics;
using MoonWorks.Input;
using MoonWorks.Math.Float;
using Riateu;
using Riateu.Graphics;

namespace Pong;
public class SimpleScene : Scene
{
    private Camera camera;
    private Ball ball;
    private int[] scores;
    private Batch batch;
    private string scoreText = "0     0";

    public SimpleScene(GameApp game) : base(game) 
    {
        batch = new Batch(game.GraphicsDevice, 512, 320);
        scores = new int[2];
    }

    public override void Begin()
    {
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
        camera = new Camera(PingPongGame.ViewportWidth, PingPongGame.ViewportHeight);
    }

    public override void Process(double delta) 
    {
        if (ball.PosX < -30) 
        {
            ball.Position = new Vector2((PingPongGame.ViewportWidth * 0.5f) - 8, (PingPongGame.ViewportHeight * 0.5f) - 4);
            ball.Velocity = new Vector2(-1, 0);
            scores[1] += 1;
            scoreText = $"{scores[0]}     {scores[1]}";
        }
        else if (ball.PosX > PingPongGame.ViewportWidth + 30) 
        {
            ball.Position = new Vector2((PingPongGame.ViewportWidth * 0.5f) - 8, (PingPongGame.ViewportHeight * 0.5f) - 4);
            ball.Velocity = new Vector2(1, 0);
            scores[0] += 1;
            scoreText = $"{scores[0]}     {scores[1]}";
        }
    }

    public override void Render(BackbufferTarget backbuffer)
    {
        batch.Begin(Resource.Atlas, DrawSampler.PointClamp, camera);
        EntityList.Draw(batch);
        batch.Draw(Resource.PressStart2PFont, scoreText, new Vector2(PingPongGame.ViewportWidth * 0.5f, 0), Color.White, new Vector2(0.2f), FontAlignment.Center);
        batch.End(true);

        RenderPass renderPass = backbuffer.BeginRendering(Color.Black);
        batch.Render(renderPass);
        backbuffer.EndRendering(renderPass);
    }

    public override void End()
    {
    }
}