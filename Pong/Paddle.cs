using System.Numerics;
using Riateu;
using Riateu.Audios;
using Riateu.Components;
using Riateu.Inputs;
using System;
using Riateu.Physics;
using System.Threading;
namespace Pong;
public class Paddle : Entity 
{
    private SpriteRenderer sprite;
    private KeyCode up;
    private bool IsOpponent = false;
    private KeyCode down;
    private PhysicsComponent area;
    private bool left;
    private float delayTick = 0;
    public const float Speed = 100.0f;
    private float CurrentDelta = 1f;
    private Thread CalculatingThread;
    private float axis = 0f;
    private bool ReadyToSubmit,ReadyToPlaySound = false;

    public Paddle(KeyCode up, KeyCode down, bool isOpponent = false, bool left = true) 
    {
        IsOpponent = isOpponent;
        sprite = new SpriteRenderer(Resource.Atlas, Resource.Atlas["pong/paddle"]);
        sprite.FlipX = !left;
        AddComponent(sprite);

        AddComponent(area = new PhysicsComponent(new AABB(this, 0, 0, 4, 24)));
        this.up = up;
        this.down = down;
        this.left = left;
        CalculatingThread = new Thread(CalculatingChanges);
        CalculatingThread.Start();
    }
    public void UpdatePos(float changes)
    {
        PosY = changes;
    }
    private void CalculatingChanges()
    {
        while(true)
        {
            if(ReadyToPlaySound==true)
            {
                ReadyToPlaySound=false;
                SocketThread.PlaySound();
            }
            if(ReadyToSubmit==true)
            {
                ReadyToSubmit=false;
                SocketThread.SubmitChanges(PosY);
            }
        }
    }

    public override void Update(double delta)
    {
        CurrentDelta = (float)delta;
        axis = GetAxis(up, down, CurrentDelta);
        float changesInPosition = (float) Math.Floor(axis * Speed * CurrentDelta*250)/100;
        PosY += changesInPosition;
        if(axis != 0) {
           ReadyToSubmit=true;
        }
        if (PosY > PingPongGame.ViewportHeight - 24) 
        {
            PosY = PingPongGame.ViewportHeight - 24;
        } 
        else if (PosY < 0) 
        {
            PosY = 0;
        }
        if(PingPongGame.IsClient==false) 
        {
            sbyte lVal = left ? (sbyte)-1 : (sbyte)1; 
            if (area.CheckAll<Ball>(Vector2.Zero, out Ball ball)) 
            {
                if (lVal == ball.Velocity.X)
                {
                    ball.Velocity.X *= -1;
                    ball.Velocity.Y += 100 * 0.005f;
                    SimpleScene.ball.PlaySound();
                    ReadyToPlaySound=true;
                }
            }
        }
        base.Update(CurrentDelta);
    }

    private int GetAxis(KeyCode up, KeyCode down, float delta) 
    {
        if(delayTick>= 0.02f) {
            delayTick=0f;
            if (Input.Keyboard.IsDown(up)) 
            {
                if(IsOpponent == false) return -1;
            }
            if (Input.Keyboard.IsDown(down)) 
            {
                if(IsOpponent == false) return 1;
            }
        }else delayTick+=delta;
        return 0;
    }
}