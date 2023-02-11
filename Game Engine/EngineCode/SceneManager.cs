using engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Engine
{
    public class Scene
    {
        public string Name;
        public string Path;

        public Scene(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }
    public static class SceneManager
    {
        public static int SceneID;
        public static List<Scene> Scenes = new List<Scene>();
        private static MapLoader Loader = null;

        public static void SetupScenes(string Root_Dir)
        {
            int SceneCount = Directory.GetDirectories(Root_Dir).Length - 1; // get all scene paths + scene count
            string[] Directories = Directory.GetDirectories(Root_Dir);
            List<string> ScenePaths = Directories.ToList();
            ScenePaths.Remove(Root_Dir + @"\ExternalInfo");
            // chop off paths to get just the scene name
            List<string> SceneNames = new List<string>();
            foreach (string ScenePath in ScenePaths)
            {
                string[] splitPath = ScenePath.Split(@"\".ToCharArray());
                SceneNames.Add(splitPath[splitPath.Length - 1]);
            }

            int index = 0;
            foreach (string SceneName in SceneNames)
            {
                Scene newScene = new Scene(SceneName, ScenePaths[index]);
                Scenes.Add(newScene);
                index++;
            }

            Loader = new MapLoader();
        }

        public static int LoadScene(int _sceneID) // if returns 1 then scene does not exist or is missing from scenes list, 0 means successfull
        {
            Booter booter = EngineManager.booterInstance;

            if (!ValidScene(_sceneID))
                return 1;

            int total = booter.Objects.Count;
            for (int x = 0; x < total - 1; x++)
            {
                booter.Objects[x].Destroy();
            }

            int layerCount = Directory.GetDirectories(Scenes[_sceneID].Path).Length;
            int currentLayer = 0;
            while (currentLayer < layerCount)
            {
                Loader.Load(Scenes[_sceneID].Path, Scenes[_sceneID].Name, currentLayer);
                currentLayer++;
            }

            return 0;
        }

        static bool ValidScene(int id)
        {
            return id < Scenes.Count;
        }
    }
}
