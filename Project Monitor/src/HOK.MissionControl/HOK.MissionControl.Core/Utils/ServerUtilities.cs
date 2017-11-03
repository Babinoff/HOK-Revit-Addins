﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        /// Returns Project from a Configuration Id.
        /// </summary>
        /// <param name="configId">Configuration Id.</param>
        /// <returns>Project class.</returns>
        public static Project GetProjectByConfigurationId(string configId)
        {
            Project projectFound = null;
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/projects/configid/{configid}", Method.GET);
                request.AddUrlSegment("configid", configId);

                var response = client.Execute<List<Project>>(request);
                if (response.Data != null)
                {
                    var items = response.Data;
                    projectFound = items.First();

                    Log.AppendLog(LogMessageType.INFO, "Project Found: " + projectFound.Id);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return projectFound;
        }

        /// <summary>
        /// Retrieves a Mission Control Configuration that matches given Central File path.
        /// </summary>
        /// <param name="centralPath">Central File Path.</param>
        /// <returns>Mission Control Configuration</returns>
        public static Configuration GetConfigurationByCentralPath(string centralPath)
        {
            Configuration configFound = null;
            try
            {
                var fileName = System.IO.Path.GetFileNameWithoutExtension(centralPath);
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/configurations/uri/{uri}", Method.GET);
                request.AddUrlSegment("uri", fileName);

                var response = client.Execute<List<Configuration>>(request);
                if (response.StatusCode == HttpStatusCode.InternalServerError) return null;
                if (response.Data != null)
                {
                    var items = response.Data;
                    foreach (var config in items)
                    {
                        foreach (var file in config.files)
                        {
                            if (!string.Equals(file.centralPath.ToLower(), centralPath.ToLower(),
                                StringComparison.Ordinal)) continue;
                            configFound = config;

                            Log.AppendLog(LogMessageType.INFO, "Configuration Found: " + configFound.Id);
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return configFound;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="centralPath"></param>
        /// <returns></returns>
        public static HealthReportData GetHealthRecordByCentralPath(string centralPath)
        {
            HealthReportData result = null;
            try
            {
                var fileName = System.IO.Path.GetFileNameWithoutExtension(centralPath);
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/healthrecords/uri/{uri}", Method.GET);
                request.AddUrlSegment("uri", fileName);

                var response = client.Execute<List<HealthReportData>>(request);
                if (response.StatusCode == HttpStatusCode.InternalServerError) return null;
                if (response.Data != null)
                {
                    var items = response.Data;
                    foreach (var record in items)
                    {
                        if (!string.Equals(record.centralPath.ToLower(), centralPath.ToLower())) continue;
                        result = record;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Retrieves a document from collection by it's centralPath property.
        /// </summary>
        /// <param name="centralPath">Full file path with file extension.</param>
        /// <param name="path">HTTP request url.</param>
        /// <returns>Document if found matching central path or type.</returns>
        public static T GetByCentralPath<T>(string centralPath, string path)
        {
            var result = default(T);
            try
            {
                // (Konrad) Since we cannot pass file path with "\" they were replaced with illegal pipe char "|".
                // Since pipe cannot be used in a legal file path, it's a good placeholder to use. 
                var filePath = centralPath.Replace('\\', '|');
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/" + path + "/" + filePath, Method.GET);


                var response = client.Execute(request);
                if (response.StatusCode == HttpStatusCode.InternalServerError) return result;

                if (!string.IsNullOrEmpty(response.Content))
                {
                    var data = JsonConvert.DeserializeObject<List<T>>(response.Content).FirstOrDefault();
                    if (data != null) result = data;
                    Log.AppendLog(LogMessageType.ERROR, "Could not find a document with matching central path.");
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="centralPath"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        // TODO: This needs to be redone!
        public static List<T> GetDataByCentralPath<T>(string centralPath, string path)
        {
            var result = new List<T>();
            try
            {
                var fileName = System.IO.Path.GetFileNameWithoutExtension(centralPath);
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/" + path + "/uri/{uri}", Method.GET);
                request.AddUrlSegment("uri", fileName);

                var response = client.Execute<List<T>>(request);
                if (response.StatusCode == HttpStatusCode.InternalServerError) return result;
                if (response.Data != null)
                {
                    var items = response.Data;
                    return items;
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Retrieves a Collection from MongoDB.
        /// </summary>
        /// <typeparam name="T">Type of response class.</typeparam>
        /// <param name="responseType">Response object type.</param>
        /// <param name="route">Route to post request to.</param>
        /// <returns></returns>
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
        public static void Put<T>(T body, string route)
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
                Log.AppendLog(LogMessageType.INFO, response.ResponseStatus + route);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
        }

        /// <summary>
        /// POSTs any new data Schema. Creates new Collection in MongoDB.
        /// </summary>
        /// <returns>Newly created Collection Schema with MongoDB assigned Id.</returns>
        public static T Post<T>(object body, string route) where T : new()
        {
            var resresponse = default(T);
            try
            {
                var client = new RestClient(RestApiBaseUrl);
                var request = new RestRequest(ApiVersion + "/"+ route, Method.POST);
                request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
                request.AddHeader("Content-type", "application/json");
                request.RequestFormat = DataFormat.Json;
                request.AddBody(body);

                var response = client.Execute<T>(request);
                if (response.Data != null)
                {
                    resresponse = response.Data;
                    Log.AppendLog(LogMessageType.INFO, response.ResponseStatus + "-" + route);
                }
                else
                {
                    resresponse = default(T);
                    Log.AppendLog(LogMessageType.ERROR, response.ResponseStatus + "-" + route);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return resresponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worksetDocumentId"></param>
        /// <param name="objectId"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static HttpStatusCode UpdateSessionInfo(string worksetDocumentId, string objectId, string action)
        {
            var httpStatusCode = HttpStatusCode.Unused;
            try
            {
                var restClient = new RestClient(RestApiBaseUrl);
                var restRequest = new RestRequest(ApiVersion + "/healthrecords/" + worksetDocumentId + "/sessioninfo/" + objectId, Method.PUT)
                {
                    RequestFormat = DataFormat.Json
                };
                restRequest.AddQueryParameter("action", action);
                httpStatusCode = restClient.Execute(restRequest).StatusCode;

                Log.AppendLog(LogMessageType.INFO, httpStatusCode + "-sessioninfo-" + action);
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return httpStatusCode;
        }

        /// <summary>
        /// POST Data object/Schema to MongoDB.
        /// </summary>
        /// <typeparam name="T">Data Scheme Type.</typeparam>
        /// <param name="dataSchema">Data Schema object to post.</param>
        /// <param name="collectionName">Main route name.</param>
        /// <param name="collectionId">Id of the main collection.</param>
        /// <param name="route">Action/route to execute.</param>
        /// <returns>Data Schema object returned from database.</returns>
        public static T PostToMongoDB<T>(T dataSchema, string collectionName, string collectionId, string route) where T : new()
        {
            var output = new T();
            try
            {
                var restClient = new RestClient(RestApiBaseUrl);
                var restRequest = new RestRequest(ApiVersion + "/" + collectionName+ "/" + collectionId + "/" + route, Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };
                restRequest.AddBody(dataSchema);
                var restResponse = restClient.Execute<T>(restRequest);
                if (restResponse.Data != null)
                {
                    output = restResponse.Data;

                    Log.AppendLog(LogMessageType.INFO, collectionName + "/" + route);
                }
            }
            catch (Exception ex)
            {
                Log.AppendLog(LogMessageType.EXCEPTION, ex.Message);
            }
            return output;
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