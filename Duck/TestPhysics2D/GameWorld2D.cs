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
                    Watermelon a;
                    Watermelon b;

                    if (mObjects[targetObjectIndex] is Watermelon)
                    {
                        a = (Watermelon)mObjects[targetObjectIndex];
                    }
                    else
                    {
                        continue;
                    }
                    if (mObjects[OtherObjectIndex] is Watermelon)
                    {
                        b = (Watermelon)mObjects[OtherObjectIndex];
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

        private void ResolveWatermelonObjectCollision(Watermelon a,
                                    Watermelon b,
                                    Vector2 normal,
                                    float penetration)
        {
            if (a.Level != b.Level)
            {
                ResolveCollision(a, b, normal, penetration); // 레벨이 같지 않다면 그냥 서로 부딧침
                return;
            }

            //TODO: 같은 등금의 과일끼리 충돌 시 합쳐지를 로직 추가

            mObjects.Remove(a);
            mObjects.Remove(b);
            if (a.Level != EWatermelonLevel.ELEVEN)
            {
                Watermelon newFruit = new Watermelon(a.Position, a.Level + 1);
                mObjects.Add(newFruit);
            }
        }
    }
}
