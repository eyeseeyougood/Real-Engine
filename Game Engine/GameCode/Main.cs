using System;
using System.Linq;
using System.Numerics;
using engine;
using Game_Engine;
using Game_Engine.Shaders;
using SDL2;

Booter booter = new Booter();
CollisionManager collisionManager = new CollisionManager();

booter.SetBGColour(new Vector3(50, 50, 50));

string workingDirectory = Environment.CurrentDirectory;

string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

Image PepsiCanUI = new Image(projectDirectory + @"\GameData\PepsiCan.png");
PepsiCanUI.Position = new Vector2(4, 4);
PepsiCanUI.Scale = new Vector2(1, 1);

Text PepsiCounterText = new Text("100", Engine.White);
PepsiCounterText.Position = new Vector2(25, 4);
PepsiCounterText.Scale = new Vector2(15, 40);

float MoveSpeed = 0.05f;

void Tick(List<string> currentKeys)
{
    collisionManager.ColliderTick(booter);

    PepsiCounterText.text = PepsiManager.PepsiCounter.ToString();

    if (currentKeys.Count != 0)
    {
        if (currentKeys.Contains("W"))
        {
            Player.Movement(new Vector2(0, 1) * MoveSpeed);
        }
        if (currentKeys.Contains("S"))
        {
            Player.Movement(new Vector2(0, -1) * MoveSpeed);
        }
        if (currentKeys.Contains("A"))
        {
            Player.Movement(new Vector2(-1, 0) * MoveSpeed);
            Player.SetSprite(0);
        }
        if (currentKeys.Contains("D"))
        {
            Player.Movement(new Vector2(1, 0) * MoveSpeed);
            Player.SetSprite(1);
        }
    }    
}

void OnObjectLoaded(GameObject obj)
{
    if (obj.Tags.Contains("PepsiCan"))
    {
        Collider col = (Collider)obj.GetComponent("SquareCollider");
        if (col != null)
            PepsiManager.PepsiCansList.Add(col);
    }
    if (obj.Tags.Contains("Player"))
    {
        obj.WorldPosition += new Vector2(0.01f, 0);
    }
}

void OnSetup() // same as start
{
    Console.WriteLine("setup ran");
    // Gravity
    //Physics.Gravity gravity = new Physics.Gravity("Gravity", Player.player(), booter);
    //gravity.Strength = 1f;
    //Player.player().Components.Add(gravity);
    //Console.WriteLine(Player.player().Components[1]);

    // setup player sprites
    foreach (string path in EngineManager.booterInstance.instance.PathToIndex.Keys)
    {
        if (path.Contains("Player_"))
            Player.sprites.Add(path);
    }

    //ShaderManager.Test(booter.GameTextures[booter.instance.PathToIndex[projectDirectory + @"\GameData\Red.png"]]);
}

// bind events
booter.Tick += Tick;
booter.onSetup += OnSetup;
GameObject.onLoaded += OnObjectLoaded;

// add UIElements
booter.UIElements.Add(PepsiCounterText);
booter.UIElements.Add(PepsiCanUI);

// Load Scene
SceneManager.SetupScenes(projectDirectory + @"\GameData\Scenes");
SceneManager.LoadScene(SceneManager.SceneID + 1);

booter.Setup(new Vector2(1007, 100), new Vector2(1920/2, 1080/2), 2);