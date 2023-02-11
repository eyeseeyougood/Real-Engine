using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using engine;
using System.IO;

namespace Game_Engine
{
    public class MapLoader
    {
        public Dictionary<Vector4, string> Key = new Dictionary<Vector4, string>(); // pixel colour to object name
        public Dictionary<string, string> TextureMAP = new Dictionary<string, string>(); // object name to texture
        public Dictionary<string, string> TagMAP = new Dictionary<string, string>(); // object name to tag

        public void Load(string ProjPath, string SceneName, int layer )
        {
            // clear key and texture map and tag map
            Key.Clear();
            TextureMAP.Clear();
            TagMAP.Clear();

            // get mains
            Booter booter = EngineManager.booterInstance;
            CollisionManager colMan = EngineManager.colmanInstance;

            // fix paths
            string root_path = ProjPath;
            string GameData_Path = "";

            string[] Sliced = ProjPath.Split(@"\");
            int val = Sliced.Length - 1;
            int SpecialVAL = Sliced.Length - 2;
            string end = Sliced[0];
            string SpecialCASE = Sliced[0];
            for (int i = 0; i < val; i++)
            {
                if (i == 0)
                    continue;
                end += @"\" + Sliced[i];
            }
            ProjPath = end;
            for (int i = 0; i < SpecialVAL; i++)
            {
                if (i == 0)
                    continue;
                SpecialCASE += @"\" + Sliced[i];
            }
            GameData_Path = SpecialCASE;
            // Decode Key
            string[] KeyLines = File.ReadAllLines(ProjPath + @"\ExternalInfo\Key.txt");
            foreach (string line in KeyLines)
            {
                int g = 0;
                foreach (char i in line)
                {
                    if (i == "=".ToCharArray()[0])
                        break;
                    g++;
                }
                string keystring = line.Substring(0, g);
                string value = line.Substring(g + 1);

                // find colour value
                int h = 0;
                foreach (char i in value)
                {
                    if (i == ',') { break; }
                    h++;
                }
                int j = 0;
                foreach (char i in value.Substring(h + 1))
                {
                    if (i == ',') { break; }
                    j++;
                }
                int k = 0;
                foreach (char i in value.Substring(h + j + 2))
                {
                    if (i == ',') { break; }
                    k++;
                }

                Vector4 Colour = new Vector4();
                Colour.X = int.Parse(value.Substring(0, h));
                Colour.Y = int.Parse(value.Substring(h + 1, j));
                Colour.Z = int.Parse(value.Substring(h + j + 2, k));
                Colour.W = int.Parse(value.Substring(h + j + k + 3));

                Key.Add(Colour, keystring);
            }

            // Load Settings
            string[] settings = File.ReadAllLines(ProjPath + @"\ExternalInfo\Settings.txt"); // all settings in array
            int u = 0; // camera start position
            string sub = settings[0].Substring(10);
            foreach (char i in sub)
            {
                if (i == ",".ToCharArray()[0]) { break; }
                u++;
            }
            Vector2 CameraPosition = new Vector2(float.Parse(sub.Substring(0, u)), float.Parse(sub.Substring(u + 1)));
            bool RenderHitboxes = settings[1].Substring(13) == "true" ? true : false;

            booter.CameraPosQueue = CameraPosition;

            // Load TextureMAP
            string[] TextureMAPLines = File.ReadAllLines(ProjPath + @"\ExternalInfo\TextureMAP.txt");

            foreach (string i in TextureMAPLines)
            {
                int p = 0;
                foreach (char j in i)
                {
                    if (j == "=".ToCharArray()[0])
                        break;
                    p++;
                }
                string ObjectNameKey = i.Substring(0, p);
                string TextureNameValue = i.Substring(p + 1);
                TextureMAP.Add(ObjectNameKey, TextureNameValue);
            }

            // Load TriggerFunctions
            foreach (string i in Key.Values)
            {
                if (i == "Air")
                    continue;

                string[] ObjectProperties = File.ReadAllLines(ProjPath + $@"\ExternalInfo\GameObjects\{i}.txt");
                if (ObjectProperties[1].Substring(10) == "true")
                {
                    if (colMan.functionsMap.ContainsKey(i))
                        continue;
                    colMan.functionsMap.Add(i, ObjectProperties[2].Substring(16));
                    colMan.ClassMap.Add(i, i);
                }
            }

            // Load Tags -- currently only works with 1 tag per object (Will be changed in future)
            string[] TagLines = File.ReadAllLines(ProjPath + @"\ExternalInfo\Tags.txt");
            foreach (string TagLine in TagLines)
            {
                int p = 0;
                foreach (char j in TagLine)
                {
                    if (j == "=".ToCharArray()[0])
                        break;
                    p++;
                }

                string Obj = TagLine.Substring(0, p);
                string Tag = TagLine.Substring(p + 1);
                if (Tag != "null")
                    TagMAP.Add(Obj, Tag);
            }

            // Load Map
            Bitmap bitmap = new Bitmap(ProjPath + $@"\{SceneName}\Layer{layer}\Scene.png");

            List<List<Vector4>> PixelData = new List<List<Vector4>>();

            int MapWidth = bitmap.Width;
            int MapHeight = bitmap.Height;

            int x = 0;
            while (x < MapWidth)
            {
                List<Vector4> Column = new List<Vector4>(); 
                int y = 0;
                while (y < MapHeight)
                {
                    Color color = bitmap.GetPixel(x, y);
                    Column.Add(new Vector4( color.R, color.G, color.B, color.A ));
                    y++;
                }
                PixelData.Add(Column);
                x++;
            }

            int z = 0;
            foreach (List<Vector4> column in PixelData)
            {
                int y = 0;
                foreach (Vector4 pixel in column)
                {
                    if (Key[pixel] != "Air")
                    {
                        string[] ObjectProperties = File.ReadAllLines(ProjPath + $@"\ExternalInfo\GameObjects\{Key[pixel]}.txt");
                        // load offset
                        int t = 0; // camera start position
                        string offsetstring = ObjectProperties[3].Substring(7);
                        foreach (char i in offsetstring)
                        {
                            if (i == ",".ToCharArray()[0]) { break; }
                            t++;
                        }
                        Vector2 offset = new Vector2(float.Parse(offsetstring.Substring(0, t)), float.Parse(offsetstring.Substring(t + 1)));
                        // load object
                        GameObject Obj = new GameObject(GameData_Path + $@"\{TextureMAP[Key[pixel]]}.png", Engine.White);
                        Obj.WorldPosition = new Vector2(z - CameraPosition.X + offset.X, MapHeight - y - CameraPosition.Y + offset.Y);
                        Obj.Scale = new Vector2(1, 1);
                        Obj.ObjName = Key[pixel];

                        if (TagMAP.ContainsKey(Key[pixel]))
                            Obj.Tags.Add(TagMAP[Key[pixel]]); // fetch tag based on obj name (obj name is gotten using the pixel)
                        
                        if (ObjectProperties[0].Substring(12) == "true")
                        {
                            SquareCollider ObjCollider = new SquareCollider(Obj, Obj.WorldPosition, Obj.WorldPosition + Obj.Scale, RenderHitboxes);
                            colMan.Colliders.Add(ObjCollider);

                            ObjCollider.isTrigger = ObjectProperties[1].Substring(10) == "true" ? true : false;
                        }

                        booter.Objects.Add(Obj);
                        GameObject.Loaded(Obj);
                    }
                    y++;
                }
                z++;
            }
        }
    }
}
