using System.Numerics;
using Riateu;
using Riateu.Audios;
using Riateu.Components;
using Riateu.Graphics;
using Riateu.Physics;

namespace Pong;

public class Ball : Entity 
{
    public Vector2 Velocity;
    private AnimatedSprite sprite;
    private const float SpeedLimitX = 80f;
    private const float SpeedLimitY = 1.0f;
    public float Speed = 100.0f;

    public Ball() 
    {
        sprite = AnimatedSprite.Create(Resource.Atlas, Resource.Animations["pong/ball"]);
        sprite.FPS = 10;
        sprite.Play("idle");
        AddComponent(sprite);

        AddComponent(new PhysicsComponent(new AABB(this, new Rectangle(0, 0, 8, 8))));
    }

    public override void Update(double delta)
    {
        float fDelta = (float)delta;
        Velocity.X = MathUtils.Clamp(Velocity.X, -SpeedLimitX, SpeedLimitX);
        Velocity.Y = MathUtils.Clamp(Velocity.Y, -SpeedLimitY, SpeedLimitY);

        PosX += Velocity.X * fDelta * Speed;
        PosY += Velocity.Y * fDelta * Speed;

        if (PosY > PingPongGame.ViewportHeight - 8) 
        {
            Velocity.Y = -1;
            Audio.PlaySound(Resource.BounceSound);
        }
        else if (PosY < 0) 
        {
            Velocity.Y = 1;
            Audio.PlaySound(Resource.BounceSound);
        }
        base.Update(delta);
    }
}