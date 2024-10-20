using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEditor;
using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace GameDataEditor
{
    public class GDEDriveHelper
    {
        #region Cert Validation
        static RemoteCertificateValidationCallback originalValidationCallback;

        public static bool GDEOAuthValidator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        static void SetCertValidation()
        {
            // Set up cert validator
            originalValidationCallback = ServicePointManager.ServerCertificateValidationCallback;
            ServicePointManager.ServerCertificateValidationCallback = GDEOAuthValidator;
        }

        static void ResetCertValidation()
        {
            ServicePointManager.ServerCertificateValidationCallback = originalValidationCallback;
        }
        #endregion

        static GDEDriveHelper _helper;
        public static GDEDriveHelper Instance
        {
            get
            {
                if (_helper == null)
                    _helper = new GDEDriveHelper();
                return _helper;
            }
        }

        const string FILE_UPLOAD = "https://www.googleapis.com/upload/drive/v3/files/";
        const string FILE_DOWNLOAD = "https://www.googleapis.com/drive/v3/files/";
        const string FILE_QUERY = "https://www.googleapis.com/drive/v3/files?mimeType contains 'spreadsheet'&corpora=user&includeItemsFromAllDrives=true&includeTeamDriveItems=true&supportsAllDrives=true&supportsTeamDrives=true&fields=files(exportLinks,name,webContentLink,mimeType,id,trashed)";
        const string ACCESS_TOKEN = "access_token=";
        float _timeout;
        const float MAX_TIMEOUT_SEC = 120f;

        GDEOAuth oauth;

        List<string> _spreadSheetNames;
        public string[] SpreadSheetNames
        {
            get
            {
                if (_spreadSheetNames == null)
                    _spreadSheetNames = new List<string>() { "" };

                return _spreadSheetNames.ToArray();
            }
            set { }
        }

        Dictionary<string, Dictionary<string, string>> _spreadSheetLinks;

        GDEDriveHelper()
        {
            SetCertValidation();

            oauth = new GDEOAuth();
            oauth.Init();

            _spreadSheetLinks = new Dictionary<string, Dictionary<string, string>>();
            _spreadSheetNames = new List<string>();

            ResetCertValidation();
        }

        public bool HasAuthenticated()
        {
            return oauth.HasAuthenticated();
        }

        public void SetAccessCode(string code)
        {
            SetCertValidation();

            oauth.SetAccessCode(code);

            ResetCertValidation();
        }

        public void RequestAuthFromUser()
        {
            SetCertValidation();

            string authURL = oauth.GetAuthURL();

            ResetCertValidation();

            Application.OpenURL(authURL);
        }

        public string DownloadSpreadSheet(string fileName, string localName)
        {
            string localPath = FileUtil.GetUniqueTempPathInProject() + localName;

            GetSpreadsheetList();

            Dictionary<string, string> metadata;
            if (_spreadSheetLinks.TryGetValue(fileName, out metadata))
            {
                string id = GetFileId(fileName);
                string fileUrl = FILE_DOWNLOAD + id + "/export?mimeType=application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                var req = UnityWebRequest.Get(fileUrl);

                req.SetRequestHeader("authorization", "Bearer " + oauth.AccessToken);
                req.SetRequestHeader("accept", "application/json");

                req.SendWebRequest();
                while (!req.isDone) { }

                if (string.IsNullOrEmpty(req.error))
                    File.WriteAllBytes(localPath, req.downloadHandler.data);
                else
                    Debug.Log(req.error);

                ResetCertValidation();
            }
            else
            {
                Debug.LogError(GDEConstants.ErrorDownloadingSheet + fileName);
                localPath = string.Empty;
            }

            return localPath;
        }

        public string GetFileId(string fileName)
        {
            Dictionary<string, string> metadata;
            _spreadSheetLinks.TryGetValue(fileName, out metadata);
            return metadata["id"];
        }

        public void GetSpreadsheetList()
        {
            SetCertValidation();
            oauth.Init();
            string url = FILE_QUERY + "&" + ACCESS_TOKEN + oauth.AccessToken;

            using (UnityWebRequest req = UnityWebRequest.Get(url))
            {
                req.SendWebRequest();
                while (!req.isDone) { }
                ProcessFileListResponse(req);
            }
        }

        void ProcessFileListResponse(UnityWebRequest req)
        {
            if (string.IsNullOrEmpty(req.error))
            {
                Dictionary<string, object> response = Json.Deserialize(req.downloadHandler.text) as Dictionary<string, object>;
                List<object> items = response["files"] as List<object>;

                _spreadSheetLinks.Clear();
                _spreadSheetNames.Clear();

                foreach (var item in items)
                {
                    Dictionary<string, object> itemData = item as Dictionary<string, object>;

                    string mimeType = itemData["mimeType"].ToString();
                    string fileName = itemData["name"].ToString();
                    string fileUrl = string.Empty;
                    string fileId = itemData["id"].ToString();

                    if (!mimeType.Contains("spreadsheet") || itemData.ContainsKey("trashedTime"))
                        continue;

                    if (itemData.ContainsKey("exportLinks"))
                    {
                        Dictionary<string, object> exportLinks = itemData["exportLinks"] as Dictionary<string, object>;
                        foreach (var pair in exportLinks)
                        {
                            if (pair.Value.ToString().Contains("xlsx"))
                                fileUrl = pair.Value.ToString();
                        }
                    }
                    else if (itemData.ContainsKey("downloadUrl"))
                        fileUrl = itemData["downloadUrl"].ToString();
                    else
                    {
                        fileUrl = string.Empty;
                    }

                    try
                    {
                        if (!_spreadSheetNames.Contains(fileName))
                        {
                            _spreadSheetNames.Add(fileName);
                            var metadata = new Dictionary<string, string>();
                            metadata.Add("url", fileUrl);
                            metadata.Add("id", fileId);
                            _spreadSheetLinks.Add(fileName, metadata);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning(ex.ToString());
                    }
                }
            }
            else
            {
                Debug.Log(req.error);
            }
        }

        public void UploadToExistingSheet(string googleSheetName, string dummyFilePath)
        {
            try
            {
                string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                if (!_spreadSheetLinks.ContainsKey(googleSheetName))
                    GetSpreadsheetList();
                else
                    oauth.Init(); //GetSpreadsheetList calls init, so if we aren't calling it, call init now in case we need to refresh the access token

                var meta = _spreadSheetLinks[googleSheetName];
                byte[] bytes = File.ReadAllBytes(dummyFilePath);

                string url = FILE_UPLOAD + GetFileId(googleSheetName) + "?uploadType=media";
                var req = UnityWebRequest.Put(url, bytes);
                req.method = "PATCH";

                req.SetRequestHeader("Authorization", "Bearer " + oauth.AccessToken);
                req.SetRequestHeader("Accept", "application/json");
                req.SetRequestHeader("Content-Type", mimeType);

                req.SendWebRequest();
                while (!req.isDone) { }

                if (!string.IsNullOrEmpty(req.error))
                {
                    Debug.Log(req.error);
                    Debug.Log(req.downloadHandler.text);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public void UploadNewSheet(string localFilePath, string title)
        {
            try
            {
                oauth.Init();

                string url = FILE_UPLOAD + "?uploadType=resumable";
                byte[] bytes = File.ReadAllBytes(localFilePath);
                var metadata = string.Format("{{\n   \"name\": \"{0}\", \"mimeType\": \"application/vnd.google-apps.spreadsheet\" \n}}", title);
                string mimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                var req = UnityWebRequest.Put(url, metadata);
                req.method = "POST";

                req.SetRequestHeader("Authorization", "Bearer " + oauth.AccessToken);
                req.SetRequestHeader("Accept", "application/json");
                req.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
                req.SetRequestHeader("X-Upload-Content-Type", mimeType);
                req.SetRequestHeader("content-length", Encoding.UTF8.GetByteCount(metadata).ToString());


                req.SendWebRequest();
                while (!req.isDone) { }

                if (!string.IsNullOrEmpty(req.error))
                {
                    Debug.Log(req.error);
                    Debug.Log(req.downloadHandler.text);
                    return;
                }

                //Save the resumable session uri
                var sessionUri = req.GetResponseHeader("location");
                var req2 = UnityWebRequest.Put(sessionUri, bytes);
                req2.SetRequestHeader("Content-Type", mimeType);

                req2.SendWebRequest();
                while (!req2.isDone) { }

                if (!string.IsNullOrEmpty(req2.error))
                {
                    Debug.Log(req2.error);
                    Debug.Log(req2.downloadHandler.text);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        void start_timer()
        {
            _timeout = Time.realtimeSinceStartup;
        }
        float elapsed_time()
        {
            return Time.realtimeSinceStartup - _timeout;
        }
    }
}
