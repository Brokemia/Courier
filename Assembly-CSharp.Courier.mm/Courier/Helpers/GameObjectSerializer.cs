using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

namespace Mod.Courier.Helpers {
    public static class GameObjectSerializer {
        public class IgnoreEntry {
            Type declaringType;
            string name;

            public IgnoreEntry(Type type, string name) {
                declaringType = type;
                this.name = name;
            }

            public override bool Equals(object obj) {
                return obj is IgnoreEntry other && other.declaringType.Equals(declaringType) && other.name.Equals(name);
            }

            public override int GetHashCode() {
                var hashCode = -117268428;
#pragma warning disable RECS0025 // Non-readonly field referenced in 'GetHashCode()'
                hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(declaringType);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
                return hashCode;
            }
        }

        public static List<IgnoreEntry> ignored = new List<IgnoreEntry>();

        static GameObjectSerializer() {
            ignored.Add(new IgnoreEntry(typeof(Transform), "parent"));
            ignored.Add(new IgnoreEntry(typeof(Transform), "root"));
            ignored.Add(new IgnoreEntry(typeof(Transform), "childCount"));
            ignored.Add(new IgnoreEntry(typeof(Transform), "worldToLocalMatrix"));
            ignored.Add(new IgnoreEntry(typeof(Transform), "localToWorldMatrix"));
        }

        private static string SerializeObject(object o, int level) {
            if(o is GameObject) {
                return SerializeGameObject(o as GameObject, level);
            }
            string res = "";
            res += GetIndent(level) + "(" + o.GetType() + ")";
            foreach(FieldInfo field in o.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance)) {
                if (!ignored.Contains(new IgnoreEntry(field.DeclaringType, field.Name))) {
                    res += " " + field.Name + "='" + field.GetValue(o) + "'";
                }
            }
            foreach (PropertyInfo property in o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)) {
                if (property.CanRead && !property.Name.Equals("transform") && !property.Name.Equals("gameObject") && !ignored.Contains(new IgnoreEntry(property.DeclaringType, property.Name))) {
                    res += " " + property.Name + "='" + property.GetGetMethod().Invoke(o, null) + "'";
                }
            }
            res += "\n";

            return res;
        }

        private static string SerializeGameObject(GameObject gameObject, int level) {
            string res = "";
            res += GetIndent(level) + "(" + gameObject.GetType() + ") name='" + gameObject.name + "' tag='" + gameObject.tag + "' layer='" + gameObject.layer;
            res += "' activeSelf='" + gameObject.activeSelf + "' isStatic='" + gameObject.isStatic + "' hideFlags='" + gameObject.hideFlags + "'\n";
            res += GetIndent(level) + "**Components**=\n";
            foreach (Component c in gameObject.GetComponents<Component>()) {
                res += SerializeObject(c, level+1);
            }
            res += GetIndent(level) + "**Children**=\n";
            foreach (Transform tf in gameObject.transform.GetChildren()) {
                res += SerializeGameObject(tf.gameObject, level + 1);
            }

            return res;
        }

        public static string Serialize(GameObject gameObject) {
            return SerializeGameObject(gameObject, 0);
        }

        public static void SerializeToStream(Stream stream, GameObject gameObject) {
            using(var writer = new StreamWriter(stream)) {
                writer.Write(Serialize(gameObject));
            }
        }

        private static string GetIndent(int level) {
            string res = "";
            for(int i = 0; i < level; i++) {
                res += "   ";
            }
            return res;
        }
    }
}
