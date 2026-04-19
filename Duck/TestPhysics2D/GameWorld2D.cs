using DuckPhysics.Physics2D;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Numerics;
using System.Text;

namespace WatermelonGame
{
    public class GameWorld2D : World2D
    {
        public GameWorld2D() : base(0.5f)
        {
        }

        protected override void HandleCollisions()
        {
            for (int i = 0; i < mObjects.Count; i++)
            {
                for (int j = i + 1; j < mObjects.Count; j++)
                {
                    WatermelonObject a;
                    WatermelonObject b;

                    if (mObjects[i] is WatermelonObject)
                    {
                        a = (WatermelonObject)mObjects[i];
                    }
                    else
                    {
                        continue;
                    }
                    if (mObjects[j] is WatermelonObject)
                    {
                        b = (WatermelonObject)mObjects[i];
                    }
                    else
                    {
                        continue;
                    }

                    // 반지름
                    float radiusA = a.Collider.Size.X * 0.5f;
                    float radiusB = b.Collider.Size.X * 0.5f;

                    Vector2 delta = b.Position - a.Position;
                    float distance = delta.Length();
                    float minDist = radiusA + radiusB;

                    if (distance >= minDist || distance <= 0.0001f) // 충돌 처리를 하지 않는 경우
                        continue;

                    Vector2 normal = Vector2.Normalize(delta);
                    float penetration = minDist - distance;

                    ResolveWatermelonObjectCollision(a, b, normal, penetration);
                }
            }
        }

        private void ResolveWatermelonObjectCollision(WatermelonObject a,
                                    WatermelonObject b,
                                    Vector2 normal,
                                    float penetration)
        {

        }
    }
}
