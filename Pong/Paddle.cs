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
    private Sprite sprite;
    private KeyCode up;
    private bool IsOpponent = false;
    private KeyCode down;
    private Collision area;
    private bool left;
    private float delayTick = 0;
    public const float Speed = 100.0f;
    private float CurrentDelta = 1f;
    private Thread CalculatingThread;
    private float axis = 0f;
    private bool ReadyToSubmit, ReadyToPlaySound = false;
    private int LastSentTime = 0;

    private int LastAxis = 0; // force update
    public Paddle(KeyCode up, KeyCode down, bool isOpponent = false, bool left = true)
    {
        IsOpponent = isOpponent;
        sprite = new Sprite(Resource.Atlas.Data["pong/paddle"]);
        sprite.FlipX = !left;
        AddComponent(sprite);

        AddComponent(area = new Collision(new AABB(this, 0, 0, 4, 24)));
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
        while (true)
        {
            if (ReadyToPlaySound == true)
            {
                ReadyToPlaySound = false;
                SocketThread.PlaySound();
            }
            if (ReadyToSubmit == true)
            {
                ReadyToSubmit = false;
                SocketThread.SubmitChanges(PosY);
            }
        }
    }

    public override void Update(double delta)
    {
        CurrentDelta = (float)delta;
        axis = GetAxis(up, down, CurrentDelta);
        PosY += axis * 100 * CurrentDelta;

        if (axis != 0) ReadyToSubmit = true;
        if (axis == 0 && LastAxis != 0)  // Ensure ReSync is called when movement stops
        {
            SocketThread.ReSync();
        }
        LastAxis = (int)axis; // Update LastAxis after checking


        if (PosY > PingPongGame.ViewportHeight - 24)
        {
            PosY = PingPongGame.ViewportHeight - 24;
        }
        else if (PosY < 0)
        {
            PosY = 0;
        }

        sbyte lVal = left ? (sbyte)-1 : (sbyte)1;
        if (area.CheckAll<Ball>(Vector2.Zero, out Ball ball))
        {
            if (lVal == ball.Velocity.X)
            {
                ball.Velocity.X *= -1;
                ball.Velocity.Y += 100 * 0.005f;
                Audio.PlaySound(Resource.BounceSound);
            }
        }

        base.Update(CurrentDelta);
    }

    private int GetAxis(KeyCode up, KeyCode down, float delta)
    {
        if (Input.Keyboard.IsDown(up))
        {
            if (IsOpponent == false) return -1;
        }
        if (Input.Keyboard.IsDown(down))
        {
            if (IsOpponent == false) return 1;
        }
        return 0;
    }
}