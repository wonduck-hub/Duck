using DuckPhysics.Physics2D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace DuckPhysics.Physics2D
{
    public class World2D : IDisposable
    {
        #region 상수들
        protected float DEFAULT_GRAVITY = 9.8f;
        #endregion
        #region 월드 속성들
        protected Stopwatch stopwatch = new Stopwatch();
        protected double lastTime = 0;

        private float mGravitiy;
        public float Gravity
        {
            get { return mGravitiy; }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }
                mGravitiy = value;
            }
        }

        private uint mWidth;
        public uint Width
        {
            get { return mWidth; }
            set { mWidth = value; }
        }
        private uint mHeight;
        public uint Height
        {
            get { return mHeight; }
            set { mHeight = value; }
        }

        private bool mHasGravity = false;
        public bool HasGravity
        {
            get { return mHasGravity; }
            set
            {
                mHasGravity = value;
            }
        }

        // 월드 재질 속성
        public float WallFriction { get; set; } = 0.95f;      // 벽 마찰 계수
        public float WallRestitution { get; set; } = 0.8f;   // 벽 반발 계수
        #endregion

        #region 가지고 있는 개체들(component)
        protected List<IObject2D> mObjects = new List<IObject2D>(1024);
        public List<IObject2D> Objects
        {
            get { return mObjects; }
        }
        #endregion

        #region Constructor
        public World2D()
        {
            Gravity = DEFAULT_GRAVITY;
        }
        public World2D(float gravitiy)
        {
            Gravity = gravitiy;
        }
        public World2D(World2D world) // 복사생성자
            : this(world.Gravity)
        { }


        #endregion
        public void Dispose()
        {
            Debug.Assert(mObjects != null);
            foreach (var obj in mObjects)
            {
                obj.Dispose();
            }
        }

        #region Methods
        public bool TryAddObject(IObject2D obj)
        {
            if (obj == null)
            {
                Debug.WriteLine("File to add object");
                return false;
            }
            mObjects.Add(obj);
            return true;
        }
        #endregion

        public void Start()
        {
            stopwatch.Start();
            lastTime = stopwatch.Elapsed.TotalSeconds;
        }

        #region 화면 갱신에 따른 업데이트 함수
        virtual public void Update()
        {
            double currentTime = stopwatch.Elapsed.TotalSeconds;
            float deltaTime = (float)(currentTime - lastTime);

            // 중력
            if (mHasGravity)
            {
                foreach (var obj in mObjects)
                {

                    obj.Velocity += new System.Numerics.Vector2(0, -Gravity); // 중력 가속도 적용
                }
            }

            // 개체 업데이트(이동)
            foreach (var obj in mObjects)
            {
                Vector2 prevPos = obj.Position;

                obj.Update(deltaTime); // 개체 내부에서 개신해야 하는 로직들 업데이트

                if (obj is DinamicObject2D dyn)
                {
                    float radius = dyn.Collider.Size.X * 0.5f;

                    if (radius > 0.0001f)
                    {
                        Vector2 movement = dyn.Position - prevPos;
                        dyn.Rotation -= movement.X / radius;
                    }
                }
            }

            //TODO: 충돌 처리
            HandleCollisions();

            //TODO: 월드 경계 처리
            HandleWorldBounds();

            lastTime = stopwatch.Elapsed.TotalSeconds;
        }

        protected virtual void HandleCollisions()
        {
            for (int targetObjectIndex = 0; targetObjectIndex < mObjects.Count; targetObjectIndex++)
            {
                for (int OtherObjectIndex = targetObjectIndex + 1; OtherObjectIndex < mObjects.Count; OtherObjectIndex++)
                {
                    DinamicObject2D a;
                    DinamicObject2D b;

                    if (mObjects[targetObjectIndex] is  DinamicObject2D)
                    {
                        a = (DinamicObject2D)mObjects[targetObjectIndex];
                    }
                    else
                    {
                        continue;
                    }
                    if (mObjects[OtherObjectIndex] is DinamicObject2D)
                    {
                        b = (DinamicObject2D)mObjects[OtherObjectIndex];
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

                    ResolveCollision(a, b, normal, penetration);
                }
            }
        }

        protected virtual void ResolveCollision(
                                    DinamicObject2D a,
                                    DinamicObject2D b,
                                    Vector2 normal,
                                    float penetration)
        {
            // 1. 위치 보정
            Vector2 correction = normal * (penetration * 0.5f);
            a.Position -= correction;
            b.Position += correction;

            // 2. 속도 반사/분리
            Vector2 relativeVelocity = b.Velocity - a.Velocity;
            float velocityAlongNormal = Vector2.Dot(relativeVelocity, normal);

            if (velocityAlongNormal > 0)
                return;

            float restitution = 1.0f;
            float impulseScalar = -(1 + restitution) * velocityAlongNormal;
            impulseScalar /= 2.0f;

            Vector2 impulse = impulseScalar * normal;
            a.Velocity -= impulse;
            b.Velocity += impulse;
        }

        virtual protected void HandleWorldBounds()
        {
            foreach (var obj in mObjects)
            {
                var dyn = obj as DinamicObject2D;
                if (dyn == null) continue;

                float radius = dyn.Collider.Size.X * 0.5f;

                // 벽과 물체의 실제 반발/마찰
                float restitution = MathF.Min(dyn.Restitution, WallRestitution);
                float friction = MathF.Sqrt(dyn.Friction * WallFriction);

                // 너무 작은 속도는 떨림 방지용으로 0 처리
                const float stopThreshold = 0.01f;

                // 좌벽 / 우벽
                if (dyn.Position.X - radius < 0)
                {
                    dyn.Position = new Vector2(radius, dyn.Position.Y);

                    dyn.Velocity = new Vector2(
                        -dyn.Velocity.X * restitution, // 법선 반사
                        dyn.Velocity.Y * friction      // 접선 마찰
                    );
                }
                else if (dyn.Position.X + radius > Width)
                {
                    dyn.Position = new Vector2(Width - radius, dyn.Position.Y);

                    dyn.Velocity = new Vector2(
                        -dyn.Velocity.X * restitution,
                        dyn.Velocity.Y * friction
                    );
                }

                // 바닥 / 천장
                if (dyn.Position.Y - radius < 0)
                {
                    dyn.Position = new Vector2(dyn.Position.X, radius);

                    dyn.Velocity = new Vector2(
                        dyn.Velocity.X * friction,      // 접선 마찰
                        -dyn.Velocity.Y * restitution   // 법선 반사
                    );

                    // 바닥에서 미세하게 미끄러지는 현상 완화
                    if (MathF.Abs(dyn.Velocity.X) < stopThreshold)
                    {
                        dyn.Velocity = new Vector2(0f, dyn.Velocity.Y);
                    }

                    if (MathF.Abs(dyn.Velocity.Y) < stopThreshold)
                    {
                        dyn.Velocity = new Vector2(dyn.Velocity.X, 0f);
                    }
                }
                else if (dyn.Position.Y + radius > Height)
                {
                    dyn.Position = new Vector2(dyn.Position.X, Height - radius);

                    dyn.Velocity = new Vector2(
                        dyn.Velocity.X * friction,
                        -dyn.Velocity.Y * restitution
                    );

                    // 바닥에서 굴러가는 회전 속도
                    dyn.AngularVelocity = -dyn.Velocity.X / radius;

                    if (MathF.Abs(dyn.Velocity.X) < stopThreshold)
                        dyn.Velocity = new Vector2(0f, dyn.Velocity.Y);

                    if (MathF.Abs(dyn.Velocity.Y) < stopThreshold)
                        dyn.Velocity = new Vector2(dyn.Velocity.X, 0f);
                }
            }
        }
        #endregion
    }
}
