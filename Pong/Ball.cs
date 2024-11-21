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
    private float delay,startingDelay = 0f;
    private int PosX_, PosY_; 
    private float CurrentDelta=1f;
    private Thread CalculatingThread;
    private bool ReadyToSubmit=false;
    private int AnimatingFrame = 0;
    private static Mutex mutex = new Mutex();


    public Ball() 
    {
        sprite = AnimatedSprite.Create(Resource.Atlas, Resource.Animations["pong/ball"]);
        sprite.FPS = 10;
        sprite.Play("idle");
        AddComponent(sprite);

        AddComponent(new PhysicsComponent(new AABB(this, new Rectangle(0, 0, 8, 8))));
        PosX_ = (int) Math.Round(((PingPongGame.ViewportWidth - 4)*50f));
        PosY_ = (int) Math.Round(PingPongGame.ViewportHeight * 50f);
        CalculatingThread = new Thread(Calculating);
        CalculatingThread.Start();
    }
    private void Calculating()
    {
        while(true)
        {
            if(ReadyToSubmit==true)
            {
                ReadyToSubmit=false;
                int ReflectedX = (int) (2*Math.Round(((PingPongGame.ViewportWidth - 4)*50f))-PosX_-300);
                SocketThread.BoardCastBallPositionChange(ReflectedX,PosY_,Velocity.X,Velocity.Y);
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
        PosX_ = (int) Math.Round(((PingPongGame.ViewportWidth - 4)*50f));
        PosY_ = (int) Math.Round(PingPongGame.ViewportHeight * 50f);
        PosX = (float)PosX_/100;
        PosY = (float)PosY_/100;
    }
    public void UpdatePosAndVelocity(int newPosX, int newPosY, float VelocityX, float VelocityY)
    {
        AnimatingFrame=0;
        PosX_=newPosX;
        PosY_=newPosY;
        Velocity.X=VelocityX;
        Velocity.Y=VelocityY;
    }

    public override void Update(double delta)
    {
        CurrentDelta = (float)delta;
        if(PingPongGame.IsClient==true)
        {
            PosX=((1-AnimatingFrame)*PosX+PosX_/100)/(2-AnimatingFrame);
            PosY=((1-AnimatingFrame)*PosY+PosY_/100)/(2-AnimatingFrame);
            if(AnimatingFrame<1)AnimatingFrame++;
        }
        if(delay >=0.05f && startingDelay>=3f && PingPongGame.IsClient==false)
        {
            delay=0;
            int dx = (int) Math.Round(Velocity.X * CurrentDelta * Speed*100)*2;
            int dy = (int) Math.Round(Velocity.Y * CurrentDelta * Speed*100)*2;
            float vdx = MathUtils.Clamp(Velocity.X, -7f*SpeedLimitX, 7f*SpeedLimitX);
            float vdy = MathUtils.Clamp(Velocity.Y, -7f*SpeedLimitY, 7f*SpeedLimitY);
            Velocity.X = vdx;
            Velocity.Y = vdy;
            PosX_+=dx;
            PosY_+=dy;
            PosX = (float)PosX_/100;
            PosY = (float)PosY_/100;
            ReadyToSubmit=true;
            if (PosY > PingPongGame.ViewportHeight - 8) 
            {
                Velocity.Y = -1;
                PlaySound();
            }
            else if (PosY < 0) 
            {
                Velocity.Y = 1;
                PlaySound();
            }
        }else if(delay < 0.05f && startingDelay>=3f) delay+=CurrentDelta;
        if(startingDelay == 0f)
        {
            Thread countdownThread = new Thread(startingCountDown);
            countdownThread.Start();
        }
        base.Update(CurrentDelta);
    }
    private void startingCountDown()
    {
        for(int i = 1; i <= 3; i++) 
        {
            startingDelay+=1f;
            SocketThread.sendClientMessage("StartingCooldown " + Math.Round(3f-i).ToString() + " ");
            SimpleScene.DelayText = "Starting in "+ Math.Round(3f-i).ToString() + " seconds...";
            if(Math.Round(3f-i) <= 0) SimpleScene.DelayText = "";
            Thread.Sleep(1000);
        }
    }
}