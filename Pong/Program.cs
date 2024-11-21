using System;
using Riateu;

internal interface Program
{
    public const int ScreenWidth = 512;
    public const int ScreenHeight = 320;

    [STAThread]
    protected internal static void Main(string[] args) 
    {

        var helloGame = new PingPongGame(
            new WindowSettings("Ping Pong", 512, 320, WindowMode.Windowed),
            GraphicsSettings.Vsync
        );
        helloGame.Run();
    }
}