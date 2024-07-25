using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using UnityEngine;
using EFT;
using Comfort.Common;
using Newtonsoft.Json;
using EFT.NextObservedPlayer;
using BSG.CameraEffects;
using System.Reflection;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.Visual;
using Diz.Skinning;
using UnityEngine.Rendering;
using System.Runtime.Remoting.Messaging;
using EFT.UI;
using UnityDiagnostics;
using Newtonsoft.Json.Linq;

namespace BasicMonoSDK
{
    public static class Globals
    {
        public static Camera MainCamera;
        public static GameWorld GameWorld;
        public static Player LocalPlayer;
        public static AssetBundle Custom;
        public static TcpListener Server = new TcpListener(IPAddress.Parse("127.0.0.1"), 3322);
        public static TcpClient client;

        public static List<IPlayer> Players = new List<IPlayer>();
        public static List<Throwable> Grenades = new List<Throwable>();

        public static bool IsMenuOpen = false;

        public static Vector3 W2S(Vector3 pos)
        {
            if (!Globals.MainCamera)
                return new Vector3(0, 0, 0);

            var screenPoint = Globals.MainCamera.WorldToScreenPoint(pos);
            var scale = Screen.height / (float)Globals.MainCamera.scaledPixelHeight;
            screenPoint.y = Screen.height - screenPoint.y * scale;
            screenPoint.x *= scale;
            if (screenPoint.x < -10 || screenPoint.y < -10 || screenPoint.z < 0)
                return new Vector3(0, 0, 0);

            return screenPoint;

        }

        // Not the best way but works.
        public static bool IsBossByName(string name)
        {
            if (name == "Килла" || name == "Решала" || name == "Глухарь" || name == "Штурман" || name == "Санитар" || name == "Тагилла" || name == "Зрячий" || name == "Кабан" || name == "Big Pipe" || name == "Birdeye" || name == "Knight" || name == "Дед Мороз" || name == "Коллонтай")
                return true;
            else
                return false;
        }

        internal static void SetPrivateField(this object obj, string name, object value)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            Type type = obj.GetType();
            FieldInfo fieldInfo = type.GetField(name, bindingFlags);
            fieldInfo.SetValue(obj, value);
        }
    }

    public static class MenuVars
    {
        public static bool EnableESP = true;
        public static bool EnableName = true;
        public static bool EnableBox = true;
        public static bool EnableLines = true;
        public static bool EnableChams = true;//no
        public static bool EnableGrenadeESP = false;
        public static bool watermark = true;
        public static bool norecoil = true;
        public static bool novsior = true;
        public static bool nosway = true;
        public static bool crosshair = true;


        public static float VR = 1;
        public static float VB = 1;
        public static float VG = 1;
        public static float IR = 1;
        public static float IG = 1;
        public static float IB = 1;
        public static bool BigHead = false;
        public static float MaxScavRenderDistance = 200f;

        public static bool ForceNightVision = false;
        public static bool ForceThermalVision = false;
    }

    public class Cheat : MonoBehaviour
    {
        public static Shader? OutlineShader { get; private set; }

        private void OnGUI()
        {

            if (MenuVars.watermark)
            {
                GUI.Label(new Rect(10f, 10f, 300f, 100f), "Brandon Tarrant's Menu");

            }

            if (MenuVars.EnableChams)
                Chams();
            if (MenuVars.crosshair)
                crosshair();
            if (MenuVars.novsior)
                novisor();


        }

        private void Update()
        {
            float LastCacheTime = 0f;

            if (Input.GetKeyUp(KeyCode.Insert))
                Globals.IsMenuOpen = !Globals.IsMenuOpen;

            if (Globals.IsMenuOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            /*
            if (!Globals.Server.Server.IsBound)
            {
                Globals.Server.Start();
            }
            if (Globals.Server.Pending())
            {
                TcpClient client = Globals.Server.AcceptTcpClient();
                Console.WriteLine("Client connected.");

                // Handle client communication asynchronously
                Task.Run(() => HandleClientCommunication(client));
            }
            */
            // Updates every 0.25f seconds.
            if (Time.time >= LastCacheTime)
            {
                if (Globals.Custom != null)
                    Globals.Custom = AssetBundle.LoadFromMemory(System.IO.File.ReadAllBytes(""));
                if (Camera.main != null)
                    Globals.MainCamera = Camera.main;

                if (Singleton<GameWorld>.Instance != null)
                    Globals.GameWorld = Singleton<GameWorld>.Instance;

                if (Globals.GameWorld != null && Globals.GameWorld.RegisteredPlayers != null)
                {
                    List<IPlayer> RegisteredPlayers = Globals.GameWorld.RegisteredPlayers;

                    Globals.Players.Clear();

                    foreach (var Player in RegisteredPlayers)
                    {
                        if (Player == null)
                            continue;
                       

                        if (Player.IsYourPlayer)
                            Globals.LocalPlayer = Player as Player;

                        Globals.Players.Add(Player);
                    }
                }
                else
                {
                    Globals.Players.Clear();
                }

                LastCacheTime = Time.time + 0.25f;
            }

            if (Globals.MainCamera != null)
            {
                Globals.MainCamera.GetComponent<NightVision>().SetPrivateField("_on", MenuVars.ForceNightVision);

                Globals.MainCamera.GetComponent<ThermalVision>().On = MenuVars.ForceThermalVision;

            }
        }

        private void HandleClientCommunication(TcpClient client)
        {
            try
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];

                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        // Client disconnected
                        break;
                    }

                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received from client: {receivedMessage}");

                    // Process received message (example: update menu variables)
                    // Example: Assuming receivedMessage is in JSON format
                     JObject jsonObject = JObject.Parse(receivedMessage);
                     MenuVars.ForceThermalVision = (int)jsonObject["ftv"] != 0;
                    // Optionally, send a response back to the client
                    // string responseMessage = "Message received!";
                    // byte[] responseData = Encoding.UTF8.GetBytes(responseMessage);
                    // stream.Write(responseData, 0, responseData.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client communication: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }


        private void Awake()
        {
            DontDestroyOnLoad(this);
        }


        void novisor()
        {
            var camera = Globals.MainCamera;
            if (camera == null)
                return;
            var component = camera.GetComponent<VisorEffect>();
            if (component == null || Mathf.Abs(component.Intensity - Convert.ToInt32(!true)) < Mathf.Epsilon)
                return;

            component.Intensity = Convert.ToInt32(!true);
        }



        void crosshair()
        {
            var centerx = Screen.width / 2;
            var centery = Screen.height / 2;
            var texture = Texture2D.whiteTexture;
            GUI.DrawTexture(new Rect(centerx - 5, centery, 5 * 2 + 2, 2), texture);
            GUI.DrawTexture(new Rect(centerx, centery - 5, 2, 5 * 2 + 2), texture);
        }

        private void Chams()
        {
            if (Globals.GameWorld == null || Globals.LocalPlayer == null || Globals.MainCamera == null || Globals.Players.IsNullOrEmpty())
                return;

            for (int i = 0; i < Globals.Players.Count(); i++)
            {
                IPlayer _Player = Globals.Players.ElementAt(i);

                if (_Player == null)
                    continue;

                // FOR ONLINE RAIDS
                
                if (_Player.GetType() != typeof(ObservedPlayerView))
                    continue;
                
                ObservedPlayerView Player = _Player as ObservedPlayerView;
                if (Player == null)
                    continue;
                
                // FOR ONLINE RAIDS

                Vector3 HeadPos = Globals.W2S(Player.PlayerBones.Head.position);

                if (HeadPos == Vector3.zero)
                    continue;


                bool IsScav = false;
                if (Player.ObservedPlayerController != null && Player.ObservedPlayerController.InfoContainer != null)
                    IsScav = Player.ObservedPlayerController.InfoContainer.Side == EPlayerSide.Savage;

                // Not the best way, see if you (the reader) can improve this.
                bool IsBoss = Globals.IsBossByName(Player.NickName.Localized());

                if (IsBoss)
                {
                    var Skins = Player.PlayerBody.BodySkins.Values;
                    foreach (var Skin in Skins)
                    {
                        foreach (var Renderer in Skin.GetRenderers())
                        {
                            var Material = Renderer.material;

                            Material.shader = Globals.Custom.LoadAsset<Shader>("chams.shader");

                            Material.SetColor("_ColorVisible", new Color(1f, 1f, 0f, 1f));
                            Material.SetColor("_ColorBehind", new Color(1f, 0f, 0f, 1f));
                        }
                    }
                }
                else if (IsScav)
                {
                    var Skins = Player.PlayerBody.BodySkins.Values;
                    foreach (var Skin in Skins)
                    {
                        foreach (var Renderer in Skin.GetRenderers())
                        {
                            var Material = Renderer.material;

                            Material.shader = Globals.Custom.LoadAsset<Shader>("chams.shader");

                            Material.SetColor("_ColorVisible", new Color(1f, 1f, 1f, 1f));
                            Material.SetColor("_ColorBehind", new Color(0f, 0f, 1f, 1f));
                        }
                    }
                }
                else
                {
                    var Skins = Player.PlayerBody.BodySkins.Values;
                    foreach (var Skin in Skins)
                    {
                        foreach (var Renderer in Skin.GetRenderers())
                        {
                            var Material = Renderer.material;

                            Material.shader = Globals.Custom.LoadAsset<Shader>("chams.shader");

                            Material.SetColor("_ColorVisible", new Color(1f, 0f, 1f, 1f));
                            Material.SetColor("_ColorBehind", new Color(0.568f, 0f, 1f, 1f));
                        }
                    }
                }
            }
            }
    }
}
