/**
 * The MIT License (MIT)
 *
 * Copyright (c) 2012-2026 DragonBones team and other contributors
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Collections.Generic;

namespace DragonBones
{
    /// <internal/>
    /// <private/>
    internal abstract class Constraint : BaseObject
    {
        protected static readonly Matrix _helpMatrix = new Matrix();
        protected static readonly Point _helpPoint = new Point();

        /// <summary>
        /// - For timeline state.
        /// </summary>
        /// <internal/>
        internal ConstraintData _constraintData;
        protected Armature _armature;

        /// <summary>
        /// - For sort bones.
        /// </summary>
        /// <internal/>
        internal Bone _target;
        /// <summary>
        /// - For sort bones.
        /// </summary>
        /// <internal/>
        internal Bone _root;
        internal Bone _bone;

        protected override void _OnClear()
        {
            this._armature = null;
            this._target = null; //
            this._root = null; //
            this._bone = null; //
        }

        public abstract void Init(ConstraintData constraintData, Armature armature);
        public abstract void Update();
        public abstract void InvalidUpdate();

        public string name
        {
            get { return this._constraintData.name; }
        }
    }
    /// <internal/>
    /// <private/>
    internal class IKConstraint : Constraint
    {
        internal bool _scaleEnabled; // TODO
        /// <summary>
        /// - For timeline state.
        /// </summary>
        /// <internal/>
        internal bool _bendPositive;
        /// <summary>
        /// - For timeline state.
        /// </summary>
        /// <internal/>
        internal float _weight;

        protected override void _OnClear()
        {
            base._OnClear();

            this._scaleEnabled = false;
            this._bendPositive = false;
            this._weight = 1.0f;
            this._constraintData = null;
        }

        private void _ComputeA()
        {
            var ikGlobal = this._target.global;
            var global = this._root.global;
            var globalTransformMatrix = this._root.globalTransformMatrix;

            var radian = (float)Math.Atan2(ikGlobal.y - global.y, ikGlobal.x - global.x);
            if (global.scaleX < 0.0f)
            {
                radian += (float)Math.PI;
            }

            global.rotation += Transform.NormalizeRadian(radian - global.rotation) * this._weight;
            global.ToMatrix(globalTransformMatrix);
        }

        private void _ComputeB()
        {
            var boneLength = this._bone.boneData.length;
            var parent = this._root as Bone;
            var ikGlobal = this._target.global;
            var parentGlobal = parent.global;
            var global = this._bone.global;
            var globalTransformMatrix = this._bone.globalTransformMatrix;

            var x = globalTransformMatrix.a * boneLength;
            var y = globalTransformMatrix.b * boneLength;

            var lLL = x * x + y * y;
            var lL = (float)Math.Sqrt(lLL);

            var dX = global.x - parentGlobal.x;
            var dY = global.y - parentGlobal.y;
            var lPP = dX * dX + dY * dY;
            var lP = (float)Math.Sqrt(lPP);
            var rawRadian = global.rotation;
            var rawParentRadian = parentGlobal.rotation;
            var rawRadianA = (float)Math.Atan2(dY, dX);

            dX = ikGlobal.x - parentGlobal.x;
            dY = ikGlobal.y - parentGlobal.y;
            var lTT = dX * dX + dY * dY;
            var lT = (float)Math.Sqrt(lTT);

            var radianA = 0.0f;
            if (lL + lP <= lT || lT + lL <= lP || lT + lP <= lL)
            {
                radianA = (float)Math.Atan2(ikGlobal.y - parentGlobal.y, ikGlobal.x - parentGlobal.x);
                if (lL + lP <= lT)
                {
                }
                else if (lP < lL)
                {
                    radianA += (float)Math.PI;
                }
            }
            else
            {
                var h = (lPP - lLL + lTT) / (2.0f * lTT);
                var r = (float)Math.Sqrt(lPP - h * h * lTT) / lT;
                var hX = parentGlobal.x + (dX * h);
                var hY = parentGlobal.y + (dY * h);
                var rX = -dY * r;
                var rY = dX * r;

                var isPPR = false;
                var parentParent = parent.parent;
                if (parentParent != null)
                {
                    var parentParentMatrix = parentParent.globalTransformMatrix;
                    isPPR = parentParentMatrix.a * parentParentMatrix.d - parentParentMatrix.b * parentParentMatrix.c < 0.0f;
                }

                if (isPPR != this._bendPositive)
                {
                    global.x = hX - rX;
                    global.y = hY - rY;
                }
                else
                {
                    global.x = hX + rX;
                    global.y = hY + rY;
                }

                radianA = (float)Math.Atan2(global.y - parentGlobal.y, global.x - parentGlobal.x);
            }

            var dR = Transform.NormalizeRadian(radianA - rawRadianA);
            parentGlobal.rotation = rawParentRadian + dR * this._weight;
            parentGlobal.ToMatrix(parent.globalTransformMatrix);
            //
            var currentRadianA = rawRadianA + dR * this._weight;
            global.x = parentGlobal.x + (float)Math.Cos(currentRadianA) * lP;
            global.y = parentGlobal.y + (float)Math.Sin(currentRadianA) * lP;
            //
            var radianB = (float)Math.Atan2(ikGlobal.y - global.y, ikGlobal.x - global.x);
            if (global.scaleX < 0.0f)
            {
                radianB += (float)Math.PI;
            }

            global.rotation = parentGlobal.rotation + rawRadian - rawParentRadian + Transform.NormalizeRadian(radianB - dR - rawRadian) * this._weight;
            global.ToMatrix(globalTransformMatrix);
        }

        public override void Init(ConstraintData constraintData, Armature armature)
        {
            if (this._constraintData != null)
            {
                return;
            }

            this._constraintData = constraintData;
            this._armature = armature;
            this._target = this._armature.GetBone(this._constraintData.target.name);
            this._root = this._armature.GetBone(this._constraintData.root.name);
            this._bone = this._constraintData.bone != null ? this._armature.GetBone(this._constraintData.bone.name) : null;

            {
                var ikConstraintData = this._constraintData as IKConstraintData;
                //
                this._scaleEnabled = ikConstraintData.scaleEnabled;
                this._bendPositive = ikConstraintData.bendPositive;
                this._weight = ikConstraintData.weight;
            }

            this._root._hasConstraint = true;
        }

        public override void Update()
        {
            this._root.UpdateByConstraint();

            if (this._bone != null)
            {
                this._bone.UpdateByConstraint();
                this._ComputeB();
            }
            else
            {
                this._ComputeA();
            }
        }

        public override void InvalidUpdate()
        {
            this._root.InvalidUpdate();

            if (this._bone != null)
            {
                this._bone.InvalidUpdate();
            }
        }
    }

    /// <internal/>
    /// <private/>
    internal class TransformConstraint : Constraint
    {
        protected static readonly Matrix _helpMatrix1 = new Matrix();
        protected static readonly Matrix _helpMatrix2 = new Matrix();
        protected static readonly Transform _helpTransform = new Transform();
        internal bool _dirty = true;

        public int index = -1;

        /**
         * - For timeline state.
         * @internal
         */
        public float _rotateWeight = 0.0f;
        /**
         * - For timeline state.
         * @internal
         */
        public float _scaleWeight = 0.0f;
        /**
         * - For timeline state.
         * @internal
         */
        public float _translateWeight = 0.0f;

        protected override void _OnClear()
        {
            base._OnClear();

            this._constraintData = null;
            this.index = -1;
        }


        public override void Init(ConstraintData constraintData, Armature armature)
        {
            if (this._constraintData != null)
            {
                return;
            }

            if (this.index < 0)
            {
                return;
            }
            this._constraintData = constraintData;
            this._armature = armature;
            this._target = this._armature.GetBone(this._constraintData.target.name);
            var transformConstraintData = this._constraintData as TransformConstraintData;
            var boneName = transformConstraintData.bones[this.index].name;
            this._bone = this._armature.GetBone(boneName);
            if (this._bone == null)
            {
                return;
            }
            this._rotateWeight = transformConstraintData.rotateWeight;
            this._scaleWeight = transformConstraintData.scaleWeight;
            this._translateWeight = transformConstraintData.translateWeight;

            this._root = this._bone;
            this._root._transformConstraint = this;
            this._target.AddTargetTransformConstraint(this);
        }

        public override void Update()
        {
            if (!this._dirty)
            {
                return;
            }
            if (this._root == null || this._target == null || this._bone == null)
            {
                return;
            }

            if (this._bone != null)
            {
                this._Compute();
            }
            this._dirty = false;
        }

        private void _Compute()
        {
            if (this._target != null && this._root != null)
            {
                if (this._scaleWeight == 0 && this._rotateWeight == 0 && this._translateWeight == 0)
                {
                    return;
                }
                TransformConstraintData transformConstraintData = this._constraintData as TransformConstraintData;
                if (transformConstraintData == null)
                {
                    return;
                }
                var offsetX = transformConstraintData.offsetX;
                var offsetY = transformConstraintData.offsetY;
                var offsetRotation = transformConstraintData.offsetRotation;
                var offsetScaleX = transformConstraintData.offsetScaleX;
                var offsetScaleY = transformConstraintData.offsetScaleY;
                var local = transformConstraintData.local;

                var globalTransformMatrix = this._root.globalTransformMatrix;
                this._root.global.FromMatrix(globalTransformMatrix, true);
                if (local)
                {
                    Matrix parentMatrix = this._target.globalTransformMatrix;
                    Matrix localMatrix = TransformConstraint._helpMatrix1.CopyFrom(parentMatrix);
                    if (this._target.parent != null)
                    {
                        Matrix grandMatrix = TransformConstraint._helpMatrix2.CopyFrom(this._target.parent.globalTransformMatrix);
                        Matrix p = grandMatrix.Invert();
                        localMatrix.Concat(p);
                    }
                    Transform localTransform = TransformConstraint._helpTransform.FromMatrix(localMatrix);
                    if (this._translateWeight != 0)
                    {
                        this._root.global.x = this._root.global.x * (1 - this._translateWeight) + (localTransform.x + offsetX) * this._translateWeight;
                        this._root.global.y = this._root.global.y * (1 - this._translateWeight) + (localTransform.y + offsetY) * this._translateWeight;
                    }
                    if (this._rotateWeight != 0)
                    {
                        this._root.global.rotation = this._root.global.rotation * (1 - this._rotateWeight) + (localTransform.rotation + offsetRotation) * this._rotateWeight;
                    }
                    if (this._scaleWeight != 0)
                    {
                        this._root.global.scaleX = this._root.global.scaleX * (1 - this._scaleWeight) + (localTransform.scaleX + offsetScaleX) * this._scaleWeight;
                        this._root.global.scaleY = this._root.global.scaleY * (1 - this._scaleWeight) + (localTransform.scaleY + offsetScaleY) * this._scaleWeight;
                    }
                    this._root.global.ToMatrix(globalTransformMatrix);
                }
                else
                {

                    var targetGlobalTransform = TransformConstraint._helpTransform.FromMatrix(this._target.globalTransformMatrix, true);
                    if (this._translateWeight != 0)
                    {
                        if(offsetX != 0 || offsetY != 0) {
                            TransformConstraint._helpMatrix1.CopyFrom(this._target.globalTransformMatrix);
                            TransformConstraint._helpMatrix2.Identity();
                            TransformConstraint._helpMatrix2.tx = offsetX;
                            TransformConstraint._helpMatrix2.ty = offsetY;
                            TransformConstraint._helpMatrix2.Concat(TransformConstraint._helpMatrix1);
                            this._root.global.x = this._root.global.x * (1 - this._translateWeight) + (TransformConstraint._helpMatrix2.tx) * this._translateWeight;
                            this._root.global.y = this._root.global.y * (1 - this._translateWeight) + (TransformConstraint._helpMatrix2.ty) * this._translateWeight;
                        }
                        else {
                            this._root.global.x = this._root.global.x * (1 - this._translateWeight) + (targetGlobalTransform.x) * this._translateWeight;
                            this._root.global.y = this._root.global.y * (1 - this._translateWeight) + (targetGlobalTransform.y) * this._translateWeight;
                        }
                        
                    }
                    if (this._rotateWeight != 0)
                    {
                        var targetRotate = 0.0f;
                        if(DragonBones.yDown)
                        {
                            targetRotate = Transform.NormalizeRadian(targetGlobalTransform.rotation + offsetRotation);
                        }
                        else
                        {
                            targetRotate = Transform.NormalizeRadian(targetGlobalTransform.rotation - offsetRotation);
                        }
                        this._root.global.rotation = this._root.global.rotation * (1 - this._rotateWeight) + (targetRotate) * this._rotateWeight;
                    }
                    if (this._scaleWeight != 0)
                    {
                        this._root.global.scaleX = this._root.global.scaleX * (1 - this._scaleWeight) + (targetGlobalTransform.scaleX + offsetScaleX) * this._scaleWeight;
                        this._root.global.scaleY = this._root.global.scaleY * (1 - this._scaleWeight) + (targetGlobalTransform.scaleY + offsetScaleY) * this._scaleWeight;
                    }
                    this._root.global.ToMatrix(globalTransformMatrix);
                }
                
            }
        }

        public override void InvalidUpdate()
        {
            if (this._root != null)
            {
                this._root.InvalidUpdate();
            }
        }
    }

    /// <internal/>
    /// <private/>
    internal class PhysicsConstraint : Constraint
    {
        /**
         * - For timeline state.
         * @internal
         */
        public float _inertia;
        /**
         * - For timeline state.
         * @internal
         */
        public float _strength;
        /**
         * - For timeline state.
         * @internal
         */
        public float _damping;
        /**
         * - For timeline state.
         * @internal
         */
        public float _mass;
        /**
         * - For timeline state.
         * @internal
         */
        public float _wind;

        /**
         * - For timeline state.
         * @internal
         */
        public float _windDisturbance;

        /**
         * - For timeline state.
         * @internal
         */
        public float _gravity;
        /**
         * - For timeline state.
         * @internal
         */
        public float _weight;
        /**
         * - For timeline state.
         * @internal
         */
        public bool _reset;

        public bool _sleeping = false;

        private float _massInverse = 0;
        private float _fpsTime = 0;
        private float _dump = 0;

        private float _remaining = 0;
        private float _lastTime = 0;
        private float _xOffset = 0;
        private float _xVelocity = 0;
        private float _yOffset = 0;
        private float _yVelocity = 0;
        private float _rotateOffset = 0;
        private float _rotateVelocity = 0;
        private float _scaleOffset = 0;
        private float _scaleVelocity = 0;
        private bool _lastReset = false;
        private float _by = 0;
        private float _bx = 0;
        private float _cx = 0;
        private float _cy = 0;
        private float _tx = 0;
        private float _ty = 0;
        private float PI2 = Transform.PI_D;
        private float PI1_2 = 1 / Transform.PI_D;
        protected override void _OnClear()
        {
            base._OnClear();

            this._constraintData = null;
        }

        public override void Init(ConstraintData constraintData, Armature armature)
        {
            if (this._constraintData != null)
            {
                return;
            }
            this._constraintData = constraintData;
            this._armature = armature;
            this._target = this._armature.GetBone(this._constraintData.target.name);
            if (this._target != null)
            {
                this._target._physicsConstraint = this;
            }
            var physicsData = this._constraintData as PhysicsConstraintData;

            this._inertia = physicsData.inertia;
            this._strength = physicsData.strength;
            this._damping = physicsData.damping;
            this._mass = physicsData.mass;
            this._wind = physicsData.wind;
            this._windDisturbance = physicsData.windDisturbance;
            this._gravity = physicsData.gravity;
            this._weight = physicsData.weight;


            this._fpsTime = 1.0f / physicsData.fps;
            this._massInverse = 1 / this._mass;
            this._dump = 1 - this._damping;
            this._reset = true;
        }

        private bool isNumberEqual(float a, float b, float acc = 0.001f)
        {
            if (a + acc >= b && a - acc <= b)
            {
                return true;
            }
            return false;
        }

        private bool isNumberEqual(double a, double b, double acc = 0.001f)
        {
            if (a + acc >= b && a - acc <= b)
            {
                return true;
            }
            return false;
        }

        public override void Update()
        {
            if (this._sleeping)
            {
                return;
            }
            var physicsData = this._constraintData as PhysicsConstraintData;
            if (this._armature.clock != null)
            {
                var bone = this._target;
                var hasX = physicsData.x > 0;
                var hasY = physicsData.y > 0;
                var hasRotate = physicsData.rotate > 0 || physicsData.shearX > 0;
                var hasScaleX = physicsData.scaleX > 0;
                var boneLength = bone.boneData.length;
                System.Random random = new System.Random();
                double randomValue = random.NextDouble(); // 0.0 <= x < 1.0
                float randomFloat = (float)randomValue;
                var windDisturbance = (randomFloat * 2 - 1) * this._windDisturbance;

                if (this._reset)
                {
                    this.Reset();
                    this._reset = false;
                    this._lastReset = true;
                }
                else
                {

                    this._sleeping = true;
                    // 过去的时间，单位是秒
                    var globalTime = this._armature.clock.time;
                    var delta = Math.Max(globalTime - this._lastTime, 0);

                    // 参与计算的时间，单位是秒
                    this._remaining += delta;
                    this._lastTime = globalTime;
                    // console.log('start bone.globalTransform.x', bone.globalTransform.x)
                    var bx = bone.global.x;
                    var by = bone.global.y;

                    if (this._lastReset)
                    {
                        this._lastReset = false;
                        this._bx = bx;
                        this._by = by;
                    }
                    else
                    {
                        var armatureReferenceScale = 100;
                        var armatureScaleX = 1;
                        var armatureScaleY = 1;
                        var armatureYDown = DragonBones.yDown;
                        var remaining = this._remaining;
                        var armatureScale = armatureReferenceScale;
                        var damping = -1.0f;
                        var qx = physicsData.limit * delta;
                        var qy = qx * Math.Abs(armatureScaleX);
                        qx *= Math.Abs(armatureScaleY);
                        if (hasX || hasY)
                        {
                            if (hasX)
                            {
                                // 在惯性下，偏移会继续增加一点，这个增加的值是上一帧的偏移和这一帧的偏移的差值
                                var u = (this._bx - bx) * this._inertia;
                                // 惯性有个最大值
                                this._xOffset += u > qx ? qx : u < -qx ? -qx : u;
                                // 每次迭代只和上一帧的偏移有关
                                this._bx = bx;
                            }
                            if (hasY)
                            {
                                var u = (this._by - by) * this._inertia;
                                this._yOffset += u > qy ? qy : u < -qy ? -qy : u;
                                this._by = by;
                            }
                            if (remaining >= this._fpsTime)
                            {
                                damping = this._dump;
                                var w = (this._wind + windDisturbance) * armatureScale * armatureScaleX;
                                var g = this._gravity * armatureScale * armatureScaleY;
                                if (this._fpsTime <= 0)
                                {
                                    return;
                                }
                                do
                                {
                                    //物理fps
                                    if (hasX)
                                    {
                                        // 力 = 弹簧力 + 风力
                                        var deltaV = (w - this._xOffset * this._strength) * this._massInverse * this._fpsTime;
                                        this._xVelocity += deltaV;
                                        var deltaOffset = this._xVelocity * this._fpsTime;
                                        this._xOffset += deltaOffset;
                                        this._xVelocity *= damping;
                                    }
                                    if (hasY)
                                    {
                                        this._yVelocity += (g - this._yOffset * this._strength) * this._massInverse * this._fpsTime;
                                        this._yOffset += this._yVelocity * this._fpsTime;
                                        this._yVelocity *= damping;
                                    }
                                    remaining -= this._fpsTime;
                                } while (remaining >= this._fpsTime);
                            }
                            if (hasX)
                            {
                                // 偏移要乘以物理的权重，再乘以x的权重
                                var offsetX = this._xOffset * this._weight * physicsData.x;
                                if (Math.Abs(offsetX) > physicsData.limit)
                                {
                                    // 防止过大
                                    offsetX = offsetX > 0 ? physicsData.limit : -physicsData.limit;
                                }
                                bone.global.x += offsetX;
                                if (!this.isNumberEqual(offsetX, 0))
                                {
                                    this._sleeping = false;
                                }
                            }
                            if (hasY)
                            {

                                var offsetY = this._yOffset * this._weight * physicsData.y;
                                if (Math.Abs(offsetY) > physicsData.limit)
                                {
                                    // 防止过大
                                    offsetY = offsetY > 0 ? physicsData.limit : -physicsData.limit;
                                }
                                bone.global.y += offsetY;
                                if (!this.isNumberEqual(offsetY, 0))
                                {
                                    this._sleeping = false;
                                }
                            }
                        }
                        if (hasRotate || hasScaleX)
                        {
                            var globalMatrix1 = bone.globalTransformMatrix;
                            float ca = (float)Math.Atan2(globalMatrix1.c, globalMatrix1.a);
                            var c = 0.0f;
                            var s = 0.0f;
                            var mr = 0.0f;
                            var dx = this._cx - bone.global.x;
                            var dy = this._cy - bone.global.y;
                            if (dx > qx)
                                dx = qx;
                            else if (dx < -qx) //
                                dx = -qx;
                            if (dy > qy)
                                dy = qy;
                            else if (dy < -qy) //
                                dy = -qy;
                            if (hasRotate)
                            {
                                mr = (physicsData.rotate + physicsData.shearX) * this._weight;
                                float r = ((float)Math.Atan2(dy + this._ty, dx + this._tx)) - ca - this._rotateOffset * mr;
                                // r 限制在 -pi到pi之间；
                                r = (r - (float)Math.Ceiling(r * this.PI1_2 - 0.5) * this.PI2);
                                float inertiaRotateOffset = r * this._inertia;
                                if (!this.isNumberEqual(inertiaRotateOffset, 0.0f, 0.0001f))
                                {
                                    this._sleeping = false;
                                }
                                // 旋转的偏移收到惯性的影响
                                this._rotateOffset += inertiaRotateOffset;
                                r = this._rotateOffset * mr + ca;
                                c = (float)Math.Cos(r);
                                s = (float)Math.Sin(r);
                                if (hasScaleX)
                                {
                                    r = boneLength * bone.global.scaleX;
                                    if (r > 0)
                                    {
                                        this._scaleOffset += (dx * c + dy * s) * this._inertia / r;
                                    }
                                }
                            }
                            else
                            {
                                c = (float)Math.Cos(ca);
                                s = (float)Math.Sin(ca);
                                var r = boneLength * bone.global.scaleX;
                                if (r > 0)
                                {
                                    this._scaleOffset += (dx * c + dy * s) * this._inertia / r;
                                }
                            }
                            remaining = this._remaining;
                            if (remaining >= this._fpsTime)
                            {
                                if (damping == -1)
                                {
                                    damping = this._dump;
                                }
                                var mass = this._massInverse * this._fpsTime;
                                var strength = this._strength;
                                var wind = this._wind + windDisturbance;
                                var gravity = (armatureYDown ? -this._gravity : this._gravity);
                                var boneLen = boneLength;
                                if (this._fpsTime <= 0)
                                {
                                    return;
                                }
                                while (true)
                                {
                                    remaining -= this._fpsTime;
                                    if (hasScaleX)
                                    {
                                        this._scaleVelocity += (wind * c - gravity * s - this._scaleOffset * strength) * mass;
                                        this._scaleOffset += this._scaleVelocity * this._fpsTime;
                                        this._scaleVelocity *= damping;
                                    }
                                    if (hasRotate)
                                    {
                                        this._rotateVelocity -= ((wind * s + gravity * c) * boneLen + this._rotateOffset * strength) * mass;
                                        // 旋转的偏移受到力作用
                                        this._rotateOffset += this._rotateVelocity * this._fpsTime;
                                        if (Math.Abs(this._rotateOffset) > this.PI2)
                                        {
                                            // 旋转的偏移限制在 -2pi到2pi之间；
                                            this._rotateOffset = this._rotateOffset > 0 ? this.PI2 : -this.PI2;
                                        }
                                        if (!this.isNumberEqual(this._rotateVelocity, 0.0f, 0.0001f))
                                        {
                                            this._sleeping = false;
                                        }
                                        this._rotateVelocity *= damping;
                                        if (remaining < this._fpsTime)
                                        {
                                            break;
                                        }
                                        var r = this._rotateOffset * mr + ca;
                                        c = (float)Math.Cos(r);
                                        s = (float)Math.Sin(r);
                                    }
                                    else if (remaining < this._fpsTime) //
                                        break;
                                }
                            }
                        }
                        this._remaining = remaining;
                    }
                    this._cx = bone.global.x;
                    this._cy = bone.global.y;
                }
                var globalMatrix = this._target.globalTransformMatrix;
                if (hasRotate)
                {
                    var o = this._rotateOffset * this._weight;
                    var s = 0.0f;
                    var c = 0.0f;
                    var a = 0.0f;
                    if (physicsData.shearX > 0)
                    {
                        var r = 0.0f;
                        if (physicsData.rotate > 0)
                        {
                            r = o * physicsData.rotate;
                            s = (float)Math.Sin(r);
                            c = (float)Math.Cos(r);
                            a = globalMatrix.b;
                            globalMatrix.b = c * a - s * globalMatrix.d;
                            globalMatrix.d = s * a + c * globalMatrix.d;
                        }
                        r += o * physicsData.shearX;
                        s = (float)Math.Sin(r);
                        c = (float)Math.Cos(r);
                        a = globalMatrix.a;
                        globalMatrix.a = c * a - s * globalMatrix.c;
                        globalMatrix.c = s * a + c * globalMatrix.c;
                    }
                    else
                    {
                        o *= physicsData.rotate;
                        s = (float)Math.Sin(o);
                        c = (float)Math.Cos(o);
                        a = globalMatrix.a;
                        globalMatrix.a = c * a - s * globalMatrix.c;
                        globalMatrix.c = s * a + c * globalMatrix.c;
                        a = globalMatrix.b;
                        globalMatrix.b = c * a - s * globalMatrix.d;
                        globalMatrix.d = s * a + c * globalMatrix.d;
                    }
                }
                if (hasScaleX)
                {
                    var s = 1 + this._scaleOffset * this._weight * physicsData.scaleX;
                    globalMatrix.a *= s;
                    globalMatrix.c *= s;
                }
                this._tx = boneLength * globalMatrix.a;
                this._ty = boneLength * globalMatrix.c;
                globalMatrix.tx = bone.global.x;
                globalMatrix.ty = bone.global.y;

                var oldRotation = bone.global.rotation;
                var oldSkew = bone.global.skew;

                bone.global.FromMatrix(globalMatrix);
                if (!this.isNumberEqual(oldRotation, bone.global.rotation)
                    || (!this.isNumberEqual(oldSkew, bone.global.skew)))
                {
                    this._sleeping = false;
                }

            }
        }
        private void Reset()
        {
            this._remaining = 0;
            this._lastTime = this._armature.clock != null ? this._armature.clock.time : 0;
            this._xOffset = 0;
            this._xVelocity = 0;
            this._yOffset = 0;
            this._yVelocity = 0;
            this._rotateOffset = 0;
            this._rotateVelocity = 0;
            this._scaleOffset = 0;
            this._scaleVelocity = 0;
        }

        public override void InvalidUpdate()
        {
            if (this._root != null)
            {
                this._root.InvalidUpdate();
            }
        }

    }

    /// <internal/>
    /// <private/>
    internal class PathConstraint : Constraint
    {
        public bool dirty;
        public float pathOffset;
        public float position;
        public float spacing;
        public float rotateOffset;
        public float rotateWeight;
        public float xWeight;
        public float yWeight;

        private Slot _pathSlot;
        private List<Bone> _bones = new List<Bone>();

        private List<float> _spaces = new List<float>();
        private List<float> _positions = new List<float>();
        private List<float> _curves = new List<float>();
        private List<float> _boneLengths = new List<float>();

        private List<float> _pathGlobalVertices = new List<float>();
        private List<float> _segments = new List<float>{10};

        protected override void _OnClear()
        {
            base._OnClear();

            this.dirty = false;
            this.pathOffset = 0;

            this.position = 0.0f;
            this.spacing = 0.0f;
            this.rotateOffset = 0.0f;
            this.rotateWeight = 1.0f;
            this.xWeight = 1.0f;
            this.yWeight = 1.0f;

            this._pathSlot = null;
            this._bones.Clear();

            this._spaces.Clear();
            this._positions.Clear();
            this._curves.Clear();
            this._boneLengths.Clear();

            this._pathGlobalVertices.Clear();
        }

        protected void _UpdatePathVertices(VerticesData verticesData, DeformVertices displayFrame)
        {
            //计算曲线的节点数据
            var armature = this._armature;
            var dragonBonesData = armature.armatureData.parent;
            var scale = armature.armatureData.scale;
            var intArray = dragonBonesData.intArray;
            var floatArray = dragonBonesData.floatArray;

            var pathOffset = verticesData.offset;
            var pathVertexCount = intArray[pathOffset + (int)BinaryOffset.GeometryVertexCount];
            var pathVertexOffset = intArray[pathOffset + (int)BinaryOffset.GeometryFloatOffset];

            // this._pathGlobalVertices.length = pathVertexCount * 2;
            var pathGlobalVerticesLength = pathVertexCount * 2;
            this._pathGlobalVertices.ResizeList(pathGlobalVerticesLength);

            var weightData = verticesData.weight;
            //没有骨骼约束我,那节点只受自己的Bone控制
            if (weightData == null) {
                var parentBone = this._pathSlot.parent;
                parentBone.UpdateByConstraint();

                var matrix = parentBone.globalTransformMatrix;
                if (displayFrame != null && displayFrame.vertices != null && displayFrame.vertices.Count == pathVertexCount * 2) {
                    // 有path形变动画
                    var deformVertices = displayFrame.vertices;
                    var i = 0;
                    var iV = pathVertexOffset;
                    for (i = 0, iV = pathVertexOffset; i < pathVertexCount * 2; i += 2)
                    {
                        var vx = (floatArray[iV++] + deformVertices[i]) * scale;
                        var vy = (floatArray[iV++] + deformVertices[i + 1]) * scale;

                        var x = matrix.a * vx + matrix.c * vy + matrix.tx;
                        var y = matrix.b * vx + matrix.d * vy + matrix.ty;

                        //
                        this._pathGlobalVertices[i] = x;
                        this._pathGlobalVertices[i + 1] = y;
                    }
                }
                else {
                    var i = 0;
                    var iV = pathVertexOffset;
                    for (i = 0, iV = pathVertexOffset; i < pathVertexCount * 2; i += 2)
                    {
                        var vx = floatArray[iV++] * scale;
                        var vy = floatArray[iV++] * scale;

                        var x = matrix.a * vx + matrix.c * vy + matrix.tx;
                        var y = matrix.b * vx + matrix.d * vy + matrix.ty;

                        //
                        this._pathGlobalVertices[i] = x;
                        this._pathGlobalVertices[i + 1] = y;
                    }
                }
                
                return;
            }
            else {
                // TODO: path 可以被骨骼绑定。有骨骼约束我,那我的节点受骨骼权重控制
                var bones = this._pathSlot._deformVertices.bones;
                var weightBoneCount = weightData.bones.Count;
                List<float> deformVertices = null;
                if (displayFrame != null)
                {
                    deformVertices = displayFrame.vertices;
                }
                bool hasDeform = false;
                if (deformVertices != null)
                {
                    hasDeform = deformVertices.Count > 0;
                }
                var weightOffset = weightData.offset;
                var floatOffset = intArray[weightOffset + (int)BinaryOffset.WeigthFloatOffset];

                var iV = floatOffset;
                var iB = weightOffset + (int)BinaryOffset.WeigthBoneIndices + weightBoneCount;
                var iF = 0;
                var i = 0;
                var iW = 0;
                for (i = 0; i < pathVertexCount; i++)
                {
                    var vertexBoneCount = intArray[iB++]; //

                    var xG = 0.0f;
                    var yG = 0.0f;
                    var ii = 0;
                    var ll = vertexBoneCount;
                    for (ii = 0; ii < ll; ii++)
                    {
                        var boneIndex = intArray[iB++];
                        var bone = bones[boneIndex];
                        if (bone == null)
                        {
                            continue;
                        }

                        var matrix = bone.globalTransformMatrix;
                        var weight = floatArray[iV++];
                        var vx = floatArray[iV++] * scale;
                        var vy = floatArray[iV++] * scale;
                        if (hasDeform && deformVertices != null)
                        {
                            vx += deformVertices[iF++];
                            vy += deformVertices[iF++];
                        }
                        xG += (matrix.a * vx + matrix.c * vy + matrix.tx) * weight;
                        yG += (matrix.b * vx + matrix.d * vy + matrix.ty) * weight;
                    }

                    this._pathGlobalVertices[iW++] = xG;
                    this._pathGlobalVertices[iW++] = yG;
                }
            }

            
        }

        protected void _ComputeVertices(int start, int count, int offset, List<float> out1)
        {
            //TODO优化
            int i = offset;
            int iW = start;
            for (i = offset, iW = start; i < count; i += 2)
            {
                out1[i] = this._pathGlobalVertices[iW++];
                out1[i + 1] = this._pathGlobalVertices[iW++];
            }
        }

        protected void _ComputeBezierCurve(PathDisplayData pathDisplayDta , int spaceCount, bool tangents, bool percentPosition, bool percentSpacing)
        {
            //计算当前的骨骼在曲线上的位置
            var armature = this._armature;
            var intArray = armature.armatureData.parent.intArray;
            var vertexCount = intArray[pathDisplayDta.vertices.offset + (int)BinaryOffset.GeometryVertexCount];

            var positions = this._positions;
            var spaces = this._spaces;
            var isClosed = pathDisplayDta.closed;
            var curveVertices = new List<float>();
            var verticesLength = vertexCount * 2;
            var curveCount = verticesLength / 6;
            var preCurve = -1;
            var position = this.position;

            positions.ResizeList(spaceCount * 3 + 2);
            // 最后一个就是整个曲线的长度
            var pathLength = pathDisplayDta.curveLengths[pathDisplayDta.curveLengths.Count - 1];
            //不需要匀速运动，效率高些
            var curve = 0;
            if (!pathDisplayDta.constantSpeed)
            {
                var lengths = pathDisplayDta.curveLengths;
                curveCount -= isClosed ? 1 : 1;
                if (percentPosition)
                {
                    position *= pathLength;
                }

                if (percentSpacing)
                {
                    for (var i1 = 0; i1 < spaceCount; i1++)
                    {
                        spaces[i1] *= pathLength;
                    }
                }

                curveVertices.ResizeList(10);
                var o2 = 0;

                for (var i2 = 0; i2 < spaceCount; i2++, o2 += 3)
                {
                    var space = spaces[i2];
                    position += space;
                    var percent = 0.0f;

                    if (isClosed)
                    {
                        position %= pathLength;
                        if (position < 0)
                        {
                            position += pathLength;
                        }
                        curve = 0;
                    }
                    if (position < 0)
                    {
                        percent = position / pathLength;
                        curve = 0;
                    }
                    else if (position > pathLength)
                    {
                        percent = (position) / pathLength;
                        curve = curveCount - 1;
                    }
                    else
                    {
                        for (; ; curve++)
                        {
                            var len = lengths[curve];
                            if (position > len)
                            {
                                continue;
                            }
                            if (curve == 0)
                            {
                                percent = position / len;
                            }
                            else
                            {
                                var preLen = lengths[curve - 1];
                                percent = (position - preLen) / (len - preLen);
                            }
                            break;
                        }
                    }
                    if (curve != preCurve)
                    {
                        preCurve = curve;
                        if (isClosed && curve == curveCount)
                        {
                            //计算是哪段曲线
                            this._ComputeVertices(verticesLength - 4, 4, 0, curveVertices);
                            this._ComputeVertices(0, 10, 4, curveVertices);
                        }
                        else
                        {
                            this._ComputeVertices(curve * 6 + 2, 10, 0, curveVertices);
                        }
                    }
                    // 计算曲线上点的位置和斜率

                    this.AddCurvePosition2(percent, curveVertices, positions, o2, tangents, pathLength);
                }

                return;
            }
            //匀速的
            if (isClosed) {
                verticesLength += 2;
                // curveVertices.length = vertexCount;
                curveVertices.ResizeList(vertexCount);
                this._ComputeVertices(2, verticesLength - 4, 0, curveVertices);
                this._ComputeVertices(0, 2, verticesLength - 4, curveVertices);

                curveVertices[verticesLength - 2] = curveVertices[0];
                curveVertices[verticesLength - 1] = curveVertices[1];
            }
            else {
                curveCount--;
                verticesLength -= 4;
                // curveVertices.length = verticesLength;
                curveVertices.ResizeList(verticesLength);
                this._ComputeVertices(2, verticesLength, 0, curveVertices);
            }
            //
            List<float> curves = new List<float>(curveCount);
            pathLength = 0;
            var x1 = curveVertices[0];
            var y1 = curveVertices[1];
            float cx1 = 0.0f;
            float cy1 = 0.0f;
            float cx2 = 0.0f;
            float cy2 = 0.0f;
            float x2 = 0.0f;
            float y2 = 0.0f;
            float tmpx = 0.0f;
            float tmpy = 0.0f;
            float dddfx = 0.0f;
            float dddfy = 0.0f;
            float ddfx = 0.0f;
            float ddfy = 0.0f;
            float dfx = 0.0f;
            float dfy = 0.0f;

            int i = 0;
            int w = 2;
            for (i = 0, w = 2; i < curveCount; i++, w += 6) {
                cx1 = curveVertices[w];
                cy1 = curveVertices[w + 1];
                cx2 = curveVertices[w + 2];
                cy2 = curveVertices[w + 3];
                x2 = curveVertices[w + 4];
                y2 = curveVertices[w + 5];
                tmpx = (x1 - cx1 * 2 + cx2) * 0.1875f;
                tmpy = (y1 - cy1 * 2 + cy2) * 0.1875f;
                dddfx = ((cx1 - cx2) * 3 - x1 + x2) * 0.09375f;
                dddfy = ((cy1 - cy2) * 3 - y1 + y2) * 0.09375f;
                ddfx = tmpx * 2 + dddfx;
                ddfy = tmpy * 2 + dddfy;
                dfx = (cx1 - x1) * 0.75f + tmpx + dddfx * 0.16666667f;
                dfy = (cy1 - y1) * 0.75f + tmpy + dddfy * 0.16666667f;
                pathLength += (float)Math.Sqrt(dfx * dfx + dfy * dfy);
                dfx += ddfx;
                dfy += ddfy;
                ddfx += dddfx;
                ddfy += dddfy;
                pathLength += (float)Math.Sqrt(dfx * dfx + dfy * dfy);
                dfx += ddfx;
                dfy += ddfy;
                pathLength += (float)Math.Sqrt(dfx * dfx + dfy * dfy);
                dfx += ddfx + dddfx;
                dfy += ddfy + dddfy;
                pathLength += (float)Math.Sqrt(dfx * dfx + dfy * dfy);
                curves[i] = pathLength;
                x1 = x2;
                y1 = y2;
            }

            if (percentPosition) {
                position *= pathLength;
            }
            if (percentSpacing) {
                var i3 = 0;
                for (i3 = 0; i3 < spaceCount; i3++)
                {
                    spaces[i3] *= pathLength;
                }
            }

            var segments = this._segments;
            float curveLength = 0;
            int i5 = 0;
            int o = 0;
            curve = 0;
            int segment = 0;
            for (; i5 < spaceCount; i5++, o += 3) {
                var space = spaces[i5];
                position += space;
                var p = position;

                if (isClosed) {
                    p %= pathLength;
                    if (p < 0) p += pathLength;
                    curve = 0;
                } else if (p < 0) {
                    continue;
                } else if (p > pathLength) {
                    continue;
                }

                // Determine curve containing position.
                for (; ; curve++) {
                    var length = curves[curve];
                    if (p > length) continue;
                    if (curve == 0)
                        p /= length;
                    else {
                        var prev = curves[curve - 1];
                        p = (p - prev) / (length - prev);
                    }
                    break;
                }

                if (curve != preCurve) {
                    preCurve = curve;
                    var ii = curve * 6;
                    x1 = curveVertices[ii];
                    y1 = curveVertices[ii + 1];
                    cx1 = curveVertices[ii + 2];
                    cy1 = curveVertices[ii + 3];
                    cx2 = curveVertices[ii + 4];
                    cy2 = curveVertices[ii + 5];
                    x2 = curveVertices[ii + 6];
                    y2 = curveVertices[ii + 7];
                    tmpx = (x1 - cx1 * 2 + cx2) * 0.03f;
                    tmpy = (y1 - cy1 * 2 + cy2) * 0.03f;
                    dddfx = ((cx1 - cx2) * 3 - x1 + x2) * 0.006f;
                    dddfy = ((cy1 - cy2) * 3 - y1 + y2) * 0.006f;
                    ddfx = tmpx * 2 + dddfx;
                    ddfy = tmpy * 2 + dddfy;
                    dfx = (cx1 - x1) * 0.3f + tmpx + dddfx * 0.16666667f;
                    dfy = (cy1 - y1) * 0.3f + tmpy + dddfy * 0.16666667f;
                    curveLength = (float)Math.Sqrt(dfx * dfx + dfy * dfy);
                    segments[0] = curveLength;
                    for (ii = 1; ii < 8; ii++) {
                        dfx += ddfx;
                        dfy += ddfy;
                        ddfx += dddfx;
                        ddfy += dddfy;
                        curveLength += (float)Math.Sqrt(dfx * dfx + dfy * dfy);
                        segments[ii] = curveLength;
                    }
                    dfx += ddfx;
                    dfy += ddfy;
                    curveLength += (float)Math.Sqrt(dfx * dfx + dfy * dfy);
                    segments[8] = curveLength;
                    dfx += ddfx + dddfx;
                    dfy += ddfy + dddfy;
                    curveLength += (float)Math.Sqrt(dfx * dfx + dfy * dfy);
                    segments[9] = curveLength;
                    segment = 0;
                }

                // Weight by segment length.
                p *= curveLength;
                for (; ; segment++) {
                    var length = segments[segment];
                    if (p > length) continue;
                    if (segment == 0)
                        p /= length;
                    else {
                        var prev = segments[segment - 1];
                        p = segment + (p - prev) / (length - prev);
                    }
                    break;
                }

                this.AddCurvePosition(p * 0.1f, x1, y1, cx1, cy1, cx2, cy2, x2, y2, positions, o, tangents);
            }
        }

        //Calculates a point on the curve, for a given t value between 0 and 1.
        private void AddCurvePosition(float t, float x1, float y1, float cx1, float cy1, float cx2, float cy2, float x2, float y2, List<float> out1, int offset, bool tangents)
        {
            if (t == 0.0f)
            {
                out1[offset] = x1;
                out1[offset + 1] = y1;
                out1[offset + 2] = 0;
                return;
            }

            if (t == 1.0f)
            {
                out1[offset] = x2;
                out1[offset + 1] = y2;
                out1[offset + 2] = 0;
                return;
            }

            var mt = 1 - t;
            var mt2 = mt * mt;
            var t2 = t * t;
            var a = mt2 * mt;
            var b = mt2 * t * 3;
            var c = mt * t2 * 3;
            var d = t * t2;

            var x = a * x1 + b * cx1 + c * cx2 + d * x2;
            var y = a * y1 + b * cy1 + c * cy2 + d * y2;

            out1[offset] = x;
            out1[offset + 1] = y;
            if (tangents)
            {
                //Calculates the curve tangent at the specified t value
                out1[offset + 2] = (float)Math.Atan2(y - (a * y1 + b * cy1 + c * cy2), x - (a * x1 + b * cx1 + c * cx2));
            }
            else
            {
                out1[offset + 2] = 0;
            }
        }

        private void AddCurvePosition2(float t, List<float> vertices, List<float> out1, int offset, bool tangents, float totalLength)
        {
            if(vertices.Count != 10)
            {
                out1[offset] = 0;
                out1[offset + 1] = 0;
                out1[offset + 2] = 0;
                return;
            }
            var x1 =  vertices[0];
            var y1 =  vertices[1];
            var cx1 = vertices[2];
            var cy1 = vertices[3];
            var cx2 = vertices[4];
            var cy2 = vertices[5];
            var  x2 = vertices[6];
            var  y2 = vertices[7];
            var cx3 = vertices[8];
            var cy3 = vertices[9];

            if (t < 0.0)
            {
                var dydt = cy1 - y1;
                var dxdt = cx1 - x1;
                var angle = (float)Math.Atan2(dydt, dxdt);
                var length = totalLength * t;
                out1[offset] = x1 + length * (float)Math.Cos(angle);
                out1[offset + 1] = y1 + length * (float)Math.Sin(angle);
                out1[offset + 2] = angle;
                return;
            }
            if (t > 1.0f) {
                var dydt = cy3 - y2;
                var dxdt = cx3 - x2;
                var angle = (float)Math.Atan2(dydt, dxdt);
                var length = totalLength * (1 - t);
                out1[offset] = x2 + length * (float)Math.Cos(angle);
                out1[offset + 1] = y2 + length * (float)Math.Sin(angle);
                out1[offset + 2] = angle;
            }
            if (t == 0.0f) {
                out1[offset] = x1;
                out1[offset + 1] = y1;
                var dydt = cy1 - y1;
                var dxdt = cx1 - x1;
                out1[offset + 2] = (float)Math.Atan2(dydt, dxdt);
                return;
            }

            if (t == 1.0f)
            {
                out1[offset] = x2;
                out1[offset + 1] = y2;
                var dydt = cy3 - y2;
                var dxdt = cx3 - x2;
                out1[offset + 2] = (float)Math.Atan2(dydt, dxdt);
                return;
            }
            var t1 = 1 - t;
            var t1Sq = t1 * t1;
            var t1Cu = t1Sq * t1;
            var tSq = t * t;
            var tCu = tSq * t;
        
            // 计算坐标
            var x = t1Cu * x1 + 3 * t1Sq * t * cx1 + 3 * t1 * tSq * cx2 + tCu * x2;
            var y = t1Cu * y1 + 3 * t1Sq * t *cy1 + 3 * t1 * tSq * cy2 + tCu * y2;
        
            out1[offset] = x;
            out1[offset + 1] = y;
            if (tangents) {
                // 计算导数（切线方向）
                var dxdt = 3 * (
                    (cx1 - x1) * t1Sq +
                    2 * (cx2 - cx1) * t1 * t +
                    (x2 - cx2) * tSq
                );

                var dydt = 3 * (
                    (cy1 - y1) * t1Sq +
                    2 * (cy2 -cy1) * t1 * t +
                    (y2 - cy2) * tSq
                );

                // 处理斜率计算
                out1[offset + 2] = (float)Math.Atan2(dydt, dxdt);
            }
            else {
                out1[offset + 2] = 0;
            }
        }

        public override void Init(ConstraintData constraintData, Armature armature)
        {
            this._constraintData = constraintData;
            this._armature = armature;

            var data = constraintData as PathConstraintData;

            this.pathOffset = data.pathDisplayData.vertices.offset;

            //
            this.position = data.position;
            this.spacing = data.spacing;
            this.rotateOffset = data.rotateOffset;
            this.rotateWeight = data.rotateWeight;
            this.xWeight = data.xWeight;
            this.yWeight = data.yWeight;

            //
            this._root = this._armature.GetBone(data.root.name) as Bone;
            this._target = this._armature.GetBone(data.target.name) as Bone;
            this._pathSlot = this._armature.GetSlot(data.pathSlot.name) as Slot;

            int i = 0;
            int l = data.bones.Count;
            for (i = 0; i < l; i++)
            {
                var bone = this._armature.GetBone(data.bones[i].name);
                if (bone != null)
                {
                    this._bones.Add(bone);
                }
            }

            if (data.rotateMode == RotateMode.ChainScale)
            {
                // TODO:    
                // this._boneLengths.length = this._bones.length;
            }

            this._root._hasConstraint = true;
        }

        public override void Update()
        {
            var pathSlot = this._pathSlot;

            if (
                pathSlot._deformVertices == null ||
                pathSlot._deformVertices.verticesData.offset != this.pathOffset // TODO: CHECK
            ) {
                return;
            }

            var constraintData = this._constraintData as PathConstraintData;

            //

            //曲线节点数据改变:父亲bone改变，权重bones改变，变形顶点改变
            var isPathVerticeDirty = false;
            if (this._root._childrenTransformDirty) {
                this._UpdatePathVertices(pathSlot._geometryData, pathSlot._deformVertices);
                isPathVerticeDirty = true;
            }
            else if (pathSlot._deformVertices.verticesDirty || pathSlot._deformVertices.isBonesUpdate()) {
                this._UpdatePathVertices(pathSlot._geometryData, pathSlot._deformVertices);
                pathSlot._deformVertices.verticesDirty = false;
                isPathVerticeDirty = true;
            }

            if (!isPathVerticeDirty && !this.dirty) {
                return;
            }

            //
            var positionMode = constraintData.positionMode;
            var spacingMode = constraintData.spacingMode;
            var rotateMode = constraintData.rotateMode;

            var bones = this._bones;

            var isLengthMode = spacingMode == SpacingMode.Length;
            var isChainScaleMode = rotateMode == RotateMode.ChainScale;
            var isTangentMode = rotateMode == RotateMode.Tangent;
            var boneCount = bones.Count;
            var spacesCount = isTangentMode ? boneCount : boneCount + 1;

            var spacing = this.spacing;
            var spaces = this._spaces;
            spaces.ResizeList(spacesCount);
            // spaces.length = spacesCount;

            //计曲线间隔和长度
            if (isChainScaleMode || isLengthMode) {
                //Bone改变和spacing改变触发
                spaces[0] = 0;
                int i5 = 0;
                int l = spacesCount - 1;
                for (; i5 < l; i5++) {
                    var bone = bones[i5];
                    bone.UpdateByConstraint();
                    var boneLength = bone._boneData.length;
                    var matrix = bone.globalTransformMatrix;
                    var x = boneLength * matrix.a;
                    var y = boneLength * matrix.b;

                    var len = (float)Math.Sqrt(x * x + y * y);
                    if (isChainScaleMode) {
                        this._boneLengths[i5] = len;
                    }
                    spaces[i5 + 1] = (boneLength + spacing) * len / boneLength;
                }
            }
            else {
                int i6 = 0;
                for (i6 = 0; i6 < spacesCount; i6++) {
                    if (spacingMode == SpacingMode.Percent) {
                        if (i6 == 0) {
                            spaces[0] = 0;
                        }
                        else {
                            spaces[i6] = (spacing * 0.01f);
                        }
                    }
                }
            }

            //
            this._ComputeBezierCurve((pathSlot._displayData as PathDisplayData), spacesCount, isTangentMode, positionMode == PositionMode.Percent, spacingMode == SpacingMode.Percent);

            //根据新的节点数据重新采样
            var positions = this._positions;
            var rotateOffset = this.rotateOffset;
            var boneX = positions[0];
            var boneY = positions[1];
            bool tip = false;
            if (rotateOffset == 0) {
                tip = rotateMode == RotateMode.Chain;
            }
            else {
                tip = false;
                var bone = pathSlot.parent;
                if (bone != null) {
                    var matrix = bone.globalTransformMatrix;
                    rotateOffset *= matrix.a * matrix.d - matrix.b * matrix.c > 0 ? Transform.DEG_RAD : - Transform.DEG_RAD;
                }
            }

            //
            var rotateWeight = this.rotateWeight;
            var xWeight = this.xWeight;
            var yWeight = this.yWeight;
            int i = 0;
            int p = 3;
            for (; i < boneCount; i++, p += 3) {
                var bone = bones[i];
                // TODO: 优化,这里可能会计算多遍
                bone.ForceUpdateTransform();
                bone.UpdateByConstraint();
                var matrix = bone.globalTransformMatrix;
                matrix.tx += (boneX - matrix.tx) * xWeight;
                matrix.ty += (boneY - matrix.ty) * yWeight;

                var x = positions[p];
                var y = positions[p + 1];
                var dx = x - boneX;
                var dy = y - boneY;
                if (isChainScaleMode) {
                    var length = this._boneLengths[i];
                    var s = ((float)Math.Sqrt(dx * dx + dy * dy) / length - 1) * rotateWeight + 1;
                    matrix.a *= s;
                    matrix.b *= s;
                }

                boneX = x;
                boneY = y;
                if (rotateWeight > 0) {
                    var a = matrix.a;
                    var b = matrix.b;
                    var c = matrix.c;
                    var d = matrix.d;
                    float r;
                    float cos;
                    float sin;
                    if (isTangentMode) {
                        r = positions[p - 1];
                    }
                    else {
                        r = (float)Math.Atan2(dy, dx);
                    }

                    r -= (float)Math.Atan2(b, a);

                    if (tip) {
                        cos = (float)Math.Cos(r);
                        sin = (float)Math.Sin(r);

                        var length = bone._boneData.length;
                        boneX += (length * (cos * a - sin * b) - dx) * rotateWeight;
                        boneY += (length * (sin * a + cos * b) - dy) * rotateWeight;
                    }
                    else {
                        r += rotateOffset;
                    }

                    if (r > Transform.PI) {
                        r -= Transform.PI_D;
                    }
                    else if (r < -Transform.PI) {
                        r += Transform.PI_D;
                    }

                    r *= rotateWeight;

                    cos = (float)Math.Cos(r);
                    sin = (float)Math.Sin(r);

                    matrix.a = cos * a - sin * b;
                    matrix.b = sin * a + cos * b;
                    matrix.c = cos * c - sin * d;
                    matrix.d = sin * c + cos * d;
                }

                bone.global.FromMatrix(matrix);
            }

            this.dirty = false;
        }

        public override void InvalidUpdate() {
            this.dirty = true;
        }
    }
}
