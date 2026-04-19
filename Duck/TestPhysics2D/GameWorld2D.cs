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
            HasGravity = true;
        }

        protected override void HandleCollisions()
        {
            for (int targetObjectIndex = 0; targetObjectIndex < mObjects.Count; targetObjectIndex++)
            {
                for (int OtherObjectIndex = targetObjectIndex + 1; OtherObjectIndex < mObjects.Count; OtherObjectIndex++)
                {
                    // Watermelone 게임에서는 벽을 제외하면 WatermelonObject만 존재해야 함
                    WatermelonObject a;
                    WatermelonObject b;

                    if (mObjects[targetObjectIndex] is WatermelonObject)
                    {
                        a = (WatermelonObject)mObjects[targetObjectIndex];
                    }
                    else
                    {
                        continue;
                    }
                    if (mObjects[OtherObjectIndex] is WatermelonObject)
                    {
                        b = (WatermelonObject)mObjects[OtherObjectIndex];
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
            if (a.Level != b.Level)
            {
                ResolveCollision(a, b, normal, penetration); // 레벨이 같지 않다면 그냥 서로 부딧침
            }


        }
    }
}
