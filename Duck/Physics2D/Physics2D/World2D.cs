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
        float DEFAULT_GRAVITY = 9.8f;
        #endregion
        #region 월드 속성들
        private Stopwatch stopwatch = new Stopwatch();
        private double lastTime = 0;

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

            DinamicObject2D circle0 = new DinamicObject2D(new System.Numerics.Vector2(30, 100), new System.Numerics.Vector2(0, 0), new System.Numerics.Vector2(30, 30), 0f, EColliderType.CIRCLE);
            DinamicObject2D circle1 = new DinamicObject2D(new System.Numerics.Vector2(35, 200), new System.Numerics.Vector2(0, 0), new System.Numerics.Vector2(30, 30), 0f, EColliderType.CIRCLE);

            TryAddObject(circle0);
            TryAddObject(circle1);
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

                    obj.Velocity += new System.Numerics.Vector2(0, -Gravity) * deltaTime; // 중력 가속도 적용
                }
            }

            // 개체 업데이트(이동)
            foreach (var obj in mObjects)
            {
                obj.Update(deltaTime);
            }

            //TODO: 충돌 처리
            HandleCollisions();

            //TODO: 월드 경계 처리
            HandleWorldBounds();

            //TODO: 기타 물리 계산


        }

        protected virtual void HandleCollisions()
        {
            for (int i = 0; i < mObjects.Count; i++)
            {
                for (int j = i + 1; j < mObjects.Count; j++)
                {
                    var a = mObjects[i] as DinamicObject2D;
                    var b = mObjects[j] as DinamicObject2D;

                    if (a == null || b == null)
                        continue;

                    if (a.Collider.Type != EColliderType.CIRCLE ||
                        b.Collider.Type != EColliderType.CIRCLE)
                        continue;

                    float radiusA = a.Collider.Size.X * 0.5f;
                    float radiusB = b.Collider.Size.X * 0.5f;

                    Vector2 delta = b.Position - a.Position;
                    float distance = delta.Length();
                    float minDist = radiusA + radiusB;

                    if (distance >= minDist || distance <= 0.0001f)
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
                }
            }
        }
        #endregion
    }
}
