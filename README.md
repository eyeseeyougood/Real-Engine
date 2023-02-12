# Real-Engine

### Basic setup
In the main file, create a new Booter instance and CollisionManager instance
these ared use to handle UI, GameObjects, and Colliders.

the way the engine is made allows you either create objects manually in the main file,
or automatically using scenes in the SceneManager.

So with this in mind the way to manually create GameObjects
is to create a new instance of the GameObject class and
set it's variables. To enable the game to use this object and render it,
it MUST be added to the booter's 'Objects' list.

To start the engine the booter's Setup() function must be ran. This must be the last line in the Main file.

### How to make a tick function:
Make a function that returns void (can be public or private)
after the function but before running Setup() you need to subscribe the function to the tick event of the booter

## Classes

### Engine
#### Variables:
Layout : -[name] - [type] - [what it is / what it's for] - [when & how it is set]

-WindowPos - Vector2 - the position of the window - Set automatically by 'Booter' Class on Setup()
 
-WindowSize - Vector2 - the size of the window - Set automatically by 'Booter' Class on Setup()

-Running - bool - determines if the game loop should keep running - Set automatically by 'Booter' Class on Setup()

-Window - IntPtr - the window pointer - Set automatically by 'Booter' Class on Setup()
 
-Renderer - IntPtr - the renderer pointer - Set automatically by 'Booter' Class on Setup()

-BGColour - Vector3 - the position of the Window - Set automatically by 'Booter' Class on Setup()

-booter - Booter - the Booter instance - Set automatically by 'Booter' Class on Setup()

-White - Vector4 - A default colour - Set when initialised
 
-Red - Vector4 - A default colour - Set when initialised
 
-Blue - Vector4 - A default colour - Set when initialised

-PathToIndex - Dictionary<string, int> - A lookup table for the index of a texture based off of it's path - Set automatically by 'MapLoader' Class on Load()

#### Has no functions!

### Booter
#### Variables:
Layout : -[name] - [type] - [what it is / what it's for] - [when & how it is set]

-Project Directory - string - for easy access of the projects directory - Set automatically when an instance of the class is made

-DebugRects - List<KeyValuePair<Vector4, Vector4>> - A list of rectangles and their colours to allow for easier debugging - Set automatically by collider tick

-Objects - List<GameObject> - A list of all the scenes current objects - Set manually / automatically by MapLoader

-UIElements - List<UIElement> - A list of all the UI elements (not scene based) - Set manually

-Instance - Engine - The current instance of the 'Engine' class - Set automatically by 'Booter' Class on Setup()

-CurrentKeys - List<string> - A list of all current pressed keys (Obsolete) - Set automatically by 'GameLoop' Class on Start()

-TTF_Font - IntPtr - A Pointer to the current font being used - Set automatically by 'Booter' Class on Setup()

-Points - Dictionary<Vector2, Vector4> - A list of points and their colours to allow for easier debugging - Set manually

-GameTextures - List<IntPtr> - A list of all the pointers to the loaded game textures - Set automatically by 'Booter' Class on LoadGameTextures()

-Camera - Camera - The current instance of the 'Camera' class - Set automatically by 'Booter' Class on Setup()

-CameraPosQueue - Vector2 - The start position of the 'Camera' class (gets set before setup is ran so it's a queue) - Set automatically by 'MapLoader' on Load()

#### Functions:
  Layout : -[name+args] - [Return Value] - [explanation]
  
  -DrawPoint(int x, int y, Vector4 colour) - Void - used to render a colour at a single pixel
  
  -LoadGameTextures() - Void - Loads all the game textures in (the png files stored in \GameData)
  
  -DrawDebugRect(KeyValuePair<Vector4, Vector4> debugRect) - Void - Draws a rectangle of the specified colour at the specified position and scale
  
  -Setup(Vector2 windowPos, Vector2 windowSize, int cameraSize) - Void - sets up alot of the booter variables and starts the game loop
  
  -SetBGColour(Vector3 colour) - Void - Sets background colour of the window
  
  -SetTick(List<string> keys) - Void - Invokes 'Tick' Event of 'Booter' Class
  
### GameLoop
  #### Variables:
  Layout : -[name] - [type] - [what it is / what it's for] - [when & how it is set]
  
  -Objects - List<GameObject> - All of the current scene objects - Set automatically by 'Booter' Class on Setup()
  
  -UIElements - List<UIElement> - All of the current UIElements - Set automatically by 'Booter' Class on Setup()
  
  -DebugRects - List<KeyValuePair<Vector4, Vector4>> - All of the scene debug rects - Set automatically by 'Booter' Class on Setup()
  
  -GameTextures - List<IntPtr> - All of the loaded game textures - Set automatically by 'Booter' Class on Setup()
  
  #### Functions:
  Layout : -[name+args] - [Return Value] - [explanation]
  
  -Start(Engine instance, List<GameObject> _Objects, List<UIElement> _UIelems, List<KeyValuePair<Vector4, Vector4>> _DebugRects, List<IntPtr> _gameTextures) - List<List<IntPtr>> - Starts the game loop which handles rendering objects, UI, debug rects, points and returns the IntPtrs that point to surfaces and textures to clean up (clean up done at the end of 'Booter'->'Setup()')

 
## How to set triggers in maps -- out dated (WON'T WORK WITH ARGUMENTS)
 
 To set triggers, you need to make a separate cs file with a class for the object (with the exact name of the object in the map) with the function to run when the trigger starts (this can contain as many and whatever arguments you want as an object array). Inside the object's properties file the exact function name must be specified, and the new Class' name must be the exact same name as the Object
An example:
 
 ![1](https://www.webpagescreenshot.info/image-url/1u708W5gL)
 
 ![2](https://www.webpagescreenshot.info/image-url/bUcj_96sC)
 
 ![3](https://www.webpagescreenshot.info/image-url/fp_d5PORa)
