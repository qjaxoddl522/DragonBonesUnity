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
namespace DragonBones
{
    using System.Collections.Generic;
    /// <internal/>
    /// <private/>
    public enum BoneType
    {
        Bone,
        Surface
    }

    /// <internal/>
    /// <private/>
    public enum PositionMode
    {
        Fixed,
        Percent
    }

    /// <internal/>
    /// <private/>
    public enum SpacingMode
    {
        Fixed,
        Percent,
        Length
    }

    /// <internal/>
    /// <private/>
    public enum RotateMode
    {
        Tangent,
        Chain,
        ChainScale
    }

    /// <internal/>
    /// <private/>
    public abstract class ConstraintData : BaseObject
    {
        public int order;
        public string name;
        public BoneData target;
        public BoneData root;
        public BoneData bone = null;

        protected override void _OnClear()
        {
            this.order = 0;
            this.name = string.Empty;
            this.target = null;
            this.bone = null;
            this.root = null;
        }
    }
    /// <internal/>
    /// <private/>
    public class IKConstraintData : ConstraintData
    {
        public bool scaleEnabled;
        public bool bendPositive;
        public float weight;

        protected override void _OnClear()
        {
            base._OnClear();

            this.scaleEnabled = false;
            this.bendPositive = false;
            this.weight = 1.0f;
        }
    }

    /// <internal/>
    /// <private/>
    public class TransformConstraintData : ConstraintData
    {
        public BoneData targetBone;
        public readonly List<BoneData> bones = new List<BoneData>();
        public float offsetX;
        public float offsetY;
        public float offsetRotation;
        public float offsetScaleX;
        public float offsetScaleY;
        public float rotateWeight;
        public float scaleWeight;
        public float translateWeight;
        public bool local;
        public bool relative;

        protected override void _OnClear()
        {
            base._OnClear();

            this.targetBone = null;
            this.bones.Clear();
            this.offsetX = 0.0f;
            this.offsetY = 0.0f;
            this.offsetRotation = 0.0f;
            this.offsetScaleX = 0.0f;
            this.offsetScaleY = 0.0f;
            this.rotateWeight = 0.0f;
            this.scaleWeight = 0.0f;
            this.translateWeight = 0.0f;
            this.local = false;
            this.relative = false;
        }
    }

    /// <internal/>
    /// <private/>
    public class PhysicsConstraintData : ConstraintData
    {
        public bool x;
        public bool y;
        public bool rotate;
        public bool scaleX;
        public bool shearX;
        public float limit;
        public float fps;
        public float inertia;
        public float strength;
        public float damping;
        public float mass;
        public float wind;
        public float windDisturbance;
        public float gravity;
        public float weight;

        protected override void _OnClear()
        {
            base._OnClear();

            this.x = false;
            this.y = false;
            this.rotate = false;
            this.scaleX = false;
            this.shearX = false;
            this.limit = 0.0f;
            this.fps = 0.0f;
            this.inertia = 0.0f;
            this.strength = 0.0f;
            this.damping = 0.0f;
            this.mass = 0.0f;
            this.wind = 0.0f;
            this.windDisturbance = 0.0f;
            this.gravity = 0.0f;
            this.weight = 0.0f;
        }
    }

    /// <internal/>
    /// <private/>
    public class PathConstraintData : ConstraintData
    {
        public SlotData pathSlot;
        public PathDisplayData pathDisplayData;
        public readonly List<BoneData> bones = new List<BoneData>();
        public PositionMode positionMode;
        public SpacingMode spacingMode;
        public RotateMode rotateMode;
        public float position;
        public float spacing;
        public float rotateOffset;
        public float rotateWeight;
        public float xWeight;
        public float yWeight;

        protected override void _OnClear()
        {
            base._OnClear();

            this.pathSlot = null;
            this.pathDisplayData = null;
            this.bones.Clear();
            this.positionMode = PositionMode.Fixed;
            this.spacingMode = SpacingMode.Fixed;
            this.rotateMode = RotateMode.Chain;
            this.position = 0.0f;
            this.spacing = 0.0f;
            this.rotateOffset = 0.0f;
            this.rotateWeight = 0.0f;
            this.xWeight = 0.0f;
            this.yWeight = 0.0f;
        }
    }
}
