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

        public static SpriteRenderer spriteRenderer()
        {
            SpriteRenderer renderer = (SpriteRenderer)player().GetComponent("SpriteRenderer");

            return renderer;
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
            SpriteRenderer spriteRenderer = (SpriteRenderer)player().GetComponent("SpriteRenderer");

            if (spriteRenderer == null)
                return; // no renderer so return out

            spriteRenderer.FlipY = true;
            spriteRenderer.TexturePath = sprites[sprite];
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