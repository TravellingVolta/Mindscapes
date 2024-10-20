using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GameDataEditor
{
	public class GDEHTTPStatusCode
	{
		public const int OK = 200;
		public const int NO_CONTENT = 201;
	}

	public class GDEOHeader
	{
		public const string APIKey = "x-api-key";
		public const string MD5 = "x-md5";
		public const string Status = "STATUS";

		public const string Body = "Body";

		public const string AWSErrType = "errorType";
		public const string AWSErrMsg = "errorMessage";
	}

	public class AWSError
	{
		public string Msg = string.Empty;
		public string Type = string.Empty;
	}

	public static class UnityWebRequestExtensions
	{
		public static long StatusCode(this UnityWebRequest variable)
		{
			if (variable != null)
				return variable.responseCode;
			return -1;
		}

		public static byte[] ResponseBody(this UnityWebRequest variable)
		{
			byte[] body = null;
			if (variable != null && variable.downloadedBytes > 0)
			{
				Dictionary<string, object> response = Json.Deserialize(variable.downloadHandler.text) as Dictionary<string, object>;
				
				List<object> temp = response[GDEOHeader.Body] as List<object>;
				body = temp.ConvertAll(x => Convert.ToByte(x)).ToArray();
			}
			return body;
		}

		public static AWSError AWSError(this UnityWebRequest variable)
		{
			AWSError err = new AWSError();

			try
			{
				if (variable != null && variable.isHttpError)
				{
					err.Msg = variable.GetResponseHeader(GDEOHeader.AWSErrMsg);
					err.Type = variable.GetResponseHeader(GDEOHeader.AWSErrType);
				}
			}
			catch
			{
				/* Do nothing */
			}

			return err;
		}
	}
}
