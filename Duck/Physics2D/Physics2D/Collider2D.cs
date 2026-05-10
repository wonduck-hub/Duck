using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace DuckPhysics.Physics2D
{
    public enum EColliderType
    {
        CIRCLE,
        RECTANGLE,
        POLYGON
    }
    public struct CollisionInfo
    {
        public bool IsColliding;
        public Vector2 Normal;
        public float Penetration;

        public CollisionInfo(bool isColliding, Vector2 normal, float penetration)
        {
            IsColliding = isColliding;
            Normal = normal;
            Penetration = penetration;
        }

        public static CollisionInfo NoCollision()
        {
            return new CollisionInfo(false, Vector2.Zero, 0f);
        }
    }
    public class Collider2D
    {
        #region 속성들
        private IObject2D mOwner { get; set; }

        public EColliderType Type { get; set; }
        public Vector2 Offset { get; set; } // 콜라이더의 중심이 객체의 위치에서 얼마나 떨어져 있는지 나타내는 벡터
        public float? Radius
        {
            get
            {
                if (Type == EColliderType.CIRCLE)
                {
                    return Size.X / 2f;
                }
                else
                {
                    return null;
                }
            }
        }
        public Vector2 Size { get; set; }     // Rectangle
        public List<Vector2> Points { get; set; } // Polygon

        public bool IsTrigger { get; set; }
        public bool IsStatic { get; set; }
        public int Layer { get; set; } // 충돌 그룹을 나누기 위한 레이어 -> 예: - 예: Player는 Enemy와 충돌하지만, PlayerProjectile은 Player와는 충돌하지 않도록 설정
        #endregion

        public Collider2D(IObject2D owner, EColliderType type, Vector2 offset, Vector2 size, int layer)
        {
            mOwner = owner;
            Type = type;
            Offset = offset;

            IsTrigger = true;
            IsStatic = false;
            Layer = layer;
            Points = new List<Vector2>();

            size.X = Math.Max(size.X, 1);
            size.Y = Math.Max(size.Y, 1);
            Size = size;
        }

        public static CollisionInfo GetCollisionInfo(IObject2D currentObject, IObject2D otherObject)
        {
            if (currentObject.Collider.Type == EColliderType.CIRCLE &&
                otherObject.Collider.Type == EColliderType.CIRCLE)
            {
                return GetCircleCircleCollisionInfo(currentObject, otherObject);
            }

            if (currentObject.Collider.Type == EColliderType.CIRCLE &&
                otherObject.Collider.Type == EColliderType.RECTANGLE)
            {
                return GetCircleRectCollisionInfo(currentObject, otherObject);
            }

            if (currentObject.Collider.Type == EColliderType.RECTANGLE &&
                otherObject.Collider.Type == EColliderType.CIRCLE)
            {
                CollisionInfo info = GetCircleRectCollisionInfo(otherObject, currentObject);

                if (info.IsColliding)
                    info.Normal = -info.Normal;

                return info;
            }

            if (currentObject.Collider.Type == EColliderType.RECTANGLE &&
                otherObject.Collider.Type == EColliderType.RECTANGLE)
            {
                return GetRectRectCollisionInfo(currentObject, otherObject);
            }

            return CollisionInfo.NoCollision();
        }

        private static CollisionInfo GetCircleCircleCollisionInfo(IObject2D currentObject, IObject2D otherObject)
        {
            float radiusA = currentObject.Collider.Radius!.Value * currentObject.Scale;
            float radiusB = otherObject.Collider.Radius!.Value * otherObject.Scale;

            Vector2 delta = otherObject.Position - currentObject.Position;
            float distance = delta.Length();
            float minDist = radiusA + radiusB;

            if (distance >= minDist || distance <= 0.0001f)
                return CollisionInfo.NoCollision();

            Vector2 normal = delta / distance;
            float penetration = minDist - distance;

            return new CollisionInfo(true, normal, penetration);
        }

        private static CollisionInfo GetCircleRectCollisionInfo(IObject2D circleObj, IObject2D rectObj)
        {
            Vector2 circleCenter = circleObj.Position;
            Vector2 rectCenter = rectObj.Position;

            float radius = circleObj.Collider.Radius!.Value * circleObj.Scale;

            float cos = MathF.Cos(-rectObj.Rotation);
            float sin = MathF.Sin(-rectObj.Rotation);

            Vector2 relative = circleCenter - rectCenter;

            Vector2 localCirclePos = new Vector2(
                relative.X * cos - relative.Y * sin,
                relative.X * sin + relative.Y * cos
            );

            Vector2 half = rectObj.Collider.Size * rectObj.Scale / 2f;

            float closestX = Clamp(localCirclePos.X, -half.X, half.X);
            float closestY = Clamp(localCirclePos.Y, -half.Y, half.Y);

            Vector2 closestPoint = new Vector2(closestX, closestY);
            Vector2 diff = localCirclePos - closestPoint;

            float distanceSquared = diff.LengthSquared();

            if (distanceSquared > radius * radius)
                return CollisionInfo.NoCollision();

            float distance = MathF.Sqrt(distanceSquared);

            Vector2 localNormal;

            float penetration;

            if (distance <= 0.0001f)
            {
                Vector2 distancesToEdges = new Vector2(
                    half.X - MathF.Abs(localCirclePos.X),
                    half.Y - MathF.Abs(localCirclePos.Y)
                );

                if (distancesToEdges.X < distancesToEdges.Y)
                {
                    if (localCirclePos.X >= 0)
                    {
                        localNormal = new Vector2(-1f, 0f);
                    }
                    else
                    {
                        localNormal = new Vector2(1f, 0f);
                    }
                    distance = distancesToEdges.X;
                }
                else
                {
                    if (localCirclePos.Y >= 0)
                    {
                        localNormal = new Vector2(0f, -1f);
                    }
                    else
                    {
                        localNormal = new Vector2(0f, 1f);
                    }
                    distance = distancesToEdges.Y;
                }

                penetration = radius + distance;
            }
            else
            {
                localNormal = -(diff / distance);
                penetration = radius - distance;
            }

            float cosBack = MathF.Cos(rectObj.Rotation);
            float sinBack = MathF.Sin(rectObj.Rotation);

            Vector2 worldNormal = new Vector2(
                localNormal.X * cosBack - localNormal.Y * sinBack,
                localNormal.X * sinBack + localNormal.Y * cosBack
            );

            return new CollisionInfo(true, worldNormal, penetration);
        }

        private static bool FindSeparatingAxis(
                                        Vector2[] verticesA,
                                        Vector2[] verticesB,
                                        ref float minOverlap,
                                        ref Vector2 smallestAxis)
        {
            for (int i = 0; i < verticesA.Length; i++)
            {
                Vector2 p1 = verticesA[i];
                Vector2 p2 = verticesA[(i + 1) % verticesA.Length];

                Vector2 edge = p2 - p1;
                Vector2 axis = new Vector2(-edge.Y, edge.X);

                if (axis.LengthSquared() <= 0.000001f)
                    continue;

                axis = Vector2.Normalize(axis);

                ProjectVertices(verticesA, axis, out float minA, out float maxA);
                ProjectVertices(verticesB, axis, out float minB, out float maxB);

                if (maxA < minB || maxB < minA)
                    return true;

                float overlap = MathF.Min(maxA, maxB) - MathF.Max(minA, minB);

                if (overlap < minOverlap)
                {
                    minOverlap = overlap;
                    smallestAxis = axis;
                }
            }

            return false;
        }

        private static CollisionInfo GetRectRectCollisionInfo(IObject2D a, IObject2D b)
        {
            Vector2[] verticesA = GetRectVertices(a);
            Vector2[] verticesB = GetRectVertices(b);

            float minOverlap = float.MaxValue;
            Vector2 smallestAxis = Vector2.Zero;

            if (FindSeparatingAxis(verticesA, verticesB, ref minOverlap, ref smallestAxis))
                return CollisionInfo.NoCollision();

            if (FindSeparatingAxis(verticesB, verticesA, ref minOverlap, ref smallestAxis))
                return CollisionInfo.NoCollision();

            Vector2 delta = b.Position - a.Position;

            if (Vector2.Dot(delta, smallestAxis) < 0)
                smallestAxis = -smallestAxis;

            return new CollisionInfo(true, smallestAxis, minOverlap);
        }

        private static Vector2[] GetRectVertices(IObject2D obj)
        {
            Vector2 half = obj.Collider.Size * obj.Scale / 2f;

            Vector2[] localVertices =
            {
                new Vector2(-half.X, -half.Y),
                new Vector2( half.X, -half.Y),
                new Vector2( half.X,  half.Y),
                new Vector2(-half.X,  half.Y)
            };

            Vector2[] worldVertices = new Vector2[4];

            float cos = MathF.Cos(obj.Rotation);
            float sin = MathF.Sin(obj.Rotation);

            for (int i = 0; i < 4; i++)
            {
                Vector2 v = localVertices[i];

                Vector2 rotated = new Vector2(
                    v.X * cos - v.Y * sin,
                    v.X * sin + v.Y * cos
                );

                worldVertices[i] = obj.Position + rotated;
            }

            return worldVertices;
        }

        private static void ProjectVertices(
            Vector2[] vertices,
            Vector2 axis,
            out float min,
            out float max)
        {
            min = Vector2.Dot(vertices[0], axis);
            max = min;

            for (int i = 1; i < vertices.Length; i++)
            {
                float projection = Vector2.Dot(vertices[i], axis);

                if (projection < min)
                    min = projection;

                if (projection > max)
                    max = projection;
            }
        }

        private static float Clamp(float value, float min, float max)
        {
            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }
    }
}
