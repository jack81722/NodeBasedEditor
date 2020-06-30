using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace NodeEditor.Generic
{
    public static class DigitConvert
    {
        public static readonly Type INT = typeof(int);
        public static readonly Type FLOAT = typeof(float);
        public static readonly Type V2 = typeof(Vector2);
        public static readonly Type V3 = typeof(Vector3);
        public static readonly Type V4 = typeof(Vector4);

        public static int CastToInt(this object value)
        {
            var type = value.GetType();
            
            if(type == V2)
            {
                Vector2 v = (Vector2)value;
                return (int)v.x;
            }
            else if (type == V3)
            {
                Vector3 v = (Vector3)value;
                return (int)v.x;
            }
            else if (type == V4)
            {
                Vector4 v = (Vector4)value;
                return (int)v.x;
            }
            else
            {
                return Convert.ToInt32(value);
            }
        }
        public static float CastToFloat(this object value)
        {
            var type = value.GetType();

            if (type == V2)
            {
                Vector2 v = (Vector2)value;
                return v.x;
            }
            else if (type == V3)
            {
                Vector3 v = (Vector3)value;
                return v.x;
            }
            else if (type == V4)
            {
                Vector4 v = (Vector4)value;
                return v.x;
            }
            else
            {
                return Convert.ToSingle(value);
            }
        }
        public static Vector2 CastToVector2(this object value)
        {
            var type = value.GetType();
            if (type == INT)
            {
                int v = Convert.ToInt32(value);
                return new Vector2(v, v);
            }
            else if (type == FLOAT)
            {
                float v = Convert.ToSingle(value);
                return new Vector2(v, v);
            }
            else if(type == V3)
            {
                Vector3 v = (Vector3)value;
                return new Vector2(v.x, v.y);
            }
            else if(type == V4)
            {
                Vector4 v = (Vector4)value;
                return new Vector2(v.x, v.y);
            }
            else
            {
                return (Vector2)value;
            }
        }
        public static Vector3 CastToVector3(this object value)
        {
            var type = value.GetType();
            if (type == INT)
            {
                int v = Convert.ToInt32(value);
                return new Vector3(v, v, v);
            }
            else if (type == FLOAT)
            {
                float v = Convert.ToSingle(value);
                return new Vector3(v, v, v);
            }
            else if (type == V2)
            {
                Vector2 v = (Vector2)value;
                return new Vector3(v.x, v.y, 0);
            }
            else if (type == V4)
            {
                Vector4 v = (Vector4)value;
                return new Vector3(v.x, v.y, v.z);
            }
            else
            {
                return (Vector3)value;
            }
        }
        public static Vector4 CastToVector4(this object value)
        {
            var type = value.GetType();
            if (type == INT)
            {
                int v = Convert.ToInt32(value);
                return new Vector4(v, v, v, v);
            }
            else if (type == FLOAT)
            {
                float v = Convert.ToSingle(value);
                return new Vector4(v, v, v, v);
            }
            else if (type == V2)
            {
                Vector2 v = (Vector2)value;
                return new Vector4(v.x, v.y, 0, 0);
            }
            else if (type == V3)
            {
                Vector3 v = (Vector3)value;
                return new Vector4(v.x, v.y, v.z, 0);
            }
            else
            {
                return (Vector4)value;
            }
        }

        public static object CastTo(this object value, Type type)
        {
            if (type == V4)
                return value.CastToVector4();
            if (type == V3)
                return value.CastToVector3();
            if (type == V2)
                return value.CastToVector2();
            
            return Convert.ToSingle(value);
        }

        public static object Add(this object a, object b, Type type)
        {
            if (type == V4)
                return a.CastToVector4() + b.CastToVector4();
            if (type == V3)
                return a.CastToVector3() + b.CastToVector3();
            if (type == V2)
                return a.CastToVector2() + b.CastToVector2();
            if (type == FLOAT)
                return a.CastToFloat() + b.CastToFloat();
            return a.CastToInt() + b.CastToInt();
        }

        public static object Minus(this object a, object b, Type type)
        {
            if (type == V4)
                return a.CastToVector4() - b.CastToVector4();
            if (type == V3)
                return a.CastToVector3() - b.CastToVector3();
            if (type == V2)
                return a.CastToVector2() - b.CastToVector2();
            if (type == FLOAT)
                return a.CastToFloat() - b.CastToFloat();
            return a.CastToInt() - b.CastToInt();
        }

        public static object Multi(this object a, object b, Type type)
        {   
            if (type == V4)
                return a.CastToVector4() * b.CastToFloat();
            if (type == V3)
                return a.CastToVector3() * b.CastToFloat();
            if (type == V2)
                return a.CastToVector2() * b.CastToFloat();
            if (type == FLOAT)
                return a.CastToFloat() * b.CastToFloat();
            return a.CastToInt() * b.CastToInt();
        }

        public static object Divide(this object a, object b, Type type)
        {
            if (type == V4)
                return a.CastToVector4() / b.CastToFloat();
            if (type == V3)
                return a.CastToVector3() / b.CastToFloat();
            if (type == V2)
                return a.CastToVector2() / b.CastToFloat();
            if (type == FLOAT)
                return a.CastToFloat() / b.CastToFloat();
            return a.CastToInt() / b.CastToInt();
        }

        public static object Normalize(this object v, Type type)
        {
            if (type == V4)
                return v.CastToVector4().normalized;
            if (type == V3)
                return v.CastToVector3().normalized;
            if (type == V2)
                return v.CastToVector2().normalized;
            if (type == FLOAT)
                return v.CastToVector2().normalized;
            return v.CastToVector2().normalized;
        }
    }
}