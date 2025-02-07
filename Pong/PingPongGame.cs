using Pong;
using Riateu;
using Riateu.Audios;
using Riateu.Content;
using System;
using System.Threading;
public class PingPongGame : GameApp
{
    public static bool IsRun = false;
    public static bool IsClient = false;
    public static bool isMultiPlayer = false;
    public static bool isStarted = false;
    public const int ViewportWidth = 128;
    public const int ViewportHeight = 80;

    public static int LastFrame = 0; // Used to track client position
    public static int LastFrameServer = 0; //Used to track host position

    public PingPongGame(WindowSettings settings, GraphicsSettings graphicsSettings) : base(settings, graphicsSettings)
    {
    }

    public override void LoadContent(AssetStorage storage)
    {
        Resource.PressStart2PFont = storage.LoadFont("Assets/font/PressStart2P-Regular.ttf", 32);
        Resource.Atlas = storage.CreateAtlas("Assets/images");
        Resource.Animations = storage.LoadAnimations("Assets/images/animation.json", Resource.Atlas);
        Resource.HitSound = storage.LoadAudioTrack("Assets/sfx/hit.wav", AudioFormat.WAV);
        Resource.BounceSound = storage.LoadAudioTrack("Assets/sfx/bounce.wav", AudioFormat.WAV);
    }
    public static void ConnectToServer()
    {
        string address = Console.ReadLine();
        Console.WriteLine("Port = (Default 8000)");
        string newPort = Console.ReadLine();
        int Port = 8000;
        try
        {
            Port = int.Parse(newPort);
            Console.WriteLine("Aight, I will use port " + Port.ToString());
        }
        catch (FormatException e)
        {
            Console.WriteLine("I don't understand so I will use port 8000 instead.");
        }
        Thread.Sleep(1000);
        Console.Clear();
        Console.WriteLine("Connecting.. If the connection is long enough, probably the provided address is invaild or the game has already started.");
        bool Connected = SocketThread.CreateConnection(address, Port);
        if (Connected == false)
        {
            Console.WriteLine("Failed to connect to the server! Try again..");
            ConnectToServer();
        }
        else
        {
            isStarted = true;
            Console.WriteLine("Successfully joined the server! Game starting soon..");
        }
    }
    public override GameLoop Initialize()
    {
        // TcpListener sv = new TcpListener(IPADD);
        IsRun = true;
        Console.WriteLine("Run this session as a multiplayer game or as an computer session? <y/n, default n>");
        string Data = Console.ReadLine();
        if (Data == "y") isMultiPlayer = true;
        if (isMultiPlayer == false) isStarted = true;
        else
        {
            Console.WriteLine("Are you going to host the game or not? <y/n, default y>");
            string IsHost = Console.ReadLine();
            if (IsHost == "n")
            {
                IsClient = true;
                Console.WriteLine("Since you are going to join a game, please enter their address. Make sure to stay in the same network: ");
                ConnectToServer();
            }
            else
            {
                Console.WriteLine("Port = (Default 8000)");
                string newPort = Console.ReadLine();
                int Port = 8000;
                try
                {
                    Port = int.Parse(newPort);
                    Console.WriteLine("Aight, I will use port " + Port.ToString());
                }
                catch (FormatException e)
                {
                    Console.WriteLine("I don't understand so I will use port 8000 instead.");
                }
                SocketThread.CreateServer(Port);
            }
        }
        return new SimpleScene(this);
    }
}