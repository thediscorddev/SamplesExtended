using System;
using Riateu;

internal interface Program
{
    public const int ScreenWidth = 1024;
    public const int ScreenHeight = 640;

    [STAThread]
    protected internal static void Main(string[] args) 
    {

        var helloGame = new PingPongGame(
            new WindowSettings("Ping Pong", 1024, 640, WindowMode.Windowed),
            GraphicsSettings.Vsync
        );
        helloGame.Run();
    }
}