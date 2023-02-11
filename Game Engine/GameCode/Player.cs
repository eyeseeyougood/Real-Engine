using engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Game_Engine
{
    public static class Player
    {
        public static List<string> sprites = new List<string>();

        public static GameObject player()
        {
            GameObject Plr = null;
            foreach (GameObject i in EngineManager.booterInstance.Objects)
            {
                if (i.Tags.Contains("Player"))
                {
                    Plr = i;
                }
            }

            return Plr;
        }
        public static void Movement(Vector2 _moveAmount)
        {
            GameObject Plr = null;
            foreach (GameObject i in EngineManager.booterInstance.Objects)
            {
                if (i.Tags.Contains("Player"))
                {
                    Plr = i;
                }
            }

            if (Plr == null)
                return;

            GameObject.MoveWithCollision(Plr, _moveAmount, EngineManager.booterInstance, EngineManager.colmanInstance);
        }

        public static void SetSprite(int sprite)
        {
            // 1 = facing right
            // 0 = facing left

            player().TexturePath = sprites[sprite];
        }
        public static void AddComponent(Component component)
        {
            foreach (GameObject i in EngineManager.booterInstance.Objects)
            {
                if (i.Tags.Contains("Player"))
                {
                    i.Components.Add(component);
                }
            }
        }
    }
}