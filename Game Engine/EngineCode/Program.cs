// 2D Game Engine

// imports
using SDL2;
using System;
using System.Data.SqlTypes;
using System.Linq;
using System.Numerics;
using System.IO;
using static SDL2.SDL;
using System.ComponentModel;
using System.Drawing;
using Game_Engine;

// main engine code

namespace engine // This whole thing has a massive memory leak somewhere!
{
    public enum Space
    {
        WorldSpace = 1,
        ScreenSpace = 2,
        WVS = 3, // World values, Screen Format (WVS Format)
        SVW = 4 // Screen values, World Format (SVW Format)
    }

    public delegate void GameTick(List<string> keys);
    public delegate void Destroy();
    public delegate void OnLoaded(GameObject obj);
    public delegate void OnSetup();

    public class Engine
    {
        // engine variables
        public Vector2 WindowPos;
        public Vector2 WindowSize;

        public bool Running;

        public IntPtr Window;
        public IntPtr Renderer;

        public Vector3 BGColour;
        public Booter booter;

        public static Vector4 White = new Vector4(255, 255, 255, 255);
        public static Vector4 Red = new Vector4(255, 0, 0, 255);
        public static Vector4 Blue = new Vector4(0, 0, 255, 255);

        public Dictionary<string, int> PathToIndex = new Dictionary<string, int>();
    }
    public class Booter
    {
        public string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        public List<KeyValuePair<Vector4, Vector4>> DebugRects = new List<KeyValuePair<Vector4, Vector4>>();
        public List<GameObject> Objects = new List<GameObject>();
        public List<UIElement> UIElements = new List<UIElement>();
        public Engine instance;
        Vector3 backGround = Vector3.Zero;
        public event GameTick Tick;
        public event OnSetup onSetup;
        public List<string> currentKeys = new List<string>();
        public IntPtr TTF_Font;
        public Dictionary<Vector2, Vector4> points = new Dictionary<Vector2, Vector4>();
        public List<IntPtr> GameTextures = new List<IntPtr>();
        public Camera camera;
        public Vector2 CameraPosQueue;

        public Booter()
        {
            EngineManager.booterInstance = this;
        }
        public void DrawPoint(int x, int y, Vector4 colour)
        {
            SDL.SDL_SetRenderDrawColor(instance.Renderer, (byte)colour.X, (byte)colour.Y, (byte)colour.Z, (byte)colour.W);
            SDL.SDL_RenderDrawPoint(instance.Renderer, x, y);
        }

        public void LoadGameTextures()
        {
            List<string> Paths = Directory.GetFiles(projectDirectory + @"\GameData\").ToList();
            List<string> Images = new List<string>();
            foreach (string Path in Paths)
            {
                if (Path.Contains(".png")) { Images.Add(Path); }
            }

            int index = 0;
            foreach (string Path in Images)
            {
                IntPtr image = SDL_image.IMG_LoadTexture(instance.Renderer, Path);
                /*
                unsafe
                {
                    SDL.SDL_Texture* surface_surf = (SDL.SDL_Surface*)image;
                    surface_surf->;
                }
                */
                GameTextures.Add(image);
                instance.PathToIndex.Add(Path, index);
                index++;
            }
        }

        public void DrawDebugRect(KeyValuePair<Vector4, Vector4> debugRect, Space space)
        {
            byte r = (byte)debugRect.Value.X;
            byte g = (byte)debugRect.Value.Y;
            byte b = (byte)debugRect.Value.Z;
            byte a = (byte)debugRect.Value.W;

            SDL_Rect Rect = new SDL.SDL_Rect();
            Rect.x = (int)GameObject.WorldToScreenSpace(EngineManager.booterInstance, new Vector2(debugRect.Key.X, debugRect.Key.Y)).X;
            Rect.y = (int)GameObject.WorldToScreenSpace(EngineManager.booterInstance, new Vector2(debugRect.Key.X, debugRect.Key.Y)).Y;
            if (space == Space.ScreenSpace)
            {
                Rect.w = (int)debugRect.Key.Z;
                Rect.h = (int)debugRect.Key.W;
            }
            else if (space == Space.WorldSpace)
            {
                Vector2 type2 = new Vector2(debugRect.Key.X + debugRect.Key.Z, debugRect.Key.Y + debugRect.Key.W); // key position MUST be in format WVW
                Vector2 type2SVS = GameObject.WorldToScreenSpace(EngineManager.booterInstance, type2);

                int PixelWidth = (int)(type2SVS.X - Rect.x);
                int PixelHeight = (int)(type2SVS.Y - Rect.y);

                Rect.w = PixelWidth;
                Rect.h = PixelHeight;
            }

            SDL.SDL_SetRenderDrawColor(instance.Renderer, r, g, b, a);
            SDL.SDL_RenderDrawRect(instance.Renderer, ref Rect);
        }

        public void Setup(Vector2 windowPos, Vector2 windowSize, int cameraSize)
        {
            instance = new Engine();

            instance.WindowPos = windowPos;
            instance.WindowSize = windowSize;

            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);

            var window = SDL.SDL_CreateWindow("Real Engine",
                (int)instance.WindowPos.X,
                (int)instance.WindowPos.Y,
                (int)instance.WindowSize.X,
                (int)instance.WindowSize.Y,
                SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN);

            var Renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

            
            IntPtr IconSurface = SDL_image.IMG_Load(projectDirectory + @"\EngineData\ICON.png");

            SDL.SDL_SetWindowIcon(window, IconSurface);
            

            SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG);

            SDL_ttf.TTF_Init();
            TTF_Font = SDL_ttf.TTF_OpenFont(@"C:\WINDOWS\FONTS\COMIC.ttf", 24);

            instance.Running = true;
            instance.Window = window;
            instance.Renderer = Renderer;
            instance.BGColour = backGround;
            instance.booter = this;

            LoadGameTextures();

            camera = new Camera();
            camera.CameraSize = cameraSize;
            camera.Activate();
            Tick += camera.CameraTick;

            GameLoop gameLoop = new GameLoop();

            onSetup.Invoke();

            List<List<IntPtr>> Ptrs = gameLoop.Start(instance, Objects, UIElements, DebugRects, GameTextures);
            Ptrs[0].ForEach(i => SDL.SDL_DestroyTexture(i));
            Ptrs[1].ForEach(i => SDL.SDL_FreeSurface(i));

            SDL.SDL_DestroyRenderer(instance.Renderer);
            SDL.SDL_DestroyWindow(instance.Window);
            SDL.SDL_Quit();
        }

        public void SetBGColour(Vector3 colour)
        {
            backGround = colour;
        }

        public void SetTick(List<string> keys)
        {
            Tick.Invoke(keys);
        }
    }

    class GameLoop
    {
        public List<GameObject> Objects;
        public List<UIElement> UIElements;
        public List<KeyValuePair<Vector4, Vector4>> DebugRects;
        public List<IntPtr> GameTextures;
        public List<List<IntPtr>> Start(Engine instance, List<GameObject> _Objects, List<UIElement> _UIelems, List<KeyValuePair<Vector4, Vector4>> _DebugRects, List<IntPtr> _gameTextures)
        {
            Objects = _Objects;
            UIElements = _UIelems;
            DebugRects = _DebugRects;
            GameTextures = _gameTextures;

            List<IntPtr> TexturePtrs = new List<IntPtr>();
            List<IntPtr> SurfacePtrs = new List<IntPtr>();
            List<List<IntPtr>> PtrsList = new List<List<IntPtr>>();
            PtrsList.Add(TexturePtrs);
            PtrsList.Add(SurfacePtrs);

            while (instance.Running)
            {
                while (SDL.SDL_PollEvent(out SDL.SDL_Event e) == 1)
                {
                    //if (instance.booter.CurrentEvent.type == SDL.SDL_EventType.SDL_KEYDOWN)
                        //Console.WriteLine(SDL.SDL_GetKeyName(instance.booter.CurrentEvent.key.keysym.sym));
                    switch (e.type)
                    {
                        case SDL.SDL_EventType.SDL_QUIT:
                            instance.Running = false;
                            break;
                        case SDL.SDL_EventType.SDL_KEYDOWN:
                            if (!instance.booter.currentKeys.Contains( SDL.SDL_GetKeyName(e.key.keysym.sym)))
                                instance.booter.currentKeys.Add(SDL.SDL_GetKeyName(e.key.keysym.sym));
                            break;
                        case SDL.SDL_EventType.SDL_KEYUP:
                            if (instance.booter.currentKeys.Contains(SDL.SDL_GetKeyName(e.key.keysym.sym)))
                                instance.booter.currentKeys.Remove(SDL.SDL_GetKeyName(e.key.keysym.sym));
                            break;
                    }
                }

                // Calc DeltaTime
                Time.time2 = DateTime.Now; // set time (used to calc delta time)
                Time.DeltaTime = (Time.time2.Ticks - Time.time1.Ticks) / 10000000f; // calculating delta time

                if (Time.currFrame > Time.Frames.Length - 1) { Time.currFrame = 0; }
                Time.Frames[Time.currFrame] = Time.DeltaTime;

                // update delta time
                Time.time1 = Time.time2;

                SDL.SDL_SetRenderDrawColor(instance.Renderer,
                    (byte)instance.BGColour.X,
                    (byte)instance.BGColour.Y,
                    (byte)instance.BGColour.Z,
                    255); // clear screen colour
                SDL.SDL_RenderClear(instance.Renderer); // clear screen

                SDL.SDL_SetRenderDrawColor(instance.Renderer,
                    (byte)200,
                    (byte)200,
                    (byte)200,
                    255); // clear screen colour

                foreach (GameObject i in Objects)
                {
                    SpriteRenderer spriteRenderer = (SpriteRenderer)i.GetComponent("SpriteRenderer");

                    if (spriteRenderer == null)
                        continue; // move on - No SpriteRenderer on this GameObject

                    if (spriteRenderer.TexturePath == "Rect")
                    {
                        Vector2 ScreenSpaceCoords = GameObject.WorldToScreenSpace(instance.booter, i.WorldPosition);
                        SDL.SDL_Rect src = new SDL.SDL_Rect();
                        src.x = (int)ScreenSpaceCoords.X;
                        src.y = (int)ScreenSpaceCoords.Y;
                        src.w = (int)i.Scale.X;
                        src.h = (int)i.Scale.Y;
                        byte R = (byte)spriteRenderer.TextureColour.X;
                        byte G = (byte)spriteRenderer.TextureColour.Y;
                        byte B = (byte)spriteRenderer.TextureColour.Z;
                        byte A = (byte)spriteRenderer.TextureColour.W;
                        SDL.SDL_SetRenderDrawColor(instance.Renderer, R, G, B, A);
                        SDL.SDL_RenderDrawRect(instance.Renderer, ref src);
                    }
                    else if (spriteRenderer.TexturePath == "FilledRect")
                    {
                        Vector2 ScreenSpaceCoords = GameObject.WorldToScreenSpace(instance.booter, i.WorldPosition);
                        SDL.SDL_Rect src = new SDL.SDL_Rect();
                        src.x = (int)ScreenSpaceCoords.X;
                        src.y = (int)ScreenSpaceCoords.Y;
                        src.w = (int)i.Scale.X;
                        src.h = (int)i.Scale.Y;
                        byte R = (byte)spriteRenderer.TextureColour.X;
                        byte G = (byte)spriteRenderer.TextureColour.Y;
                        byte B = (byte)spriteRenderer.TextureColour.Z;
                        byte A = (byte)spriteRenderer.TextureColour.W;
                        SDL.SDL_SetRenderDrawColor(instance.Renderer, R, G, B, A);
                        SDL.SDL_RenderFillRect(instance.Renderer, ref src); 
                    }
                    else
                    {
                        IntPtr image = GameTextures[instance.PathToIndex[spriteRenderer.TexturePath]];
                        //TexturePtrs.Add(image);
                        int W;
                        int H;
                        uint nullUInt;
                        int nullInt;
                        Vector2 ScreenSpaceCoords = GameObject.WorldToScreenSpace(instance.booter, i.WorldPosition);
                        SDL_QueryTexture(image, out nullUInt, out nullInt, out W, out H);
                        SDL.SDL_Rect src = new SDL.SDL_Rect();
                        src.x = (int)ScreenSpaceCoords.X;
                        src.y = (int)ScreenSpaceCoords.Y;
                        src.w = W * (int)i.Scale.X * instance.booter.camera.CameraSize;
                        src.h = H * (int)i.Scale.Y * instance.booter.camera.CameraSize;
                        spriteRenderer.TextureRes = new Vector2(W, H);

                        SDL_RendererFlip flip = SDL_RendererFlip.SDL_FLIP_NONE;

                        if (spriteRenderer.FlipX && !spriteRenderer.FlipY)
                        {
                            flip = SDL_RendererFlip.SDL_FLIP_HORIZONTAL;
                        }
                        else if (!spriteRenderer.FlipX && spriteRenderer.FlipY)
                        {
                            flip = SDL_RendererFlip.SDL_FLIP_VERTICAL;
                        }
                        else if (spriteRenderer.FlipX && spriteRenderer.FlipY)
                        {
                            flip = SDL_RendererFlip.SDL_FLIP_HORIZONTAL | SDL_RendererFlip.SDL_FLIP_VERTICAL;
                        }

                        SDL.SDL_RenderCopyEx(instance.Renderer, image, IntPtr.Zero, ref src, i.Rotation, IntPtr.Zero, flip);
                    }
                }

                foreach (UIElement i in UIElements)
                {
                    if (i.GetType() == typeof(Text))
                    {
                        SDL_Color TextColour = new SDL_Color();
                        TextColour.r = (byte)((Text)i).textColour.X;
                        TextColour.g = (byte)((Text)i).textColour.Y;
                        TextColour.b = (byte)((Text)i).textColour.Z;
                        TextColour.a = (byte)((Text)i).textColour.W;

                        IntPtr surfaceMessage = SDL_ttf.TTF_RenderText_Solid(instance.booter.TTF_Font, ((Text)i).text, TextColour);
                        IntPtr Message = SDL.SDL_CreateTextureFromSurface(instance.Renderer, surfaceMessage);

                        int W;
                        int H;
                        uint nullUInt;
                        int nullInt;
                        SDL_QueryTexture(Message, out nullUInt, out nullInt, out W, out H);

                        SDL_Rect Message_rect;
                        Message_rect.x = (int)i.Position.X;
                        Message_rect.y = (int)i.Position.Y;
                        Message_rect.w = W;
                        Message_rect.h = H;

                        SDL_FreeSurface(surfaceMessage);

                        SDL_RenderCopy(instance.Renderer, Message, IntPtr.Zero, ref Message_rect);

                        SDL_DestroyTexture(Message);
                    }
                    else if (i.GetType() == typeof(Image))
                    {
                        IntPtr image = GameTextures[instance.PathToIndex[((Image)i).ImagePath]];
                        TexturePtrs.Add(image);
                        int W;
                        int H;
                        uint nullUInt;
                        int nullInt;
                        SDL_QueryTexture(image, out nullUInt, out nullInt, out W, out H);
                        SDL.SDL_Rect src = new SDL.SDL_Rect();
                        src.x = (int)(i.Position).X;
                        src.y = (int)(i.Position).Y;
                        src.w = W * (int)i.Scale.X * instance.booter.camera.CameraSize;
                        src.h = H * (int)i.Scale.Y * instance.booter.camera.CameraSize;
                        ((Image)i).TextureRes = new Vector2(W, H);
                        SDL.SDL_RenderCopyEx(instance.Renderer, image, IntPtr.Zero, ref src, ((Image)i).Rotation, IntPtr.Zero, SDL.SDL_RendererFlip.SDL_FLIP_NONE);
                    }
                }

                foreach (KeyValuePair<Vector2, Vector4> i in instance.booter.points)
                {
                    //SDL.SDL_SetRenderDrawColor(instance.Renderer, (byte)i.Value.X, (byte)i.Value.Y, (byte)i.Value.Z, (byte)i.Value.W);
                    //SDL.SDL_RenderDrawPoint(instance.Renderer, i.Key.X, i.Key.Y);
                    instance.booter.DrawPoint((int)i.Key.X, (int)i.Key.Y, i.Value);
                }

                foreach (KeyValuePair<Vector4, Vector4> i in DebugRects)
                {
                    instance.booter.DrawDebugRect(i, Space.WorldSpace);
                }

                instance.booter.SetTick(instance.booter.currentKeys);

                SDL.SDL_RenderPresent(instance.Renderer); // switch buffer
            }

            return PtrsList;
        }
    }

    public static class Time
    {
        public static float DeltaTime;
        public static float[] Frames = new float[10000];
        public static int currFrame = 0;
        // Delta time
        public static DateTime time1 = DateTime.Now;
        public static DateTime time2 = DateTime.Now;
    }

    public static class EngineManager
    {
        public static Booter booterInstance;
        public static CollisionManager colmanInstance;
    }

    public class Camera
    {
        public int WorldSpaceMultiplier;
        public Vector2 CameraPosition;
        public Vector2 CameraWorldPosition;
        public int CameraSize;
        public bool Active { get; private set; }

        public void Activate()
        {
            Active = true;
            WorldSpaceMultiplier = 16 * CameraSize;
        }

        public void CameraTick(List<string> keys)
        {
            WorldSpaceMultiplier = 16 * CameraSize;
        }
    }

    public class GameObject : IRaycastable
    {
        public string Name;
        public string ObjName;
        public List<string> Tags;
        public Vector2 WorldPosition;
        public Vector2 Scale;
        public double Rotation;
        public event Destroy destroy;
        public static event OnLoaded onLoaded;
        public List<Component> Components = new List<Component>();

        public GameObject()
        {
            Name = "defObject";
            Tags = new List<string>();
        }

        public GameObject(List<string> tags)
        {
            Name = "defObject";
            Tags = tags;
        }

        public GameObject(List<string> tags, string objName)
        {
            Name = "defObject";
            Tags = tags;
            ObjName = objName;
        }

        public GameObject(string objName)
        {
            Name = "defObject";
            Tags = new List<string>();
            ObjName = objName;
        }

        public static void Loaded(GameObject obj)
        {
            onLoaded.Invoke(obj);
        }

        public void Destroy()
        {
            destroy.Invoke();
            EngineManager.booterInstance.Objects.Remove(this);
        }

        public static Vector2 WorldToScreenSpace(Booter _instance, Vector2 WorldSpaceCoords)
        {
            if (!_instance.camera.Active)
                return Vector2.Zero;
            Vector2 ScreenSpaceCoords = new Vector2(WorldSpaceCoords.X - _instance.camera.CameraPosition.X, -WorldSpaceCoords.Y - -_instance.camera.CameraPosition.Y) * _instance.camera.WorldSpaceMultiplier + _instance.instance.WindowSize / 2;
            //ScreenSpaceCoords -= new Vector2( _instance.camera.CameraPosition.X, -_instance.camera.CameraPosition.Y);
            return ScreenSpaceCoords;
        }

        public static Vector2 ScreenToWorldSpace(Booter _instance, Vector2 ScreenSpaceCoords)
        {
            if (!_instance.camera.Active)
                return Vector2.Zero;
            Vector2 WorldSpaceCoords = ScreenSpaceCoords - _instance.instance.WindowSize / 2;
            WorldSpaceCoords = WorldSpaceCoords / _instance.camera.WorldSpaceMultiplier;
            WorldSpaceCoords += new Vector2(_instance.camera.CameraPosition.X, -_instance.camera.CameraPosition.Y);
            WorldSpaceCoords = new Vector2(WorldSpaceCoords.X, -WorldSpaceCoords.Y);
            return WorldSpaceCoords;
        }

        public static int MoveWithCollision(GameObject _obj, Vector2 _moveAmount, Booter booter, CollisionManager colMan) // clean up later by storing important objects such as booter and collision manager in one class
        {
            SpriteRenderer spriteRenderer = (SpriteRenderer)_obj.GetComponent("SpriteRenderer");

            if (spriteRenderer == null)
                return 1; // fail - No SpriteRenderer on specified GameObject

            Vector2 NextPos = _obj.WorldPosition + _moveAmount;
            Vector2 ScreenNextPos = WorldToScreenSpace(booter, NextPos);
            Vector2 MaxWorldSpacePos = ScreenToWorldSpace(booter, new Vector2(ScreenNextPos.X + _obj.Scale.X * spriteRenderer.TextureRes.X, ScreenNextPos.Y + _obj.Scale.Y * spriteRenderer.TextureRes.Y));
            Vector4 NextCollisionRect = new Vector4(NextPos.X, MaxWorldSpacePos.Y, MaxWorldSpacePos.X, NextPos.Y);
            Collider col = (Collider)_obj.GetComponent("SquareCollider");
            if (colMan.CollideRect(NextCollisionRect, col) == null)
                _obj.WorldPosition = NextPos;
            return 0; // successfull
        }

        public Component GetComponent(string _type)
        {
            Component comp = null;

            foreach (Component i in Components)
            {
                if (i.Name == _type)
                {
                    comp = i;
                    break;
                }
            }

            return comp;
        }

        public Component GetComponent(Type _type)
        {
            Component comp = null;

            foreach (Component i in Components)
            {
                if (i.GetType() == _type)
                {
                    comp = i;
                    break;
                }
            }

            return comp;
        }
    }

    public class SpriteRenderer : Component
    {
        public string TexturePath;
        public Vector2 TextureRes;
        public Vector4 TextureColour;
        public bool FlipX;
        public bool FlipY;

        public SpriteRenderer(GameObject bindingObj, string path) : base("SpriteRenderer", bindingObj)
        {
            TexturePath = path;
            TextureColour = Engine.White;
            FlipX = false;
            FlipY = false;
        }

        public SpriteRenderer(GameObject bindingObj, string path, Vector4 colour) : base("SpriteRenderer", bindingObj)
        {
            TexturePath = path;
            TextureColour = colour;
            FlipX = false;
            FlipY = false;
        }

        public SpriteRenderer(GameObject bindingObj, string path, Vector4 colour, bool flipX, bool flipY) : base("SpriteRenderer", bindingObj)
        {
            TexturePath = path;
            TextureColour = colour;
            FlipX = flipX;
            FlipY = flipY;
        }

        public SpriteRenderer(GameObject bindingObj, string path, bool flipX, bool flipY) : base("SpriteRenderer", bindingObj)
        {
            TexturePath = path;
            TextureColour = Engine.White;
            FlipX = flipX;
            FlipY = flipY;
        }
    }

    public class UIElement
    {
        public string Name;
        public Vector2 Position;
        public Vector2 Scale;
    }

    public class Text : UIElement
    {
        public string text;
        public Vector4 textColour;
        public Text(string _text, Vector4 _textColour)
        {
            Name = "Text";
            text = _text;
            textColour = _textColour;
        }
    }

    public class Image : UIElement
    {
        public string ImagePath;
        public Vector2 TextureRes;
        public double Rotation;

        public Image(string Path)
        {
            Name = "Image";
            ImagePath = Path;
        }
    }

    public class Component
    {
        public string Name;
        public GameObject BoundObject;

        public Component(string name, GameObject bindingObject)
        {
            Name = name;
            BoundObject = bindingObject;
            bindingObject.Components.Add(this);
        }
    }
}