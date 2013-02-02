using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Groceries.Data
{
    class ApiService
    {
        private const string URL_FORMAT_ITEMS = @"http://lists.delaha.us/lists/{0}/items";
        private ListsDatabase _db;

        public delegate void SuccessEventHandler();
        public delegate void ErrorEventHandler(Exception e);

        public event SuccessEventHandler UpdateGroceryItemsCompleted;
        public event ErrorEventHandler UpdatedGroceryItemsFailed;

        private class RequestState {
            public SuccessEventHandler successEvent { get; set; }
            public ErrorEventHandler errorEvent { get; set; }
            public HttpWebRequest webRequest { get; set; }
            public Action<JsonReader> jsonHandler { get; set; }
        }

        private class ListsDatabase : DataContext
        {
            public ListsDatabase() : base("datasource=isostore:/lists.sdf") { }

            public Table<ItemViewModel> Lists;
        }

        public ApiService()
        {
            _db = new ListsDatabase();
            if (!_db.DatabaseExists())
                _db.CreateDatabase();
        }

        public void GetGroceryItems(ObservableCollection<ItemViewModel> items)
        {
            foreach (ItemViewModel tmpItem in _db.Lists.OrderBy(item => item.Name))
            {
                bool found = false;
                foreach (ItemViewModel item in items)
                {
                    if (item.Id == tmpItem.Id)
                    {
                        item.Id = tmpItem.Id;
                        item.Name = tmpItem.Name;
                        item.Needed = tmpItem.Needed;
                        item.Deleted = tmpItem.Deleted;
                        found = true;
                        break;
                    }
                }
                if (!found)
                    items.Add(tmpItem);
            }
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

                        ItemViewModel item = _db.Lists.SingleOrDefault(it => it.Id == id);
                        if (item != null)
                        {
                            item.Id = id;
                            item.Name = name;
                            item.Needed = needed;
                            item.Deleted = deleted;
                        }
                        else
                        {
                            _db.Lists.InsertOnSubmit(new ItemViewModel(id, name, needed, deleted));
                        }
                    }

                    _db.SubmitChanges(ConflictMode.ContinueOnConflict);
                    if (_db.ChangeConflicts.Count > 0)
                        System.Diagnostics.Debug.WriteLine(_db.ChangeConflicts);

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        GetGroceryItems(items);
                    });

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
                request.Accept = "application/json";
                request.Method = "GET";
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
