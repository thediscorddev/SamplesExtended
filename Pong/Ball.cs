using System;
using Riateu;
using Riateu.Audios;
using System.Numerics;
using Riateu.Components;
using Riateu.Graphics;
using Riateu.Physics;
using System.Threading;
namespace Pong;

public class Ball : Entity
{
    public Vector2 Velocity;
    private AnimatedSprite sprite;
    private const float SpeedLimitX = 80f;
    private const float SpeedLimitY = 1.0f;
    public float Speed = 100.0f;
    private float delay, startingDelay = 0f;
    private float delay_ = 0f;
    private int PosX_, PosY_;
    private float CurrentDelta = 1f;
    private Thread CalculatingThread;
    private bool ReadyToSubmit = false;
    private int AnimatingFrame = 0;
    private static Mutex mutex = new Mutex();


    public Ball()
    {
        sprite = AnimatedSprite.Create(Resource.Animations["pong/ball"]);
        sprite.FPS = 10;
        sprite.Play("idle");
        AddComponent(sprite);

        AddComponent(new Collision(new AABB(this, new Rectangle(0, 0, 8, 8))));
        PosX_ = (int)Math.Round(((PingPongGame.ViewportWidth - 4) * 50f));
        PosY_ = (int)Math.Round(PingPongGame.ViewportHeight * 50f);
        if (PingPongGame.IsClient == false)
        {
            CalculatingThread = new Thread(Calculating);
            CalculatingThread.Start();
        }
    }
    private void Calculating()
    {
        while (true)
        {
            if (ReadyToSubmit == true)
            {
                ReadyToSubmit = false;
                int ReflectedX = (int)(2 * Math.Round(((PingPongGame.ViewportWidth - 4) * 50f)) - PosX_ - 300);
                SocketThread.BoardCastBallPositionChange(ReflectedX, PosY_, Velocity.X, Velocity.Y);
            }
        }
    }
    public void PlaySound()
    {
        Audio.PlaySound(Resource.BounceSound);
        SocketThread.PlaySound();
    }
    public void resetPos()
    {
        PosX_ = (int)Math.Round(((PingPongGame.ViewportWidth - 4) * 50f));
        PosY_ = (int)Math.Round(PingPongGame.ViewportHeight * 50f);
        PosX = (float)PosX_ / 100;
        PosY = (float)PosY_ / 100;
    }
    public void UpdatePosAndVelocity(int newPosX, int newPosY, float VelocityX, float VelocityY)
    {
        AnimatingFrame = 0;
        PosX_ = newPosX;
        PosY_ = newPosY;
        Velocity.X = VelocityX;
        Velocity.Y = VelocityY;
    }

    public override void Update(double delta)
    {
        if (startingDelay == 0f)
        {
            Thread countdownThread = new Thread(startingCountDown);
            countdownThread.Start();
        }
        CurrentDelta = (float)delta;
        if (startingDelay >= 3f && delay >= 0.05f)
        {
            delay = 0f;
            int dx = (int)Math.Round(Velocity.X * CurrentDelta * Speed * 100) * 2;
            int dy = (int)Math.Round(Velocity.Y * CurrentDelta * Speed * 100) * 2;
            float vdx = MathUtils.Clamp(Velocity.X, -7f * SpeedLimitX, 7f * SpeedLimitX);
            float vdy = MathUtils.Clamp(Velocity.Y, -7f * SpeedLimitY, 7f * SpeedLimitY);
            Velocity.X = vdx;
            Velocity.Y = vdy;
            if (PingPongGame.IsClient) PosX_ -= dx;
            else PosX_ += dx;
            PosY_ += dy;
            PosX = (float)PosX_ / 100;
            PosY = (float)PosY_ / 100;

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


        }
        else if (startingDelay >= 3f) delay += CurrentDelta;
        if (PingPongGame.IsClient == false)
        {
            if (delay_ >= 0.085f)
            {
                delay_ = 0f;
                ReadyToSubmit = true;
            }
            else delay_ += CurrentDelta;
        }
        base.Update(CurrentDelta);
    }
    private void startingCountDown()
    {
        for (int i = 1; i <= 3; i++)
        {
            startingDelay += 1f;
            SocketThread.sendClientMessage("StartingCooldown " + Math.Round(3f - i).ToString() + " ");
            SimpleScene.DelayText = "Starting in " + Math.Round(3f - i).ToString() + " seconds...";
            if (Math.Round(3f - i) <= 0) SimpleScene.DelayText = "";
            Thread.Sleep(1000);
        }
    }
}