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
            set { Width = value; }
        }
        private uint mHeight;
        public uint Height
        {
            get { return mHeight; }
            set { Height = value; }
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
        #endregion

        #region 가지고 있는 개체들(component)
        private List<IObject2D> mObjects = new List<IObject2D>(1024);
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

        public void Start()
        {
            stopwatch.Start();
            lastTime = stopwatch.Elapsed.TotalSeconds;

            DinamicObject2D circle0 = new DinamicObject2D(new System.Numerics.Vector2(0, 0), new System.Numerics.Vector2(0, 0), new System.Numerics.Vector2(10, 10), 0f, EColliderType.CIRCLE);
            DinamicObject2D circle1 = new DinamicObject2D(new System.Numerics.Vector2(20, 20), new System.Numerics.Vector2(0, 0), new System.Numerics.Vector2(10, 10), 0f, EColliderType.CIRCLE);

            TryAddObject(circle0);
            TryAddObject(circle1);
        }

        public void Update()
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

        private void HandleCollisions()
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

                    if (distance < minDist && distance > 0.0001f)
                    {
                        Vector2 normal = Vector2.Normalize(delta);
                        float penetration = minDist - distance;

                        // 1. 위치 보정
                        Vector2 correction = normal * (penetration * 0.5f);
                        a.Position -= correction;
                        b.Position += correction;

                        // 2. 속도 반사/분리
                        Vector2 relativeVelocity = b.Velocity - a.Velocity;
                        float velocityAlongNormal = Vector2.Dot(relativeVelocity, normal);

                        // 이미 멀어지는 중이면 무시
                        if (velocityAlongNormal > 0) // 이게 없으면 계속해서 충돌이 발생할 수 있음
                            continue;

                        float restitution = 1.0f; // 1.0 = 완전 탄성, 0.0 = 비탄성

                        float impulseScalar = -(1 + restitution) * velocityAlongNormal;
                        impulseScalar /= 2.0f; // 질량이 둘 다 1이라고 가정

                        Vector2 impulse = impulseScalar * normal;

                        a.Velocity -= impulse;
                        b.Velocity += impulse;
                    }
                }
            }
        }

        private void HandleWorldBounds()
        {
            foreach (var obj in mObjects)
            {
                var dyn = obj as DinamicObject2D;
                if (dyn == null) continue;

                float radius = dyn.Collider.Size.X * 0.5f;
                float restitution = 0.8f;

                if (dyn.Position.X - radius < 0)
                {
                    dyn.Position = new Vector2(radius, dyn.Position.Y);
                    dyn.Velocity = new Vector2(-dyn.Velocity.X * restitution, dyn.Velocity.Y);
                }
                else if (dyn.Position.X + radius > Width)
                {
                    dyn.Position = new Vector2(Width - radius, dyn.Position.Y);
                    dyn.Velocity = new Vector2(-dyn.Velocity.X * restitution, dyn.Velocity.Y);
                }

                if (dyn.Position.Y - radius < 0)
                {
                    dyn.Position = new Vector2(dyn.Position.X, radius);
                    dyn.Velocity = new Vector2(dyn.Velocity.X, -dyn.Velocity.Y * restitution);
                }
                else if (dyn.Position.Y + radius > Height)
                {
                    dyn.Position = new Vector2(dyn.Position.X, Height - radius);
                    dyn.Velocity = new Vector2(dyn.Velocity.X, -dyn.Velocity.Y * restitution);
                }
            }
        }
        #endregion
    }
}
