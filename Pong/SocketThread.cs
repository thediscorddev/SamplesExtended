using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Pong;
class SocketThread
{
    public static int port = 8000;
    public static string SAddress = null; // it will be a string if clientSide is true.
    private static TcpListener server = null;
    private static Thread ConnectionThreadServer = null;
    private static Thread ConnectionThread = null;
    private static Thread ConnectionThread_kick = null;
    private static TcpClient Player = null;
    private static TcpClient ServerConnection = null;
    private static Mutex mutex = new Mutex();
    public static void CreateServer(int Port = 8000)
    {
        port = Port;
        try
        {
            IPAddress localAddr = IPAddress.Any;
            server = new TcpListener(localAddr, port);
            server.Start();
            string hostName = Dns.GetHostName();
            IPHostEntry hostEntry = Dns.GetHostEntry(hostName);
            foreach(IPAddress address in hostEntry.AddressList)
            {
                if(address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    if(address.ToString() == "127.0.0.1") continue;
                    Console.WriteLine("Available Connection address: " + address.ToString());
                }
            }
            Console.WriteLine("Waiting...");
            Player = server.AcceptTcpClient();
            ConnectionThread_kick = new Thread(KickOffPlayerIfRoomIsFull);
            ConnectionThread_kick.Start();
            ConnectionThread = new Thread(HandleClient);
            ConnectionThread.Start();
            Console.Clear();
            Console.WriteLine("Player Connected! Starting the game..");
        }catch(Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
        }
    }
    public static void KickOffPlayerIfRoomIsFull()
    {
        Thread.Sleep(1000);
        TcpClient NewPlayer = server.AcceptTcpClient();
        NetworkStream stream = NewPlayer.GetStream();
        byte[] buffer = new byte[1024];
        int bytesRead;
        while((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
        {
            string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            if(message == "StartedOrNot")
            {
                if(PingPongGame.isStarted == false) sendClientMessage("Approxed");
                else if(PingPongGame.isStarted == true && NewPlayer != Player) sendClientMessage("Disapproxed");
                else sendClientMessage("Disapproxed");
            }
        }
        KickOffPlayerIfRoomIsFull();
    }
    public static bool CreateConnection(string address, int Port=8000)
    {
        port = Port;
        try
        {
            TcpClient Server = new TcpClient(address, port);
            ServerConnection = Server;
            NetworkStream stream = Server.GetStream();
            string message = "StartedOrNot";
            sendServerMessage(message);
            byte[] buffer = new byte[1024];
            int AvailableBytes = stream.Read(buffer, 0, buffer.Length);
            string response = Encoding.ASCII.GetString(buffer, 0, AvailableBytes);
            if (response == "Approxed")
            {
                ConnectionThreadServer = new Thread(HandleServer);
                ConnectionThreadServer.Start();
                Console.WriteLine("Connected");
                return true;
            }
            else
            {
                Console.WriteLine("Game already started!");
                Server.Close();
                return false;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    public static void HandleServer()
    {
        mutex.WaitOne();
        while(true)
        {
            NetworkStream stream = ServerConnection.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;
            while((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                var arg = message.Split(" ");
                if(arg[0] == "Pos") {
                    SimpleScene.Opponent.UpdatePos(float.Parse(arg[1]));
                }else if(arg[0] == "BallPos")
                {
                    SimpleScene.ball.UpdatePosAndVelocity(int.Parse(arg[1]),int.Parse(arg[2]),float.Parse(arg[3]),float.Parse(arg[4]));
                }else if(arg[0] == "PlaySound")
                {
                    SimpleScene.ball.PlaySound();
                }else if(arg[0] == "Score")
                {
                    SimpleScene.UpdateScoreAndResetBall(int.Parse(arg[2]),int.Parse(arg[1]));
                }else if(arg[0] == "StartingCooldown")
                {
                    SimpleScene.DelayText = "Starting in "+ arg[1] + " seconds...";
                    if(int.Parse(arg[1]) <= 0) SimpleScene.DelayText = "";
                }
            }
        }
        
    }
    public static void BoardCastBallPositionChange(int dx, int dy, float vdx, float vdy)
    {
        string message = "BallPos " + dx.ToString() + " " + dy.ToString() + " " + vdx.ToString() + " " + vdy.ToString() + " ";
        sendClientMessage(message);
    }
    public static void sendClientMessage(string message)
    {
        Thread.Sleep(150);
        if(PingPongGame.IsClient == false) {
            NetworkStream stream = Player.GetStream();
            byte[] messageAsByte = Encoding.ASCII.GetBytes(message);
            stream.Write(messageAsByte, 0, messageAsByte.Length);
        }
    }
    public static void sendServerMessage(string message)
    {
        if(PingPongGame.IsClient==true) {
            NetworkStream stream = ServerConnection.GetStream();
            byte[] messageAsByte = Encoding.ASCII.GetBytes(message);
            stream.Write(messageAsByte, 0, messageAsByte.Length);
        }
    }
    public static void PlaySound()
    {
        string message = "PlaySound ";
        sendClientMessage(message);
    }
    public static void HandleClient()
    {
        while(true) {
            NetworkStream stream = Player.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead;
            while((bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                if(message.Split(" ")[0] == "Pos") {
                    SimpleScene.Opponent.UpdatePos(float.Parse(message.Split(" ")[1]));
                }
                if(message == "StartedOrNot")
                {
                    if(PingPongGame.isStarted == false) {
                        PingPongGame.isStarted = true;
                        sendClientMessage("Approxed");
                    }
                    else sendClientMessage("Disapproxed");
                }
            }
        }
        /*TODO*/
    }
    public static void ResetBallAndBoardCastScore(int ServerScore, int ClientScore)
    {
        sendClientMessage("Score " + ServerScore.ToString() + " " + ClientScore + " ");
    }
    public static void SubmitChanges(float BallPositionChanges)
    {
        string message = "Pos " + BallPositionChanges.ToString() + " ";
        if(PingPongGame.IsClient == true) sendServerMessage(message);
        else sendClientMessage(message);
    }
}