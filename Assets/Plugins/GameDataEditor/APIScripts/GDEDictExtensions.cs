using UnityEngine;
using UnityEngine.Video;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameDataEditor
{
public static class GDEDictKeyExtensions
{
    public static string GDETypeKey(this string fieldName)
    {
        return string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName);
    }

    public static string GDEGetType(this Dictionary<string, object> gdeData, string field)
    {
        string typeKey = field.GDETypeKey();
        string value = string.Empty;
        gdeData.TryGetString(typeKey, out value);
        return value;
    }
}

public static class ToGDEDictExtensions
{
    public static object ToDictValue (this bool variable)
    {
        return variable;
    }

    public static object ToDictValue (this int variable)
    {
        return variable;
    }


    public static object ToDictValue (this float variable)
    {
        return variable;
    }


    public static object ToDictValue (this string variable)
    {
        return variable;
    }


    public static object ToDictValue(this Enum variable)
    {
        return variable.ToString();
    }

    public static object ToDictValue (this Vector2 variable)
    {
        var result = new Dictionary<string, object> ();
        result.Add ("x", variable.x);
        result.Add ("y", variable.y);
        return result;
    }

    public static object ToDictValue (this Vector3 variable)
    {
        var result = new Dictionary<string, object> ();
        result.Add ("x", variable.x);
        result.Add ("y", variable.y);
        result.Add ("z", variable.z);
        return result;
    }

    public static object ToDictValue (this Vector4 variable)
    {
        var result = new Dictionary<string, object> ();
        result.Add ("x", variable.x);
        result.Add ("y", variable.y);
        result.Add ("z", variable.z);
        result.Add ("w", variable.w);
        return result;
    }

    public static object ToDictValue (this Color variable)
    {
        var result = new Dictionary<string, object> ();
        result.Add ("r", variable.r);
        result.Add ("g", variable.g);
        result.Add ("b", variable.b);
        result.Add ("a", variable.a);
        return result;
    }

    public static object ToDictValue<T> (this T variable) where T : IGDEData
    {
        string result = string.Empty;
        if (variable != null)
            result = variable.Key;
        return result;
    }

    public static object ToDictValue (this UnityEngine.Object variable)
    {
        string result = string.Empty;
        if (variable != null)
            result = variable.GetPath().ToString();
        return result;
    }

    public static object ToDictValue (this GameObject variable)
    {
        return (variable as UnityEngine.Object).ToDictValue ();
    }

    public static object ToDictValue (this Texture2D variable)
    {
        return (variable as UnityEngine.Object).ToDictValue ();
    }

    public static object ToDictValue (this Material variable)
    {
        return (variable as UnityEngine.Object).ToDictValue ();
    }

    public static object ToDictValue (this AudioClip variable)
    {
        return (variable as UnityEngine.Object).ToDictValue ();
    }

    public static object ToDictValue (this VideoClip variable)
    {
        return (variable as UnityEngine.Object).ToDictValue ();
    }

    public static Dictionary<string, object> ToGDEDict (this bool variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (fieldName.GDETypeKey(), BasicFieldType.Bool.ToString ());
        result.Add (fieldName, variable.ToDictValue ());
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this int variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (fieldName.GDETypeKey(), BasicFieldType.Int.ToString ());
        result.Add (fieldName, variable.ToDictValue ());
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this float variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (fieldName.GDETypeKey(), BasicFieldType.Float.ToString ());
        result.Add (fieldName, variable.ToDictValue ());
        return result;
    }


    public static Dictionary<string, object> ToGDEDict (this string variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (fieldName.GDETypeKey(), BasicFieldType.String.ToString ());
        result.Add (fieldName, variable.ToDictValue ());
        return result;
    }


    public static Dictionary<string, object> EnumToGDEDict<T> (this T variable, string fieldName) where T : Enum
    {
        var result = new Dictionary<string, object> ();
        result.Add (fieldName.GDETypeKey(), typeof(T).Name);
        result.Add (fieldName, variable.ToDictValue ());
        return result;
    }


    public static Dictionary<string, object> ToGDEDict (this Vector2 variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (fieldName.GDETypeKey(), BasicFieldType.Vector2.ToString ());
        result.Add (fieldName, variable.ToDictValue ());
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this Vector3 variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (fieldName.GDETypeKey(), BasicFieldType.Vector3.ToString ());
        result.Add (fieldName, variable.ToDictValue ());
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this Vector4 variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (fieldName.GDETypeKey(), BasicFieldType.Vector4.ToString ());
        result.Add (fieldName, variable.ToDictValue ());
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this Color variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (fieldName.GDETypeKey(), BasicFieldType.Color.ToString ());
        result.Add (fieldName, variable.ToDictValue ());
        return result;
    }

    public static Dictionary<string, object> ToGDEDict<T> (this T variable, string fieldName) where T : IGDEData
    {
        var result = new Dictionary<string, object> ();
        result.Add (fieldName.GDETypeKey(), variable.SchemaName ());
        result.Add (fieldName, variable.ToDictValue ());
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this GameObject variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (fieldName.GDETypeKey(), BasicFieldType.GameObject.ToString ());
        result.Add (fieldName, variable.ToDictValue ());
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this Texture2D variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (fieldName.GDETypeKey(), BasicFieldType.Texture2D.ToString ());
        result.Add (fieldName, variable.ToDictValue ());
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this Material variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Material.ToString ());
        result.Add (fieldName, variable.GetPath ());
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this VideoClip variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (fieldName.GDETypeKey(), BasicFieldType.VideoClip.ToString ());
        result.Add (fieldName, variable.ToDictValue ());
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this AudioClip variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (fieldName.GDETypeKey(), BasicFieldType.AudioClip.ToString ());
        result.Add (fieldName, variable.ToDictValue ());
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<bool> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Bool.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 1);

        var list = new List<object> ();

        if (variable != null) {
            for (int x = 0; x < variable.Count; x++)
                list.Add (variable [x].ToDictValue ());
        }

        result.Add (fieldName, list);
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<int> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Int.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 1);

        var list = new List<object> ();

        if (variable != null) {
            for (int x = 0; x < variable.Count; x++)
                list.Add (variable [x].ToDictValue ());
        }

        result.Add (fieldName, list);
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<float> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Float.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 1);

        var list = new List<object> ();
        if (variable != null) {
            for (int x = 0; x < variable.Count; x++)
                list.Add (variable [x].ToDictValue ());
        }

        result.Add (fieldName, list);
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<string> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.String.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 1);

        var list = new List<object> ();
        if (variable != null) {
            for (int x = 0; x < variable.Count; x++)
                list.Add (variable [x].ToDictValue ());
        }

        result.Add (fieldName, list);
        return result;
    }

    public static Dictionary<string, object> EnumToGDEDict<T> (this List<T> variable, string fieldName) where T : Enum
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format(GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), typeof(T).Name);
        result.Add (string.Format(GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 1);

        var list = new List<object> ();

        if (variable != null)
        {
            for (int x = 0; x < variable.Count; x++)
                list.Add(variable[x].ToDictValue());
        }

        result.Add (fieldName, list);
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<Vector2> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Vector2.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 1);

        var list = new List<object> ();
        if (variable != null) {
            for (int x = 0; x < variable.Count; x++)
                list.Add (variable [x].ToDictValue ());
        }

        result.Add (fieldName, list);
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<Vector3> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Vector3.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 1);

        var list = new List<object> ();
        if (variable != null) {
            for (int x = 0; x < variable.Count; x++)
                list.Add (variable [x].ToDictValue ());
        }

        result.Add (fieldName, list);
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<Vector4> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Vector4.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 1);

        var list = new List<object> ();
        if (variable != null) {
            for (int x = 0; x < variable.Count; x++)
                list.Add (variable [x].ToDictValue ());
        }

        result.Add (fieldName, list);
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<Color> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Color.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 1);

        var list = new List<object> ();
        if (variable != null) {
            for (int x = 0; x < variable.Count; x++)
                list.Add (variable [x].ToDictValue ());
        }

        result.Add (fieldName, list);
        return result;
    }

    public static Dictionary<string, object> ToGDEDict<T> (this List<T> variable, string fieldName) where T : IGDEData
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), variable.SchemaName ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 1);
        result.Add (fieldName, variable.GetKeyList ());

        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<GameObject> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.GameObject.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 1);
        result.Add (fieldName, variable.GetPathList ());

        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<Texture2D> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Texture2D.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 1);
        result.Add (fieldName, variable.GetPathList ());

        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<Material> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Material.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 1);
        result.Add (fieldName, variable.GetPathList ());

        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<VideoClip> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.VideoClip.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 1);
        result.Add (fieldName, variable.GetPathList ());

        return result;
    }


    public static Dictionary<string, object> ToGDEDict (this List<AudioClip> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.AudioClip.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 1);
        result.Add (fieldName, variable.GetPathList ());

        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<List<bool>> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Bool.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 2);

        var list = new List<List<object>> ();
        if (variable != null) {
            foreach (var sublist in variable) {
                var newSubList = new List<object> ();
                if (sublist != null) {
                    foreach (var val in sublist)
                        newSubList.Add (val.ToDictValue ());
                }
                list.Add (newSubList);
            }
        }

        result.Add (fieldName, list);
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<List<int>> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Int.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 2);

        var list = new List<List<object>> ();
        if (variable != null) {
            foreach (var sublist in variable) {
                var newSubList = new List<object> ();
                if (sublist != null) {
                    foreach (var val in sublist)
                        newSubList.Add (val.ToDictValue ());
                }
                list.Add (newSubList);
            }
        }

        result.Add (fieldName, list);
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<List<float>> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Float.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 2);

        var list = new List<List<object>> ();
        if (variable != null) {
            foreach (var sublist in variable) {
                var newSubList = new List<object> ();
                if (sublist != null) {
                    foreach (var val in sublist)
                        newSubList.Add (val.ToDictValue ());
                }
                list.Add (newSubList);
            }
        }

        result.Add (fieldName, list);
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<List<string>> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.String.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 2);

        var list = new List<List<object>> ();
        if (variable != null) {
            foreach (var sublist in variable) {
                var newSubList = new List<object> ();
                if (sublist != null) {
                    foreach (var val in sublist)
                        newSubList.Add (val.ToDictValue ());
                }
                list.Add (newSubList);
            }
        }

        result.Add (fieldName, list);
        return result;
    }

    public static Dictionary<string, object> EnumToGDEDict<T> (this List<List<T>> variable, string fieldName) where T : Enum
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), typeof(T).Name);
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 2);

        var list = new List<List<object>> ();
        if (variable != null) {
            foreach (var sublist in variable) {
                var newSubList = new List<object> ();
                if (sublist != null) {
                    foreach (var val in sublist)
                        newSubList.Add (val.ToDictValue ());
                }
                list.Add (newSubList);
            }
        }

        result.Add (fieldName, list);
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<List<Vector2>> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Vector2.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 2);

        var list = new List<List<object>> ();
        if (variable != null) {
            foreach (var sublist in variable) {
                var newSubList = new List<object> ();
                if (sublist != null) {
                    foreach (var val in sublist)
                        newSubList.Add (val.ToDictValue ());
                }
                list.Add (newSubList);
            }
        }

        result.Add (fieldName, list);
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<List<Vector3>> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Vector3.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 2);

        var list = new List<List<object>> ();
        if (variable != null) {
            foreach (var sublist in variable) {
                var newSubList = new List<object> ();
                if (sublist != null) {
                    foreach (var val in sublist)
                        newSubList.Add (val.ToDictValue ());
                }
                list.Add (newSubList);
            }
        }

        result.Add (fieldName, list);
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<List<Vector4>> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Vector4.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 2);

        var list = new List<List<object>> ();
        if (variable != null) {
            foreach (var sublist in variable) {
                var newSubList = new List<object> ();
                if (sublist != null) {
                    foreach (var val in sublist)
                        newSubList.Add (val.ToDictValue ());
                }
                list.Add (newSubList);
            }
        }

        result.Add (fieldName, list);
        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<List<Color>> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Color.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 2);

        var list = new List<List<object>> ();
        if (variable != null) {
            foreach (var sublist in variable) {
                var newSubList = new List<object> ();
                if (sublist != null) {
                    foreach (var val in sublist)
                        newSubList.Add (val.ToDictValue ());
                }
                list.Add (newSubList);
            }
        }

        result.Add (fieldName, list);
        return result;
    }

    public static Dictionary<string, object> ToGDEDict<T> (this List<List<T>> variable, string fieldName) where T : IGDEData
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), variable.SchemaName ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 2);

        if (variable != null)
            result.Add (fieldName, variable.GetKeyList ());
        else
            result.Add (fieldName, new List<List<object>> ());

        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<List<GameObject>> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.GameObject.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 2);

        if (variable != null)
            result.Add (fieldName, variable.GetPathList ());
        else
            result.Add (fieldName, new List<List<object>> ());

        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<List<Texture2D>> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Texture2D.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 2);

        if (variable != null)
            result.Add (fieldName, variable.GetPathList ());
        else
            result.Add (fieldName, new List<List<object>> ());

        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<List<Material>> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.Material.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 2);

        if (variable != null)
            result.Add (fieldName, variable.GetPathList ());
        else
            result.Add (fieldName, new List<List<object>> ());

        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<List<VideoClip>> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.VideoClip.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 2);

        if (variable != null)
            result.Add (fieldName, variable.GetPathList ());
        else
            result.Add (fieldName, new List<List<object>> ());

        return result;
    }

    public static Dictionary<string, object> ToGDEDict (this List<List<AudioClip>> variable, string fieldName)
    {
        var result = new Dictionary<string, object> ();
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.TypePrefix, fieldName), BasicFieldType.AudioClip.ToString ());
        result.Add (string.Format (GDMConstants.MetaDataFormat, GDMConstants.IsListPrefix, fieldName), 2);

        if (variable != null)
            result.Add (fieldName, variable.GetPathList ());
        else
            result.Add (fieldName, new List<List<object>> ());

        return result;
    }
}
}
