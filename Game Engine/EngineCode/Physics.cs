using engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Game_Engine
{
    public class Physics
    {
        public struct Ray
        {
            public Vector2 Position; // World Position
            public Vector2 Direction; // x = 0 - 1, y = 0 - 1 floats
            public float StepSize;
            public int StepCount;

            public Ray(Vector2 position, Vector2 direction, float stepSize, int stepCount)
            {
                Position = position;
                Direction = new Vector2(direction.X, direction.Y);
                StepSize = stepSize;
                StepCount = stepCount;
            }
        }

        public class HitInfo
        {
            public Collider Collider;

            public HitInfo()
            {
                Collider = null;
            }

            public HitInfo(Collider collider)
            {
                Collider = collider;
            }
        }

        public class TagMask
        {
            public List<string> Tags;

            public TagMask()
            {
                Tags = new List<string>();
            }

            public TagMask(List<string> tags)
            {
                Tags = tags;
            }
        }

        public class Gravity : Component
        {
            public float Strength;
            public Vector2 Velocity = new Vector2();

            public Gravity(string name, GameObject obj, Booter booter) : base(name, obj)
            {
                booter.Tick += Tick;
            }

            public void Tick(List<string> keys)
            {
                HitInfo hitInfo;
                Vector2 rayPos = BoundObject.WorldPosition + new Vector2(BoundObject.Scale.X / 2, -BoundObject.Scale.Y / 4);
                Ray ray = new Ray(rayPos, new Vector2(0, -1), 0.05f, 10);
                TagMask tagMask = new TagMask();
                tagMask.Tags.Add("Player");
                tagMask.Tags.Add("PepsiCan");

                if (Raycast(ray, tagMask, out hitInfo))
                {
                    Velocity.Y = 0;
                }
                else
                {
                    Velocity.Y -= Strength * Time.DeltaTime;
                }
                GameObject.MoveWithCollision(BoundObject, Velocity, EngineManager.booterInstance, EngineManager.colmanInstance);
                //Console.WriteLine($"Gravity Tick, current velocity: {Velocity}, delta time: {Time.DeltaTime}, frame 0: {Time.Frames[0]}");
                // will add logis as soon as raycasting is added
            }
        }

        public static bool Raycast(Ray ray, out HitInfo hitInfo)
        { // discrete collision detection
            bool hit = false;
            HitInfo _hitInfo = new HitInfo();

            int step = 0;
            while (step < ray.StepCount - 1)
            {
                foreach (GameObject obj in EngineManager.booterInstance.Objects)
                {
                    if (obj is IRaycastable raycastableObject) // check if has IRaycastable interface
                    {
                        Vector2 dist = obj.WorldPosition - (ray.Position + ray.Direction * ray.StepSize);
                        if (EngineManager.colmanInstance.CheckPoint((ray.Position + ray.Direction * ray.StepSize)))
                        {
                            hit = true;
                            _hitInfo.Collider = (Collider)obj.GetComponent(typeof(SquareCollider));
                            break;
                        }
                    }
                }
                step++;
            }

            hitInfo = _hitInfo;
            return hit;
        }

        public static bool Raycast(Ray ray, TagMask tagMask, out HitInfo hitInfo)
        { // discrete collision detection
            bool hit = false;
            HitInfo _hitInfo = new HitInfo();

            int step = 0;
            while (step < ray.StepCount - 1)
            {
                Vector2 pos = ray.Position + ray.Direction * ray.StepSize * step;
                if (!EngineManager.booterInstance.points.ContainsKey(pos))
                    EngineManager.booterInstance.points.Add(pos, new Vector4(step*25.5f, 0, 0, 0));
                foreach (GameObject obj in EngineManager.booterInstance.Objects)
                {
                    if (obj is IRaycastable raycastableObject) // check if has IRaycastable interface
                    {
                        bool self = false;
                        foreach (string tag in obj.Tags)
                        {
                            if (tagMask.Tags.Contains(tag))
                            {
                                hitInfo = _hitInfo;
                                self = true;
                                break;
                            }
                        }
                        if (self) { continue; }
                        Vector2 dist = obj.WorldPosition - pos;
                        if (obj.GetComponent("SquareCollider") == null)
                            continue;

                        if (((SquareCollider)obj.GetComponent("SquareCollider")).CheckPoint(pos))
                        {
                            hit = true;
                            _hitInfo.Collider = (Collider)obj.GetComponent(typeof(SquareCollider));
                            break;
                        }
                    }
                }
                step++;
            }

            hitInfo = _hitInfo;
            return hit;
        }
        /* // Shitty attempt at continuous collision detection
        public static bool TryGetIntersectingPoint(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 hitPoint)
        {
            hitPoint = Vector2.Zero;

            // determinant
            float d = (a1.x - a2.x) * (b1.y - b2.y) - (a1.y - b2.y) * (b1.x - b2.x);

            // check if lines are parallel
            if (Approximately(d, epsilon)) return false;

            float px = (a1.x * a2.y - a1.y * a2.x) * (b1.x - b2.x) - (a1.x - a2.x) * (b1.x * b2.y - b1.y * b2.x);
            float py = (a1.x * a2.y - a1.y * a2.x) * (b1.y - b2.y) - (a1.y - a2.y) * (b1.x * b2.y - b1.y * b2.x);

            hitPoint = new Vector2(px, py) / d;
            return true;
        }
        */
    }
}
