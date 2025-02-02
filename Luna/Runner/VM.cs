﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Luna.Assets;
using Luna.Runner;
using Luna.Types;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Luna {
    static class VM {
        public static Stopwatch Timer = new Stopwatch();
        public static List<List<LValue>> Lists = new List<List<LValue>>();
        public static List<Dictionary<LValue, LValue>> Maps = new List<Dictionary<LValue, LValue>>();

        public static void LoadRoom(Game _assets, LRoom _room) {
            _assets.CurrentRoom = _room;
            LInstance[] _roomInstances = new LInstance[_room.Instances.Count];
            for (int i = 0; i < _room.Instances.Count; i++) {
                LRoomInstance _instGet = _room.Instances[i];
                LInstance _instCreate = new LInstance(_assets, _instGet.Index, true, _instGet.X, _instGet.Y);
                _instCreate.Variables["image_xscale"] = LValue.Real(_instGet.ScaleX);
                _instCreate.Variables["image_yscale"] = LValue.Real(_instGet.ScaleY);
                _instCreate.Variables["image_speed"] = LValue.Real(_instGet.ImageSpeed);
                _instCreate.Variables["image_index"] = LValue.Real(_instGet.ImageIndex);
                _instCreate.Variables["image_blend"] = LValue.Real(_instGet.ImageBlend);
                _instCreate.Variables["image_angle"] = LValue.Real(_instGet.Rotation);
                _instCreate.RoomPreCreate = _instGet.PreCreate;
                _instCreate.RoomCreate = _instGet.CreationCode;
                _roomInstances[i] = _instCreate;
            }

            for(int i = 0; i < _roomInstances.Length; i++) {
                LInstance _instGet = _roomInstances[i];
                if (_instGet.PreCreate != null) _instGet.Environment.ExecuteCode(_assets, _instGet.PreCreate);
                if (_instGet.RoomPreCreate != null) _instGet.Environment.ExecuteCode(_assets, _instGet.RoomPreCreate);
                if (_instGet.Create != null) _instGet.Environment.ExecuteCode(_assets, _instGet.Create);
                if (_instGet.RoomCreate != null) _instGet.Environment.ExecuteCode(_assets, _instGet.RoomCreate);
            }

            if (_room.CreationCode != null) {
                Domain _roomEnvironment = new LInstance(_assets, -100, false).Environment;
                _roomEnvironment.ExecuteCode(_assets, _room.CreationCode);
            }
        }

        public static void DrawDefaultObject(Game _assets, GMLObject _inst) {
            LValue _index = _inst.Variables["sprite_index"];
            LValue _image = _inst.Variables["image_index"];
            if (_index.I32 >= 0 && _index < _assets.SpriteMapping.Count) {
                LSprite _sprite = _assets.SpriteMapping[_index];
                double _x = _inst.Variables["x"];
                double _y = _inst.Variables["y"];
                GL.PushMatrix();
                GL.Translate(_x,_y,0);
                GL.Rotate(_inst.Variables["image_angle"].Number,0,0,1);
                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, _assets.TextureEntries[_image].GLTexture);
                GL.Begin(PrimitiveType.TriangleStrip);
                byte[] _color = BitConverter.GetBytes((int)_inst.Variables["image_blend"].Number);
                GL.Color4(_color[2] / 255d, _color[1] / 255d, _color[0] / 255d, _inst.Variables["image_alpha"]);
                GL.TexCoord2(0.0,0.0);
                GL.Vertex2(-_sprite.XOrigin, -_sprite.YOrigin);
                GL.TexCoord2(1.0,0.0);
                GL.Vertex2(-_sprite.XOrigin+_sprite.Width, -_sprite.YOrigin);
                GL.TexCoord2(0.0,1.0);
                GL.Vertex2(-_sprite.XOrigin, -_sprite.YOrigin+_sprite.Height);
                GL.TexCoord2(1.0,1.0);
                GL.Vertex2(-_sprite.XOrigin+_sprite.Width, -_sprite.YOrigin+_sprite.Height);
                GL.End();
                GL.Disable(EnableCap.Texture2D);
                GL.PopMatrix();
            }
        }
    }

    /// <summary>
    /// Implementation/transplant of the Well512a RNG algorithm in C#
    /// </summary>
    /// <remarks>
    /// Original C code at http://www.iro.umontreal.ca/~panneton/well/WELL512a.c
    /// </remarks>
    static class WellGenerator
    {
        private static readonly int W = 32;
        private static readonly int R = 16;
        private static readonly int P = 0;
        private static readonly int M1 = 13;
        private static readonly int M2 = 9;
        private static readonly int M3 = 5;
        private static readonly double FACT = 2.32830643653869628906e-10;
        
        private static int state_i;
        private static List<uint> State = Enumerable.Repeat(0u,R).ToList();
        private static uint seed;//should never be 0 ever, that's why i set it to a funny number in the constructor

        public static uint Seed
        {
            get => seed;
            set
            {
                seed = value;
                State = Enumerable.Repeat(value,R).ToList();
            }
        }

        static WellGenerator()
        {
            Seed = 8008135;//i know, total comedian aren't i
        }

        private static uint z0, z1, z2;

        private static uint Mat0Pos(int t, uint v) => v ^ v >> t;
        private static uint Mat0Neg(int t, uint v) => v ^ v << -t;
        private static uint Mat3Neg(int t, uint v) => v << -t;
        private static uint Mat4Neg(int t, uint b, uint v) => v ^ v << -t & b;
        public static double Next()
        {
            z0 = State[(state_i + 15) & 15];
            z1 = Mat0Neg(-16,State[state_i]) ^ Mat0Neg(-15, State[(state_i + M1) & 15]);
            z2 = Mat0Pos(11, State[(state_i + M2) & 15]);
            uint newV1 = State[state_i] = z1 ^ z2;
            State[(state_i + 15) & 15] = Mat0Neg(-2,z0) ^ Mat0Neg(-18,z1) ^ Mat3Neg(-28,z2) ^ Mat4Neg(-5,0xda442d24U, newV1) ; 
            state_i = (state_i + 15) & 15;
            return State[state_i] * FACT;
        }

        public static int ENext(int min, int max)
        {
            min = (int) Math.Ceiling((double) min);
            max = (int) Math.Floor((double) max);
            return (int) (Math.Floor(Next()*(max - min))+min);
        }

        public static int ENext(int max)
        {
            return ENext(0,max);
        }

        public static int INext(int min, int max)
        {
            min = (int) Math.Ceiling((double) min);
            max = (int) Math.Floor((double) max);
            return (int) (Math.Floor(Next()*(max - min+1))+min);
        }

        public static int INext(int max)
        {
            return INext(0, max);
        }
    }
}
