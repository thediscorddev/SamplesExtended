using System;
using MoonWorks;

internal interface Program
{
    public const int ScreenWidth = 1024;
    public const int ScreenHeight = 640;

    [STAThread]
    protected internal static void Main(string[] args) 
    {
        var windowInfo = new WindowCreateInfo("Hello Game", ScreenWidth, ScreenHeight, 
            ScreenMode.Windowed, PresentMode.FIFO);

        var frameLimiter = new FrameLimiterSettings(FrameLimiterMode.Capped, 60);

        var helloGame = new PingPongGame("Hello Game", ScreenWidth, ScreenHeight, debugMode: true);
        helloGame.Run();
    }
}