using DuckPhysics.Physics2D;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
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
            for (int currentObjectIndex = 0; currentObjectIndex < mObjects.Count; currentObjectIndex++)
            {
                for (int otherObjectIndex = currentObjectIndex + 1; otherObjectIndex < mObjects.Count; otherObjectIndex++)
                {
                    // Watermelone 게임에서는 벽을 제외하면 WatermelonObject만 존재해야 함
                    IObject2D currentObject;
                    IObject2D otherObject;

                    currentObject = mObjects[currentObjectIndex];
                    otherObject = mObjects[otherObjectIndex];

                    CollisionInfo collisionInfo = Collider2D.GetCollisionInfo(currentObject, otherObject);

                    if (!collisionInfo.IsColliding)
                    {
                        continue;
                    }

                    if (currentObject is Watermelon && otherObject is Watermelon)
                    {
                        ResolveWatermelonObjectCollision((Watermelon)currentObject, (Watermelon)otherObject, collisionInfo.Normal, collisionInfo.Penetration);
                    }
                    else
                    {
                        ResolveCollision(currentObject, otherObject, collisionInfo.Normal, collisionInfo.Penetration);
                    }

                    Debug.WriteLine($"current: {currentObject.GetType().FullName}, index: {currentObjectIndex}");
                    Debug.WriteLine($"other: {otherObject.GetType().FullName}, index: {otherObjectIndex}");
                }
            }
        }

        private void ResolveWatermelonObjectCollision(Watermelon currentObject,
                                    Watermelon otherObject,
                                    Vector2 normal,
                                    float penetration)
        {
            if (currentObject.Level != otherObject.Level)
            {
                ResolveCollision(currentObject, otherObject, normal, penetration); // 레벨이 같지 않다면 그냥 서로 부딧침
                return;
            }

            //TODO: 같은 등금의 과일끼리 충돌 시 합쳐지를 로직 추가

            mObjects.Remove(currentObject);
            mObjects.Remove(otherObject);
            if (currentObject.Level != EWatermelonLevel.ELEVEN)
            {
                Watermelon newFruit = new Watermelon(currentObject.Position, currentObject.Level + 1);
                mObjects.Add(newFruit);
            }
        }
    }
}
