using MoonWorks.Math;
using MoonWorks.Math.Float;
using Pong.ECS;

namespace Riateu.ECS;

public class BallSystem : UpdateSystem
{
    private ComponentQuery BallUpdate;

    public BallSystem(World world) : base(world)
    {
        BallUpdate = World.QueryBuilder
            .Include<Vector2>()
            .Include<Ball>()
            .Build();
    }

    public override void Update(double delta)
    {
        float deltaFloat = (float)delta;
        foreach (var entity in BallUpdate.Entities) 
        {
            ref Vector2 position = ref World.GetComponent<Vector2>(entity);
            ref Ball ball = ref World.GetComponent<Ball>(entity);

            ball.velocity = new Vector2(
                MathHelper.Clamp(ball.velocity.X, -80f, 80f),
                MathHelper.Clamp(ball.velocity.Y, -1.0f, 1.0f)
            );

            position.X += ball.velocity.X * deltaFloat * ball.speed;
            position.Y += ball.velocity.Y * deltaFloat * ball.speed;

            if (position.Y > PingPongGame.ViewportHeight - 8) 
            {
                ball.velocity = new Vector2(ball.velocity.X, -1);
            }
            else if (position.Y < 0) 
            {
                ball.velocity = new Vector2(ball.velocity.X, 1);
            }

            if (position.X < -30) 
            {
                position = new Vector2((PingPongGame.ViewportWidth * 0.5f) - 8, (PingPongGame.ViewportHeight * 0.5f) - 4);
                ball.velocity = new Vector2(-1, 0);
                Send<ScoreMessage>(new ScoreMessage(true));
            }
            else if (position.X > PingPongGame.ViewportWidth + 30) 
            {
                position = new Vector2((PingPongGame.ViewportWidth * 0.5f) - 8, (PingPongGame.ViewportHeight * 0.5f) - 4);
                ball.velocity = new Vector2(1, 0);
                Send<ScoreMessage>(new ScoreMessage(false));
            }
        }
    }
}

public record struct ScoreMessage(bool left);