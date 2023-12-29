﻿using Luna.Types;
using System;

namespace Luna {
    public enum LType { //taken from YYRValue.h
        Number,
        String,
        Array,
        Pointer,
        Vec3,
        Undefined,
        Object,
        Int32 = 7,
        Vec4,
        Matrix,
        Int64,
        Accessor,
        Null,
        Bool,
        Iterator,
        Ref
    }

    struct LValue {
        public LType Type;
        public double Number;
        public Int32 I32;
        public Int64 I64;
        public string String;
        public LValue[] Array;
        public bool Undefined;
        public UInt32 flags;
        public GMLObject Object;
        public LValue(LType _type, object _val) {
            this.Number = 0;
            this.String = "";
            this.I32 = 0;
            this.I64 = 0;
            this.Array = null;
            this.Undefined = false;
            this.Object = null;

            this.flags = 0; //a bit flag that YYRValue uses sometimes? apparently only used for javascript
            this.Type = _type;
            switch (this.Type) {
                case LType.Number: this.Number = (double)_val; break;
                case LType.String: this.String = (string)_val; break;
                case LType.Int32: this.I32 = (Int32)_val; break;
                case LType.Int64: this.I64 = (Int64)_val; break;
                case LType.Array: this.Array = (LValue[])_val; break;
                case LType.Undefined: this.Undefined = true; break;
                //case LType.Object: this.Object = _val; break;
            }
        }

        public override string ToString() {
            switch (this.Type) {
                case LType.Number: return this.Number.ToString();
                case LType.String: return this.String;
                case LType.Int32: return this.I32.ToString();
                case LType.Int64: return this.I64.ToString();
                case LType.Undefined: return "undefined";
                case LType.Array: {
                    string _valReturn = "[ ";
                    for (int i = 0; i < this.Array.Length; i++) {
                        LValue _valGet = this.Array[i];
                        if (_valGet.Type == LType.String) _valReturn += '"';
                        _valReturn += _valGet.ToString();
                        if (_valGet.Type == LType.String) _valReturn += '"';
                        if (i < this.Array.Length - 1) _valReturn += ",";
                    }
                    return _valReturn + " ]";
                }
            }
            throw new Exception(String.Format("Could not convert type \"{0}\" to string", this.Type));
        }

        public object Value {
            get {
                switch (this.Type) {
                    case LType.Number: return this.Number;
                    case LType.String: return this.String;
                    case LType.Int32: return this.I32;
                    case LType.Int64: return this.I64;
                    case LType.Array: return this.Array;
                    case LType.Undefined: return this.Undefined;
                }
                throw new Exception(String.Format("Could not return value for type: {0}", this.Type));
            }
        }

        public static implicit operator double(LValue _val) => _val.Number;
        public static implicit operator string(LValue _val) => _val.String;
        public static implicit operator Int32(LValue _val) => _val.I32;
        public static implicit operator Int64(LValue _val) => _val.I64;
        public static implicit operator LValue[](LValue _val) => _val.Array;
        public static implicit operator bool(LValue _val) => _val.Number > 0.5 ? true : false;

        #region Math
        public static LValue operator +(LValue a, LValue b) {
            if (a.Type == LType.Number && a.Type == LType.Number) {
                return LValue.Real(a.Number + b.Number);
            } else if (a.Type == LType.String && b.Type == LType.String) {
                return LValue.Text(a.String + b.String);
            }
            throw new Exception(String.Format("Could not add {0} (Type: {2}) to {1} (Type: {3})", a.ToString(), b.ToString(), a.Type, b.Type));
        }

        public static LValue operator -(LValue a, LValue b) {
            if (a.Type == LType.Number && b.Type == LType.Number) {
                return LValue.Real(a.Number - b.Number);
            }
            throw new Exception(String.Format("Could not subtract {0} (Type: {2}) from {1} (Type: {3})", a.ToString(), b.ToString(), a.Type, b.Type));
        }

        public static LValue operator *(LValue a, LValue b) {
            if (a.Type == LType.Number && b.Type == LType.Number) {
                return LValue.Real(a.Number * b.Number);
            }
            throw new Exception(String.Format("Could not divide {0} (Type: {2}) by {1} (Type: {3})", a.ToString(), b.ToString(), a.Type, b.Type));
        }

        public static LValue operator /(LValue a, LValue b) {
            if (a.Type == LType.Number && b.Type == LType.Number) {
                return LValue.Real(a.Number / b.Number);
            }
            throw new Exception(String.Format("Could not divide {0} (Type: {2}) by {1} (Type: {3})", a.ToString(), b.ToString(), a.Type, b.Type));
        }
        #endregion

        #region Conditionals
        public static LValue operator ==(LValue a, LValue b) {
            switch (a.Type) {
                case LType.Number: return LValue.Bool(Math.Abs(a.Number - b.Number) < 0.00001f);
                case LType.String: return LValue.Bool(a.String == b.String);
                case LType.Undefined: return LValue.Bool(b.Type == LType.Undefined);
            }
            throw new Exception(String.Format("Could not check if {0} (Type: {2}) is equal to {1} (Type: {3})", a.ToString(), b.ToString(), a.Type, b.Type));
        }

        public static LValue operator !=(LValue a, LValue b) {
            switch (a.Type) {
                case LType.Number: return LValue.Real(Math.Abs(a.Number - b.Number) > 0.00001 ? 1 : 0);
                case LType.String: return LValue.Real(a.String != b.String ? 1 : 0);
                case LType.Undefined: return LValue.Bool(b.Type != LType.Undefined);
            }
            throw new Exception(String.Format("Could not check if {0} (Type: {2}) is not equal to {1} (Type: {3})", a.ToString(), b.ToString(), a.Type, b.Type));
        }

        public static LValue operator <(LValue a, LValue b) {
            switch (a.Type) {
                case LType.Number: return LValue.Real(a.Number < b.Number ? 1 : 0);
            }
            throw new Exception(String.Format("Could not check if {0} (Type: {2}) is less than {1} (Type: {3})", a.ToString(), b.ToString(), a.Type, b.Type));
        }

        public static LValue operator >(LValue a, LValue b) {
            switch (a.Type) {
                case LType.Number: return LValue.Real(a.Number > b.Number ? 1 : 0);
            }
            throw new Exception(String.Format("Could not check if {0} (Type: {2}) is greater than {1} (Type: {3})", a.ToString(), b.ToString(), a.Type, b.Type));
        }

        public static LValue operator <=(LValue a, LValue b) {
            switch (a.Type) {
                case LType.Number: return LValue.Real(a.Number <= b.Number ? 1 : 0);
            }
            throw new Exception(String.Format("Could not check if {0} (Type: {2}) is less than or equal to {1} (Type: {3})", a.ToString(), b.ToString(), a.Type, b.Type));
        }

        public static LValue operator >=(LValue a, LValue b) {
            switch (a.Type) {
                case LType.Number: return LValue.Real(a.Number >= b.Number ? 1 : 0);
            }
            throw new Exception(String.Format("Could not check if {0} (Type: {2}) is greater than or equal to {1} (Type: {3})", a.ToString(), b.ToString(), a.Type, b.Type));
        }

        public static bool GetBool(LValue a) {
            if (a.Type == LType.Number) {
                if (a.Number >= 0.5) return true;
            }
            return false;
        }
        #endregion

        #region Static Initializers
        public static LValue Real(double _value) => new LValue(LType.Number, _value);

        public static LValue Text(string _value) => new LValue(LType.String, _value);

        public static LValue Values(params LValue[] _values) => new LValue(LType.Array, _values);

        public static LValue Values(params Object[] _values) {
            LValue[] _vals = new LValue[_values.Length];
            for (int i = 0; i < _values.Length; i++) {
                object _val = _values[i];
                if (_val is String _str) _vals[i] = Text(_str);
                else if (_val is IConvertible _conv) {
                    _vals[i] = Real(_conv.ToDouble(null));
                } else _vals[i] = Real(0);//failed to convert, change this to undefined when it's implemented
            }
            return Values(_vals);
        }

        public static LValue Undef() => new LValue(LType.Undefined, null);

        public static LValue Bool(bool _value)
        {
            return _value==true?True():False();
        }

        //Shorthand for Real 0
        public static LValue False()
        {
            return LValue.Real(0);
        }

        //Shorthand for Real 1
        public static LValue True()
        {
            return LValue.Real(1);
        }

        #endregion
    }
}
