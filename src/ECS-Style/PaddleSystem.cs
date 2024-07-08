using MoonWorks.Input;
using MoonWorks.Math.Float;
using Riateu;
using Riateu.ECS;

namespace Pong.ECS;


public class PaddleSystem : UpdateSystem
{
    private ComponentQuery PaddleUpdate;
    private CollisionSystem collisionSystem;

    public PaddleSystem(World world, CollisionSystem collisionSystem) : base(world)
    {
        this.collisionSystem = collisionSystem;
        PaddleUpdate = World.QueryBuilder
            .Include<Vector2>()
            .Include<Paddle>()
            .Build();
    }

    public override void Update(double delta)
    {
        float deltaFloat = (float)delta;

        foreach (var entity in PaddleUpdate.Entities) 
        {
            ref Vector2 position = ref World.GetComponent<Vector2>(entity);
            ref Paddle paddle = ref World.GetComponent<Paddle>(entity);
            var axis = GetAxis(paddle.up, paddle.down);

            position.Y += axis * 100.0f * deltaFloat;
            if (position.Y > PingPongGame.ViewportHeight - 24) 
            {
                position.Y = PingPongGame.ViewportHeight - 24;
            } 
            else if (position.Y < 0) 
            {
                position.Y = 0;
            }
            sbyte lVal = position.X < 50 ? (sbyte)-1 : (sbyte)1;
            foreach (var ballEntity in collisionSystem.HitList[entity]) 
            {
                ref Ball ball = ref World.GetComponent<Ball>(ballEntity);
                if (lVal == ball.velocity.X)
                {
                    ball.velocity = new Vector2(ball.velocity.X * -1, ball.velocity.Y + 100 * 0.005f);
                }
            }
        }
    }

    private int GetAxis(KeyCode up, KeyCode down) 
    {
        if (Input.InputSystem.Keyboard.IsDown(up)) 
        {
            return -1;
        }
        if (Input.InputSystem.Keyboard.IsDown(down)) 
        {
            return 1;
        }
        return 0;
    }
}