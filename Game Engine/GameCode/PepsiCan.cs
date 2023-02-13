using engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game_Engine
{
    public class PepsiCan
    {
        public static void Pickup(Collider pepsican, Collider other)
        {
            if (!other.BoundObject.Tags.Contains("Player")) // other collider must be the player
                return;

            if (PepsiManager.PepsiCansList.Contains(pepsican))
            {
                pepsican.BoundObject.Destroy();
                PepsiManager.PepsiCounter++;
            }
        }
    }

    public static class PepsiManager
    {
        public static List<Collider> PepsiCansList = new List<Collider>();
        public static int PepsiCounter = 0;
    }
}