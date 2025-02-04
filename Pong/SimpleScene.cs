using System.Numerics;
using Riateu;
using Riateu.Audios;
using Riateu.Graphics;
using Riateu.Inputs;
using System;
using System.Threading;
namespace Pong;
public class SimpleScene : Scene
{
    private Camera camera;
    public static Ball ball;
    private static int[] scores;
    private Batch batch;
    public static Paddle CurrentPlayer;
    public static Paddle Opponent;
    private string scoreText = "You          Opponent";
    private static string scoreTexts = "0            0";
    public static string DelayText = "Starting in 3 seconds...";

    public SimpleScene(GameApp game) : base(game) 
    {
        batch = new Batch(game.GraphicsDevice, 256, 160);
        scores = new int[2];
    }
    public static void UpdateScoreAndResetBall(int YourScore, int OpponentScore)
    {
        ball.Position = new Vector2((PingPongGame.ViewportWidth * 0.5f), (PingPongGame.ViewportHeight * 0.5f));
        ball.Velocity = new Vector2(-1, 0);
        scoreTexts = $"{YourScore}                {OpponentScore}";
        ball.resetPos();
    }

    public override void Begin()
    {
        CurrentPlayer = new Paddle(KeyCode.Up, KeyCode.Down);
        CurrentPlayer.PosY = (PingPongGame.ViewportHeight * 0.5f) - 12;
        Add(CurrentPlayer);
        Opponent = new Paddle(KeyCode.Up, KeyCode.Down,true, false);
        Opponent.PosX = PingPongGame.ViewportWidth - 4;
        Opponent.PosY = (PingPongGame.ViewportHeight * 0.5f) - 12;
        Add(Opponent);
        Console.Write(CurrentPlayer.PosX);
        Console.Write("     ");
        Console.WriteLine(CurrentPlayer.PosY);
        Console.Write(Opponent.PosX);
        Console.Write("     ");
        Console.WriteLine(Opponent.PosY);
        Console.WriteLine("______________");
        ball = new Ball();
        ball.PosX = ((PingPongGame.ViewportWidth - 4)*0.5f);
        ball.PosY = (PingPongGame.ViewportHeight * 0.5f);
        Add(ball);
        ball.Velocity = new Vector2(-1, 0);
        camera = new Camera(PingPongGame.ViewportWidth, PingPongGame.ViewportHeight);
    }

    public override void Process(double delta) 
    {
        if(PingPongGame.IsClient == false) {
            if (ball.PosX < -30) 
            {
                ball.resetPos();
                ball.Velocity = new Vector2(-1, 0);
                scores[1] += 1;
                scoreTexts = $"{scores[0]}                {scores[1]}";
                SocketThread.ResetBallAndBoardCastScore(scores[0],scores[1]);

            }
            else if (ball.PosX > PingPongGame.ViewportWidth + 30) 
            {
                ball.resetPos();
                ball.Velocity = new Vector2(1, 0);
                scores[0] += 1;
                scoreTexts = $"{scores[0]}                {scores[1]}";
                SocketThread.ResetBallAndBoardCastScore(scores[0],scores[1]);
            }
        }
    }

    public override void End()
    {
    }

    public override void Render(CommandBuffer commandBuffer, RenderTarget backbuffer)
    {
        batch.Begin(Resource.Atlas.Data, DrawSampler.PointClamp, camera);
        EntityList.Draw(batch);
        batch.End();
        batch.Begin(Resource.PressStart2PFont.Texture, DrawSampler.PointClamp, camera);
        batch.Draw(Resource.PressStart2PFont, scoreText, new Vector2(PingPongGame.ViewportWidth * 0.5f, 0), Color.White, new Vector2(0.2f), FontAlignment.Center);
        batch.Draw(Resource.PressStart2PFont, scoreTexts, new Vector2(PingPongGame.ViewportWidth * 0.5f, 7f), Color.White, new Vector2(0.2f), FontAlignment.Center);
        batch.Draw(Resource.PressStart2PFont, DelayText, new Vector2(PingPongGame.ViewportWidth * 0.5f, PingPongGame.ViewportHeight*0.5f-10), Color.White, new Vector2(0.2f), FontAlignment.Center);
        batch.End();
        batch.Flush(commandBuffer);
        RenderPass renderPass = GraphicsDevice.BeginTarget(backbuffer, Color.Black, true);
        batch.Render(renderPass);
        GraphicsDevice.EndTarget(renderPass);
    }
}