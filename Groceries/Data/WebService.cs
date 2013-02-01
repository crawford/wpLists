using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Groceries.Data
{
    class WebService
    {
        private const string URL_FORMAT_ITEMS = @"http://lists.delaha.us/lists/{0}/items";

        public delegate void SuccessEventHandler();
        public delegate void ErrorEventHandler(Exception e);

        public event SuccessEventHandler UpdateGroceryItemsCompleted;
        public event ErrorEventHandler UpdatedGroceryItemsFailed;

        internal class RequestState {
            public SuccessEventHandler successEvent { get; set; }
            public ErrorEventHandler errorEvent { get; set; }
            public HttpWebRequest webRequest { get; set; }
            public Action<JsonReader> jsonHandler { get; set; }
        }

        public void UpdateGroceryItemsAsync(ObservableCollection<ItemViewModel> items)
        {
            RequestState state = new RequestState()
            {
                errorEvent = UpdatedGroceryItemsFailed,
                successEvent = UpdateGroceryItemsCompleted,
                jsonHandler = (reader) =>
                {
                    JArray list = JArray.Load(reader);
                    foreach (JObject jItem in list)
                    {
                        ulong id = jItem.Value<ulong>("id");
                        string name =jItem.Value<string>("name");
                        bool needed = jItem.Value<bool>("needed");
                        bool deleted = jItem.Value<bool>("deleted");

                        ItemViewModel item = null;
                        foreach (ItemViewModel i in items) {
                            if (i.Id == id) {
                                item = i;
                                break;
                            }
                        }
                        
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            if (item != null)
                            {
                                item.Id = id;
                                item.Name = name;
                                item.Needed = needed;
                                item.Deleted = deleted;
                            }
                            else
                            {
                                items.Add(new ItemViewModel(id, name, needed, deleted));
                            }
                        });
                    }

                    if (UpdateGroceryItemsCompleted != null)
                        UpdateGroceryItemsCompleted();
                }
            };
            BeginRequestAsync(new Uri(string.Format(URL_FORMAT_ITEMS, "test")), state);
        }

        private void BeginRequestAsync(Uri url, RequestState state)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.AllowReadStreamBuffering = false;
                state.webRequest = request;
                request.BeginGetResponse(new AsyncCallback(RequestCallback), state);
            }
            catch (Exception e)
            {
                if (state.errorEvent != null)
                    state.errorEvent(e);
            }
        }

        private void RequestCallback(IAsyncResult asynchronousResult)
        {
            RequestState request = (RequestState)asynchronousResult.AsyncState;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.webRequest.EndGetResponse(asynchronousResult))
                {
                    using (JsonReader reader = new JsonTextReader(new StreamReader(response.GetResponseStream())))
                    {
                        request.jsonHandler(reader);
                    }
                }
            }
            catch (WebException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Response);
                if (request.errorEvent != null)
                    request.errorEvent(e);
            }
        }
    }
}
