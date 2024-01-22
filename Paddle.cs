using MoonWorks.Input;
using Riateu;
using Riateu.Components;
using Riateu.Physics;


public class Paddle : Entity 
{
    private SpriteRenderer sprite;
    private KeyCode up;
    private KeyCode down;
    public const float Speed = 100.0f;

    public Paddle(KeyCode up, KeyCode down, bool left = true) 
    {
        sprite = new SpriteRenderer(Resource.AtlasTexture, Resource.Atlas["pong/paddle"]);
        sprite.Flip = !left;
        AddComponent(sprite);

        AddComponent(new PhysicsComponent(new AABB(this, 0, 0, 4, 24), OnCollided));
        this.up = up;
        this.down = down;
    }

    private void OnCollided(Entity entity, PhysicsComponent component) 
    {
        if (entity is Ball ball) 
        {
            ball.Velocity.X *= -1;
            ball.Velocity.Y += 100 * 0.005f;
        }
    }

    public override void Update(double delta)
    {
        float deltaFloat = (float)delta;
        var axis = GetAxis(up, down);

        PosY += axis * Speed * deltaFloat;
        if (PosY > PingPongGame.ViewportHeight - 24) 
        {
            PosY = PingPongGame.ViewportHeight - 24;
        } 
        else if (PosY < 0) 
        {
            PosY = 0;
        }
        
        base.Update(delta);
    }

    private int GetAxis(KeyCode up, KeyCode down) 
    {
        if (Scene.GameInstance.Inputs.Keyboard.IsDown(up)) 
        {
            return -1;
        }
        if (Scene.GameInstance.Inputs.Keyboard.IsDown(down)) 
        {
            return 1;
        }
        return 0;
    }
}