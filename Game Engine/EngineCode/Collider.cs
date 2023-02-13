using engine;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace Game_Engine
{
    public struct Line
    {
        public Vector2 p1;
        public Vector2 p2;

        public Line(Vector2 firstPoint, Vector2 secondPoint)
        {
            p1 = firstPoint;
            p2 = secondPoint;
        }
    }

    public class CollisionManager
    {
        public List<Collider> Colliders = new List<Collider>();
        public Dictionary<string, string> functionsMap = new Dictionary<string, string>(); // (bound)object name to function name
        public Dictionary<string, string> ClassMap = new Dictionary<string, string>(); // (bound)object name to class name
        public Dictionary<string, List<string>> ArgsMap = new Dictionary<string, List<string>>(); // (bound)object name to Args Map

        public CollisionManager()
        {
            EngineManager.colmanInstance = this;
        }

        public void ColliderTick(Booter instance)
        {
            List<MethodInfo> Methods = new List<MethodInfo>();
            List<object[]> Args = new List<object[]>();

            foreach (SquareCollider collider in Colliders)
            {
                collider.ColliderTick(instance);
                Collider triggeredObj = null;
                if (TriggerRect(collider).Length != 0)
                {
                    triggeredObj = TriggerRect(collider)[0];
                }
                    
                if (triggeredObj != null)
                {
                    object[] args = new object[ArgsMap[triggeredObj.BoundObject.ObjName].Count];
                    MethodInfo method = null;

                    Type t = Type.GetType($@"Game_Engine.{ClassMap[triggeredObj.BoundObject.ObjName]}");
                    method
                            = t.GetMethod(functionsMap[triggeredObj.BoundObject.ObjName], BindingFlags.Static | BindingFlags.Public);

                    int x = 0;
                    foreach(string i in ArgsMap[triggeredObj.BoundObject.ObjName])
                    {
                        switch (i)
                        {
                            case "":
                                Console.WriteLine("NO VALID ARGUMENTS PASSED IN TA FILE!");
                                break;
                            case "self":
                                args[x] = triggeredObj; // trigger collider
                                break;
                            case "other":
                                args[x] = collider; // other collider
                                break;
                        }
                        x++;
                    }

                    Methods.Add(method);
                    Args.Add(args);
                }
            }

            int methodIndex = 0;
            foreach (MethodInfo method in Methods)
            {
                if (method != null)
                    method.Invoke(null, Args[methodIndex]);
                methodIndex++;
            }
        }

        public bool CheckPoint(Vector2 Position)
        {
            bool res = false;
            foreach (Collider i in Colliders)
            {
                if (i.CheckPoint(Position))
                {
                    res = true;
                    break;
                }
            }
            return res;
        }

        public Collider CollideRect(SquareCollider collider)
        {
            Vector2 MinPoint = collider.MinPoint;
            Vector2 MaxPoint = collider.MaxPoint;
            SquareCollider res = null;
            foreach (SquareCollider i in Colliders)
            {
                if (i != collider && !i.isTrigger)
                {
                    bool thisone = true;
                    // if rectangle has area 0, no overlap
                    if (MinPoint.X == MaxPoint.X || MinPoint.Y == MaxPoint.Y || i.MaxPoint.X == i.MinPoint.X || i.MinPoint.Y == i.MaxPoint.Y)
                        thisone = false;

                    // If one rectangle is on left side of other
                    if (MinPoint.X > i.MaxPoint.X || i.MinPoint.X > MaxPoint.X)
                        thisone = false;

                    // If one rectangle is above other
                    if (MaxPoint.Y < i.MinPoint.Y || i.MaxPoint.Y < MinPoint.Y)
                        thisone = false;

                    if (thisone)
                    {
                        res = i;
                        break;
                    }
                }
            }
            return res;
        }

        public Collider CollideRect(Vector4 rect) // RECT MUST BE INPUT AS WORLDSPACE COORDS
        {
            Vector2 MinPoint = new Vector2(rect.X, rect.Y);
            Vector2 MaxPoint = new Vector2(rect.Z, rect.W);
            SquareCollider res = null;
            foreach (SquareCollider i in Colliders)
            {
                if (!i.isTrigger)
                {
                    Vector2 MinPoint1 = i.MinPoint - new Vector2(0.45f, 0);
                    Vector2 MaxPoint1 = i.MaxPoint + new Vector2(0, 0.3f);
                    bool thisone = true;
                    // if rectangle has area 0, no overlap
                    if (MinPoint.X == MaxPoint.X || MinPoint.Y == MaxPoint.Y || MaxPoint1.X == MinPoint1.X || MinPoint1.Y == MaxPoint1.Y)
                        thisone = false;

                    // If one rectangle is on left side of other
                    if (MinPoint.X > MaxPoint1.X || MinPoint1.X > MaxPoint.X)
                        thisone = false;

                    // If one rectangle is above other
                    if (MaxPoint.Y < MinPoint1.Y || MaxPoint1.Y < MinPoint.Y)
                        thisone = false;

                    if (thisone)
                    {
                        res = i;
                        break;
                    }
                }
            }
            return res;
        }

        public Collider CollideRect(Vector4 rect, Collider Ignore) // RECT MUST BE INPUT AS WORLDSPACE COORDS
        {
            Vector2 MinPoint = new Vector2(rect.X, rect.Y);
            Vector2 MaxPoint = new Vector2(rect.Z, rect.W);
            SquareCollider res = null;
            foreach (SquareCollider i in Colliders)
            {
                if (i != Ignore && !i.isTrigger)
                {
                    Vector2 MinPoint1 = i.MinPoint - new Vector2(0.45f, 0);
                    Vector2 MaxPoint1 = i.MaxPoint + new Vector2(0, 0.3f);
                    bool thisone = true;
                    // if rectangle has area 0, no overlap
                    if (MinPoint.X == MaxPoint.X || MinPoint.Y == MaxPoint.Y || MaxPoint1.X == MinPoint1.X || MinPoint1.Y == MaxPoint1.Y)
                        thisone = false;

                    // If one rectangle is on left side of other
                    if (MinPoint.X > MaxPoint1.X || MinPoint1.X > MaxPoint.X)
                        thisone = false;

                    // If one rectangle is above other
                    if (MaxPoint.Y < MinPoint1.Y || MaxPoint1.Y < MinPoint.Y)
                        thisone = false;

                    if (thisone)
                    {
                        res = i;
                        break;
                    }
                }
            }
            return res;
        }

        // trigger
        public SquareCollider[] TriggerRect(SquareCollider collider)
        {
            Vector2 MinPoint = collider.MinPoint;
            Vector2 MaxPoint = collider.MaxPoint;
            List<SquareCollider> res = new List<SquareCollider>();
            foreach (SquareCollider i in Colliders)
            {
                if (i.isTrigger && i != collider)
                {
                    Vector2 MinPoint1 = i.MinPoint;
                    Vector2 MaxPoint1 = i.MaxPoint;
                    bool thisone = true;
                    // if rectangle has area 0, no overlap
                    if (MinPoint.X == MaxPoint.X || MinPoint.Y == MaxPoint.Y || MaxPoint1.X == MinPoint1.X || MinPoint1.Y == MaxPoint1.Y)
                        thisone = false;

                    // If one rectangle is on left side of other
                    if (MinPoint.X > MaxPoint1.X || MinPoint1.X > MaxPoint.X)
                        thisone = false;

                    // If one rectangle is above other
                    if (MaxPoint.Y < MinPoint1.Y || MaxPoint1.Y < MinPoint.Y)
                        thisone = false;

                    if (thisone)
                    {
                        res.Add(i);
                        break;
                    }
                }
            }
            return res.ToArray();
        }
    }
    public abstract class Collider : Component
    {
        public bool isTrigger;
        public abstract bool CheckPoint(Vector2 Position);
        public Collider(GameObject BindingObject) : base("Collider", BindingObject) { }
    }

    public class SquareCollider : Collider
    {
        public Vector2 MinPoint;
        public Vector2 MaxPoint;
        KeyValuePair<Vector4, Vector4> Debug;
        bool DebugSet;
        bool DebugDraw;
        List<Line> Lines = new List<Line>();

        public SquareCollider(GameObject BindingObject, Vector2 _minPoint, Vector2 _maxPoint, bool _drawDebug) : base(BindingObject)
        {
            Name = "SquareCollider";
            BoundObject.destroy += CleanupCollider;
            DebugDraw = _drawDebug;
        }

        public void CleanupCollider()
        {
            Booter booter = EngineManager.booterInstance;
            CollisionManager colMan = EngineManager.colmanInstance;

            if (DebugDraw)
                booter.DebugRects.Remove(Debug);
            colMan.Colliders.Remove(this);
        }

        public void ColliderTick(Booter _instance)
        {
            SpriteRenderer spriteRenderer = (SpriteRenderer)BoundObject.GetComponent("SpriteRenderer");

            if (spriteRenderer == null)
                return; // Don't Complete Tick - No SpriteRenderer - POTENTIALLY DANGEROUS CODE - Must Fix Later with error system

            MinPoint = GameObject.WorldToScreenSpace( _instance, BoundObject.WorldPosition );
            MaxPoint = MinPoint + new Vector2(BoundObject.Scale.X * spriteRenderer.TextureRes.X, BoundObject.Scale.Y * spriteRenderer.TextureRes.Y) * 2;

            float MinPointYBeforeChange = MinPoint.Y;
            MinPoint = new Vector2(MinPoint.X, MaxPoint.Y);
            MaxPoint = new Vector2(MaxPoint.X, MinPointYBeforeChange);

            MinPoint = GameObject.ScreenToWorldSpace( _instance, MinPoint );
            MaxPoint = GameObject.ScreenToWorldSpace( _instance, MaxPoint );

            /*
            Lines.Clear();
            Lines.Add(new Line(new Vector2(MinPoint.X, MinPoint.Y), new Vector2(MaxPoint.X, MinPoint.Y)));
            Lines.Add(new Line(new Vector2(MinPoint.X, MinPoint.Y), new Vector2(MinPoint.X, MaxPoint.Y)));

            Lines.Add(new Line(new Vector2(MinPoint.X, MaxPoint.Y), new Vector2(MaxPoint.X, MaxPoint.Y)));
            Lines.Add(new Line(new Vector2(MaxPoint.X, MinPoint.Y), new Vector2(MaxPoint.X, MaxPoint.Y)));

            Lines.Add(new Line(new Vector2(MinPoint.X, MinPoint.Y), new Vector2(MaxPoint.X, MaxPoint.Y)));
            Lines.Add(new Line(new Vector2(MinPoint.X, MaxPoint.Y), new Vector2(MaxPoint.X, MinPoint.Y)));
            */

            if (!DebugSet && DebugDraw)
            {
                Debug = new KeyValuePair<Vector4, Vector4>(new Vector4(MinPoint, MaxPoint.X, MaxPoint.Y), Engine.Red);
                _instance.DebugRects.Add(Debug);
                DebugSet = true;
            }

            if (DebugDraw)
            {
                bool found = true;
                int index = 0;
                foreach (KeyValuePair<Vector4, Vector4> i in _instance.DebugRects)
                {
                    if (i.Value == Debug.Value && i.Key == Debug.Key)
                    {
                        found = true;
                        break;
                    }
                    index++;
                }

                float PixelSize;

                // Key of keyvaluepair must be the worldspace minpoint as the first vec2 and the worldspace DeltaX and DeltaY from point max - min
                Debug = new KeyValuePair<Vector4, Vector4>(new Vector4(MinPoint.X, MinPoint.Y, MaxPoint.X - MinPoint.X, MaxPoint.Y - MinPoint.Y), Engine.Red);

                if (found)
                {
                    _instance.DebugRects[index] = Debug;
                }
            }
        }

        public override bool CheckPoint(Vector2 position)
        {
            bool returnval = false;
            if (position.X >= MinPoint.X && position.X <= MaxPoint.X) // x check
            {
                if (position.Y >= MinPoint.Y && position.Y <= MaxPoint.Y) // y check
                {
                    returnval = true;
                }
            }
            return returnval;
        }
    }

    public class CircleCollider : Collider
    {
        public float Radius;

        public CircleCollider(GameObject BindingObject) : base(BindingObject)
        {
            Name = "CircleCollider";
            BoundObject = BindingObject;
        }

        public override bool CheckPoint(Vector2 position)
        {
            bool res = MathF.Pow((BoundObject.WorldPosition.X + BoundObject.Scale.X / 2) - position.X, 2) + MathF.Pow((BoundObject.WorldPosition.Y + BoundObject.Scale.Y / 2) - position.Y, 2) <= MathF.Pow(Radius, 2);

            return res;
        }
    }
}