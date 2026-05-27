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
 using System.Collections.Generic;

namespace DragonBones
{
    /// <internal/>
    /// <private/>
    public abstract class ConstraintData : BaseObject
    {
        public int order;
        public string name;
        public BoneData target;
        public BoneData root;
        public BoneData bone = null;
        public ConstraintType type;

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
        public List<BoneData> bones;
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

            this.target = null;
            this.bones = null;
            this.offsetX = 0;
            this.offsetY = 0;
            this.offsetRotation = 0;
            this.offsetScaleX = 0;
            this.offsetScaleY = 0;
            this.rotateWeight = 0;
            this.scaleWeight = 0;
            this.translateWeight = 0;
            this.local = false;
            this.relative = false;
            this.type = ConstraintType.Transform;
        }
    }

    /// <internal/>
    /// <private/>

    public class PhysicsConstraintData : ConstraintData
    {
        public float x;
        public float y;
        public float rotate;
        public float scaleX;
        public float shearX;
        public int limit;
        public uint fps;
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

            this.x = 0;
            this.y = 0;
            this.rotate = 0;
            this.scaleX = 0;
            this.shearX = 0;
            this.limit = 0;
            this.fps = 0;
            this.inertia = 0;
            this.strength = 0;
            this.damping = 0;
            this.mass = 0;
            this.wind = 0;
            this.windDisturbance = 0;
            this.gravity = 0;
            this.weight = 0;
            this.type = ConstraintType.Physics;
        }
    }

    public class PathConstraintData : ConstraintData
    {

        public SlotData pathSlot;
        public PathDisplayData pathDisplayData;
        public List<BoneData> bones;

        public PositionMode positionMode;
        public SpacingMode spacingMode;
        public RotateMode rotateMode;

        public float position;
        public float spacing;
        public float rotateOffset;
        public float rotateWeight;
        public float xWeight;
        public float yWeight;

        protected override void _OnClear() {
            base._OnClear();

            this.pathSlot = null;
            this.pathDisplayData = null;
            this.bones = new List<BoneData>();

            this.positionMode = PositionMode.Fixed;
            this.spacingMode = SpacingMode.Fixed;
            this.rotateMode = RotateMode.Chain;

            this.position = 0.0f;
            this.spacing = 0.0f;
            this.rotateOffset = 0.0f;
            this.rotateWeight = 0.0f;
            this.xWeight = 0.0f;
            this.yWeight = 0.0f;
            this.type = ConstraintType.Path;
        }

        public void AddBone( BoneData value) {
            this.bones.Add(value);
        }
    }
}
