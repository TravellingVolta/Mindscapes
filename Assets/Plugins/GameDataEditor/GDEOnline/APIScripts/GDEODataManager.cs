using UnityEngine;
using UnityEngine.Networking;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using static System.Convert;
using System.Linq;

#if !UNITY_WEBPLAYER
namespace GameDataEditor
{
public struct RuntimeDataSetInfo
{
	public string ID;
	public DateTime CreateDate;
	public DateTime ModDate;
}

public partial class GDEDataManager
{
	public static HashSet<string> DataSetIDCache = new HashSet<string>();

	/// <summary>
	/// Loads the specified GDE dataset from GDE Online using the GDEOConfig present
	/// in the project
	/// </summary>
	/// <returns><c>true</c>, if the dataset was loaded, <c>false</c> otherwise.</returns>
	public static bool GDEOInit ()
	{
		bool result = false;

		if (!GDEOConfig.ConfigSet())
		{
			Debug.LogError(GDMConstants.ErrorConfigNotSet);
			return false;
		}
		
		try
		{
			string url = GDEOConfig.PrimaryDataUrl();
			masterData = null;

			var request = UnityWebRequest.Get(url);
			request.SetRequestHeader("x-api-key", GDEOConfig.APIKey);

			request.SendWebRequest();

			while (!request.isDone) {}

			if (request.responseCode == 200)
			{
				GDEODebug.Log(request.downloadHandler.text);
				var body = Json.Deserialize(request.downloadHandler.text) as Dictionary<string, object>;
				masterData = body["masterds"] as Dictionary<string, object>;

				BuildDataKeysBySchemaList ();
				result = true;
			}
			else
			{
				GDEODebug.Log ("http_resp_code: " + request.responseCode);
				GDEODebug.Log (request.error);
				result = false;
			}
		}
		catch (Exception ex)
		{
			GDEODebug.LogException(ex);
			GDEODebug.Log ("Error fetching master data set.");
			result = false;
		}

		return result;
	}

	/// <summary>
	/// Uploads the current user data to GDE Online
	/// </summary>
	/// <param name="uid">Unique User ID</param>
	public static bool GDEOSave (string playerID)
	{
		bool result = false;

		if (!GDEOConfig.ConfigSet())
		{
			Debug.LogError(GDMConstants.ErrorConfigNotSet);
			return false;
		}

		try
		{
			using (var stream = new MemoryStream())
			{
				BinaryFormatter bin = new BinaryFormatter ();
				bin.Serialize (stream, ModifiedData);
				var moddataArray = stream.ToArray();
				File.WriteAllBytes(modifiedDataPath, moddataArray);

				string datasetID = GDEOKeyGen.GetUniqueKey(5);
				var url = GDEOConfig.RuntimeDataUrl() + datasetID;

				var formData = new List<IMultipartFormSection>();
				formData.Add(new MultipartFormDataSection("id", datasetID));
				formData.Add(new MultipartFormDataSection("name", playerID));
				formData.Add(new MultipartFormDataSection("username", GDEOConfig.DevID));
				formData.Add(new MultipartFormDataSection("gameid", GDEOConfig.GameID));
				formData.Add(new MultipartFormFileSection("rdsBytes", moddataArray, modifiedDataPath, "application/octet-stream"));

				string modJson = Json.Serialize(GDEDataManager.ModifiedData);
				formData.Add(new MultipartFormFileSection("rdsJson", modJson, Encoding.UTF8, modifiedDataPath));
				GDEODebug.Log(modJson);

				var request = UnityWebRequest.Post(url, formData);
				request.SetRequestHeader("Accept", "application/json, text/plain, */*");
				request.SetRequestHeader("x-api-key", GDEOConfig.APIKey);

				request.SendWebRequest();
				while (!request.isDone) {}

				GDEODebug.Log(request.responseCode);

				if (request.responseCode == 200)
				{
					DataSetIDCache.Add(datasetID);
					result = true;
				}
			}
		}
		catch (Exception ex)
		{
			GDEODebug.LogError (GDMConstants.ErrorSavingData);
			GDEODebug.LogException (ex);

			result = false;
		}

		return result;
	}

	public static List<RuntimeDataSetInfo> GDEOGetRuntimeDataIDs (string uid)
	{
		List<RuntimeDataSetInfo> datasetInfos = null;

		if (!GDEOConfig.ConfigSet())
		{
			Debug.LogError(GDMConstants.ErrorConfigNotSet);
			return null;
		}

		try
		{
			string url = GDEOConfig.RuntimeDataListUrl(uid);
			var request = UnityWebRequest.Get(url);
			request.SetRequestHeader("x-api-key", GDEOConfig.APIKey);

			request.SendWebRequest();
			while (!request.isDone) {}

			GDEODebug.Log(url);
			GDEODebug.Log(request.responseCode.ToString());

			if (request.responseCode == 200)
			{
				GDEODebug.Log(request.downloadHandler.text);

				datasetInfos = new List<RuntimeDataSetInfo>();
				var responseDict = Json.Deserialize(request.downloadHandler.text) as List<object>;
				foreach (var item in responseDict)
				{
					var infoItem = item as Dictionary<string, object>;
					var temp = new RuntimeDataSetInfo();
					temp.ID = infoItem["id"].ToString();
					temp.CreateDate = DateTime.Parse(infoItem["createDate"].ToString());
					temp.ModDate = DateTime.Parse(infoItem["modDate"].ToString());
					datasetInfos.Add(temp);
				}
			}
			else
			{
				GDEODebug.Log ("http_resp_code: " + request.responseCode);
				GDEODebug.Log (request.error);
			}
		}
		catch (Exception ex)
		{
			GDEODebug.LogException(ex);
		}

		return datasetInfos;
	}

	public static bool GDEOGetRuntimeData(string rdsID, string playerID)
	{
		bool result = false;

		if (!GDEOConfig.ConfigSet())
		{
			Debug.LogError(GDMConstants.ErrorConfigNotSet);
			return false;
		}

		try
		{
			string url = GDEOConfig.RuntimeDataSetGetUrl(rdsID, playerID);

			var request = UnityWebRequest.Get(url);
			request.SetRequestHeader("x-api-key", GDEOConfig.APIKey);

			request.SendWebRequest();
			while (!request.isDone) {}

			GDEODebug.Log(url);
			GDEODebug.Log(request.responseCode);

			if (request.responseCode == 200)
			{
				GDEODebug.Log(request.downloadHandler.text);
				result = true;

				var responseDict = Json.Deserialize(request.downloadHandler.text) as Dictionary<string, object>;
				var rawbytes = ((Dictionary<string, object>)responseDict["rdsBytes"])["data"] as List<object>;
				byte[] bytes = rawbytes.Select(x => Convert.ToByte(x)).ToArray();
				using (var stream = new MemoryStream(bytes))
				{
					BinaryFormatter bin = new BinaryFormatter();
					ModifiedData = bin.Deserialize(stream) as Dictionary<string, Dictionary<string, object>>;
					File.WriteAllBytes(modifiedDataPath, stream.ToArray());
				}
			}
			else
			{
				GDEODebug.Log ("http_resp_code: " + request.responseCode);
				GDEODebug.Log (request.error);
				result = false;
			}
		}
		catch (Exception ex)
		{
			GDEODebug.LogException(ex);
			result = false;
		}

		return result;
	}

	public static bool GDEODeleteRuntimeData (string playerID, string rdsID)
	{
		if (!GDEOConfig.ConfigSet())
		{
			Debug.LogError(GDMConstants.ErrorConfigNotSet);
			return false;
		}		

		string url = GDEOConfig.RuntimeDataSetDeleteUrl(playerID, rdsID);
		var request = UnityWebRequest.Delete(url);
		try
		{
			request.SetRequestHeader("x-api-key", GDEOConfig.APIKey);

			ClearSaved();
			request.SendWebRequest();

			while (!request.isDone) {}
		}
		catch (Exception ex)
		{
			GDEODebug.LogException(ex);
		}
		return request.responseCode == 200;
	}	
}
}
#else
namespace GameDataEditor
{
public partial class GDEDataManager
{
	public static bool GDEOInit (string ID)
	{
		throw new Exception("GDE Online is not supported in Web Player!");
	}
}
}
#endif
