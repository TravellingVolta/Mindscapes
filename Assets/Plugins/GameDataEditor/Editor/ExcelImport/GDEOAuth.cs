using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

using Debug = UnityEngine.Debug;

namespace GameDataEditor
{
    public class GDEOAuth
    {
        const string CLIENT_ID = "835206785031-e728g5seco0r583h6sivu0iota14ars4.apps.googleusercontent.com";
        const string CLIENT_SECRET = "WuxBy5qFjoy6XWVvlFTS4sdD";
        const string SCOPE = "https://www.googleapis.com/auth/drive";
        const string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob";
        const string TOKEN_URL = "https://oauth2.googleapis.com/token";

        const int ACCESS_TOKEN_TIMEOUT = 3600;

        public struct OAuth2Parameters
        {
            public string ClientId;
            public string ClientSecret;
            public string RedirectUri;
            public string Scope;
            public string AccessToken;
            public string AccessCode;
            public string RefreshToken;
            public DateTime AccessTokenRefreshedDate;
        }

        OAuth2Parameters oauth2Params;

        public string ClientSecret
        {
            get { return oauth2Params.ClientSecret; }
        }

        public string ClientID
        {
            get { return oauth2Params.ClientId; }
        }

        public string AccessToken
        {
            get { return oauth2Params.AccessToken; }
            set { }
        }

        public GDEOAuth()
        {
            oauth2Params = new OAuth2Parameters();

            oauth2Params.ClientId = CLIENT_ID;
            oauth2Params.ClientSecret = CLIENT_SECRET;
            oauth2Params.RedirectUri = REDIRECT_URI;
            oauth2Params.Scope = SCOPE;
        }

        public bool HasAuthenticated()
        {
            return !string.IsNullOrEmpty(GDESettings.Instance.AccessTokenKey);
        }

        public string GetAuthURL()
        {
            string authURL = "https://accounts.google.com/o/oauth2/v2/auth?";
            authURL += "scope=" + oauth2Params.Scope + "&";
            authURL += "response_type=code&";
            authURL += "redirect_uri=" + oauth2Params.RedirectUri + "&";
            authURL += "client_id=" + oauth2Params.ClientId;

            return authURL;
        }

        public void SetAccessCode(string code)
        {
            oauth2Params.AccessCode = code;
            GetRefreshAndAccessTokens();
        }

        void SendRequestSync(UnityWebRequest req)
        {
            req.SendWebRequest();
            while (!req.isDone) { }
        }

        void RefreshAccessToken()
        {
            var form = new WWWForm();
            form.AddField("client_id", oauth2Params.ClientId);
            form.AddField("client_secret", oauth2Params.ClientSecret);
            form.AddField("refresh_token", oauth2Params.RefreshToken);
            form.AddField("grant_type", "refresh_token");

            UnityWebRequest req = UnityWebRequest.Post(TOKEN_URL, form);
            SendRequestSync(req);

            if (!string.IsNullOrEmpty(req.error))
            {
                Debug.Log(req.error);
                return;
            }

            ProcessRefreshAccessTokenRequest(req);
        }

        void ProcessRefreshAccessTokenRequest(UnityWebRequest req)
        {
            var response = Json.Deserialize(req.downloadHandler.text) as Dictionary<string, object>;
            oauth2Params.AccessToken = response["access_token"].ToString();
            oauth2Params.AccessTokenRefreshedDate = DateTime.Now;

            SaveTokens();
        }

        void GetRefreshAndAccessTokens()
        {
            var form = new WWWForm();
            form.AddField("client_id", oauth2Params.ClientId);
            form.AddField("client_secret", oauth2Params.ClientSecret);
            form.AddField("code", oauth2Params.AccessCode);
            form.AddField("redirect_uri", oauth2Params.RedirectUri);
            form.AddField("grant_type", "authorization_code");

            UnityWebRequest req = UnityWebRequest.Post(TOKEN_URL, form);
            SendRequestSync(req);

            ProcessRefreshAndAccessTokenRequest(req);
            GDEDriveHelper.Instance.GetSpreadsheetList();
        }

        void ProcessRefreshAndAccessTokenRequest(UnityWebRequest req)
        {
            var response = Json.Deserialize(req.downloadHandler.text) as Dictionary<string, object>;
            oauth2Params.AccessToken = response["access_token"].ToString();
            oauth2Params.RefreshToken = response["refresh_token"].ToString();
            oauth2Params.AccessTokenRefreshedDate = DateTime.Now;

            SaveTokens();

            GDEDriveHelper.Instance.GetSpreadsheetList();
        }

        public void Init()
        {
            GDESettings settings = GDESettings.Instance;
            string accessToken = settings.AccessTokenKey;
            string refreshToken = settings.RefreshTokenKey;

            oauth2Params.AccessToken = accessToken;
            oauth2Params.RefreshToken = refreshToken;

            string timeString = settings.AccessTokenLastRefreshed;
            DateTime lastRefreshed = DateTime.MinValue;

            if (timeString != null && !timeString.Equals(string.Empty))
                oauth2Params.AccessTokenRefreshedDate = DateTime.Parse(timeString);
            else
                oauth2Params.AccessTokenRefreshedDate = DateTime.MinValue;

            TimeSpan timeSinceRefresh = DateTime.Now.Subtract(oauth2Params.AccessTokenRefreshedDate);
            if (HasAuthenticated() && timeSinceRefresh.TotalSeconds >= ACCESS_TOKEN_TIMEOUT)
                RefreshAccessToken();
        }

        void SaveTokens()
        {
            GDESettings settings = GDESettings.Instance;

            settings.AccessTokenLastRefreshed = oauth2Params.AccessTokenRefreshedDate.ToString();
            settings.AccessTokenKey = oauth2Params.AccessToken;
            settings.RefreshTokenKey = oauth2Params.RefreshToken;

            settings.Save();
        }
    }
}


