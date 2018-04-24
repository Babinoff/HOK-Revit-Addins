﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas;

namespace HOK.MissionControl.Core.Utils
{
    /// <summary>
    /// Different states of the Revit model.
    /// </summary>
    public enum WorksetMonitorState
    {
        onopened,
        onsynched
    }

    public static class ServerUtilities
    {
        public static bool UseLocalServer = true;
        //public const string RestApiBaseUrl = "http://hok-184vs/";
        public const string RestApiBaseUrl = "http://localhost:8080/";
        public const string ApiVersion = "api/v1";

        #region GET

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool Get<T>(string path, out T result) where T : new()
        {
            result = default(T);
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/" + path, Method.GET);
                var response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK) return false;

                if (!string.IsNullOrEmpty(response.Content))
                {
                    var data = JsonConvert.DeserializeObject<List<T>>(response.Content).FirstOrDefault();
                    if (data != null)
                    {
                        result = data;
                        return true;
                    }

                    Log.AppendLog(LogMessageType.ERROR, "Could not find a document with matching central path.");
                }

                return false;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Retrieves a document from collection by it's centralPath property.
        /// </summary>
        /// <param name="centralPath">Full file path with file extension.</param>
        /// <param name="path">HTTP request url.</param>
        /// <param name="result"></param>
        /// <returns>Document if found matching central path or type.</returns>
        public static bool GetByCentralPath<T>(string centralPath, string path, out T result) where T : new()
        {
            result = new T();
            try
            {
                // (Konrad) Since we cannot pass file path with "\" they were replaced with illegal pipe char "|".
                // Since pipe cannot be used in a legal file path, it's a good placeholder to use.
                // File path can also contain forward slashes for RSN and BIM 360 paths ex: RSN:// and BIM 360://
                string filePath;
                if (centralPath.Contains(@"\")) filePath = centralPath.Replace(@"\", "|");
                else if (centralPath.Contains(@"/")) filePath = centralPath.Replace(@"/", "|");
                else
                {
                    Log.AppendLog(LogMessageType.ERROR, "Could not replace \\ or / with | in the file path. Exiting.");
                    return false;
                }

                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/" + path + "/" + filePath, Method.GET);
                var response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.OK) return false;
                if (!string.IsNullOrEmpty(response.Content))
                {
                    var data = JsonConvert.DeserializeObject<List<T>>(response.Content).FirstOrDefault();
                    if (data != null)
                    {
                        result = data;
                        return true;
                    }

                    Log.AppendLog(LogMessageType.ERROR, "Could not find a document with matching central path.");
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Retrieves all objects in a collection by route.
        /// </summary>
        /// <typeparam name="T">Type to be returned.</typeparam>
        /// <param name="route">Route to make the request to.</param>
        /// <returns>A List of Type objects if any were found.</returns>
        public static async Task<List<T>> FindAll<T>(string route) where T : new()
        {
            var client = new RestClient(RestApiBaseUrl);
            var request = new RestRequest(ApiVersion + "/" + route, Method.GET);
            var response = await client.ExecuteTaskAsync<List<T>>(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, response.StatusDescription);
                return new List<T>();
            }

            Log.AppendLog(LogMessageType.INFO, response.StatusDescription + "-" + route);
            return response.Data;
        }

        /// <summary>
        /// Retrieves a Collection from MongoDB.
        /// </summary>
        /// <typeparam name="T">Type of response class.</typeparam>
        /// <param name="responseType">Response object type.</param>
        /// <param name="route">Route to post request to.</param>
        /// <returns></returns>
        [Obsolete]
        public static List<T> FindAll<T>(T responseType, string route) where T : new()
        {
            var items = new List<T>();
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/" + route, Method.GET);
                var response = client.Execute<List<T>>(request);
                if (response.Data != null)
                {
                    items = response.Data;

                    Log.AppendLog(LogMessageType.INFO, response.ResponseStatus + "-" + route);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return items;
        }

        /// <summary>
        /// Retrieves a Collection from MongoDB.
        /// </summary>
        /// <typeparam name="T">Type of response class.</typeparam>
        /// <param name="route">Route to post request to.</param>
        /// <returns></returns>
        public static T FindOne<T>(string route) where T : new()
        {
            var items = default(T);
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/" + route, Method.GET);
                var response = client.Execute<T>(request);
                if (response.Data != null)
                {
                    items = response.Data;

                    Log.AppendLog(LogMessageType.INFO, response.ResponseStatus + "-" + route);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return items;
        }

        #endregion

        #region POST

        /// <summary>
        /// Posts Trigger Records to MongoDB. Trigger records are created when users override DTM Tools.
        /// </summary>
        /// <param name="record">Record to post.</param>
        public static HttpStatusCode PostTriggerRecords(TriggerRecord record)
        {
            var status = HttpStatusCode.Unused;
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request =
                    new RestRequest(ApiVersion + "/triggerrecords", Method.POST) { RequestFormat = DataFormat.Json };
                request.AddBody(record);

                var response = client.Execute<TriggerRecord>(request);
                status = response.StatusCode;

                Log.AppendLog(LogMessageType.INFO, response.ResponseStatus + "-triggerrecords");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return status;
        }

        /// <summary>
        /// PUTs created Health Record Id into Project's healthrecords array.
        /// </summary>
        /// <param name="project">Project class.</param>
        /// <param name="id"></param>
        public static void AddHealthRecordToProject(Project project, string id)
        {
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(
                    ApiVersion + "/projects/" + project.Id + "/addhealthrecord/" + id, Method.PUT)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddBody(project);
                var response = client.Execute(request);
                Log.AppendLog(LogMessageType.INFO, response.ResponseStatus + "-addhealthrecord");
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// PUTs body
        /// </summary>
        /// <param name="body"></param>
        /// <param name="route"></param>
        public static bool Put<T>(T body, string route)
        {
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(
                    ApiVersion + "/" + route, Method.PUT)
                {
                    RequestFormat = DataFormat.Json
                };
                request.AddBody(body);
                var response = client.Execute(request);
                if (response.StatusCode != HttpStatusCode.Created)
                {
                    Log.AppendLog(LogMessageType.INFO, response.ResponseStatus + route);
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// POSTs any new data Schema. Creates new Collection in MongoDB.
        /// </summary>
        /// <typeparam name="T">Type of return data.</typeparam>
        /// <param name="body">Body of rest call.</param>
        /// <param name="route">Route to be called.</param>
        /// <returns>Newly created Collection Schema with MongoDB assigned Id.</returns>
        public static async Task<T> PostAsync<T>(object body, string route) where T : new()
        {
            var client = new RestClient(RestApiBaseUrl);
            client.ClearHandlers();
            client.AddHandler("application/json", new NewtonsoftJsonSerializer());
            client.AddHandler("text/json", new NewtonsoftJsonSerializer());
            client.AddHandler("text/x-json", new NewtonsoftJsonSerializer());
            client.AddHandler("text/javascript", new NewtonsoftJsonSerializer());
            client.AddHandler("*+json", new NewtonsoftJsonSerializer());

            var request = new RestRequest(ApiVersion + "/" + route, Method.POST);
            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
            request.AddHeader("Content-type", "application/json");
            request.RequestFormat = DataFormat.Json;
            request.JsonSerializer = new NewtonsoftJsonSerializer();
            request.AddBody(body);

            var response = await client.ExecuteTaskAsync<T>(request);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, response.StatusDescription);
                return new T();
            }

            Log.AppendLog(LogMessageType.INFO, response.StatusDescription + "-" + route);
            return response.Data;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="body"></param>
        /// <param name="route"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool Post<T>(object body, string route, out T result) where T : new()
        {
            result = default(T);
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                client.ClearHandlers();
                client.AddHandler("application/json", new NewtonsoftJsonSerializer());
                client.AddHandler("text/json", new NewtonsoftJsonSerializer());
                client.AddHandler("text/x-json", new NewtonsoftJsonSerializer());
                client.AddHandler("text/javascript", new NewtonsoftJsonSerializer());
                client.AddHandler("*+json", new NewtonsoftJsonSerializer());

                var request = new RestRequest(ApiVersion + "/"+ route, Method.POST);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                request.AddHeader("Content-type", "application/json");
                request.RequestFormat = DataFormat.Json;
                request.JsonSerializer = new NewtonsoftJsonSerializer();
                request.AddBody(body);

                var response = client.Execute<T>(request);
                if (response.StatusCode != HttpStatusCode.Created) return false;
                if (response.Data != null)
                {
                    result = response.Data;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
                return false;
            }
        }

        #endregion

        #region Revit Server REST API

        /// <summary>
        /// Retrieves information about the model, inclusing its file size.
        /// </summary>
        /// <param name="clientPath">Base URL to the Revit Server.</param>
        /// <param name="requestPath">Request string.</param>
        /// <returns>File size in bytes.</returns>
        public static int GetFileInfoFromRevitServer(string clientPath, string requestPath)
        {
            var size = 0;
            try
            {
                var client = new RestClient(clientPath);
                var request = new RestRequest(requestPath, Method.GET);
                request.AddHeader("User-Name", Environment.UserName);
                request.AddHeader("User-Machine-Name", Environment.UserName + "PC");
                request.AddHeader("Operation-GUID", Guid.NewGuid().ToString());

                var response = client.Execute<RsFileInfo>(request);
                if (response.Data != null)
                {
                    size = response.Data.ModelSize;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return size;
        }

        #endregion
    }
}