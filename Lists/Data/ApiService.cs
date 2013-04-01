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

namespace Lists.Data
{
    class ApiService : IDisposable
    {
        private const string URL_FORMAT_LIST  = @"http://lists.delaha.us/lists/{0}";
        private const string URL_FORMAT_ITEMS = @"http://lists.delaha.us/lists/{0}/items";
        private const string URL_FORMAT_ITEM  = @"http://lists.delaha.us/lists/{0}/items/{1}";
        private ListsDatabase _db;

        public delegate void SuccessEventHandler();
        public delegate void ErrorEventHandler(Exception e);

        public event SuccessEventHandler UpdateListItemsCompleted;
        public event ErrorEventHandler UpdateListItemsFailed;

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

        public void Dispose()
        {
            _db.Dispose();
        }

        public void GetListItems(ObservableCollection<ItemViewModel> items)
        {
            foreach (ItemViewModel dbItem in _db.Lists.OrderBy(i => i.Name))
            {
                ItemViewModel item = items.FirstOrDefault(i => i.Id == dbItem.Id);
                if (item == null)
                {
                    items.Add(dbItem);
                }
                else
                {
                    item.Name = dbItem.Name;
                    item.Needed = dbItem.Needed;
                    item.Deleted = dbItem.Deleted;
                }
            }
        }

        public void UpdateListItemsAsync(ObservableCollection<ItemViewModel> items)
        {
            RequestState state = new RequestState()
            {
                errorEvent = UpdateListItemsFailed,
                successEvent = UpdateListItemsCompleted,
                jsonHandler = (reader) =>
                {
                    JArray list = JArray.Load(reader);
                    foreach (JObject jItem in list)
                    {
                        ulong id = jItem.Value<ulong>("id");
                        string name =jItem.Value<string>("name");
                        bool needed = jItem.Value<bool>("needed");
                        bool deleted = jItem.Value<bool>("deleted");
                        DateTime lastModified = jItem.Value<DateTime>("updated_at");

                        ItemViewModel item = _db.Lists.SingleOrDefault(it => it.Id == id);
                        if (item != null)
                        {
                            item.Needed = needed;
                            item.Deleted = deleted;
                            item.LastModified = lastModified;
                        }
                        else
                        {
                            _db.Lists.InsertOnSubmit(new ItemViewModel(id, name, needed, deleted, lastModified));
                        }
                    }

                    _db.SubmitChanges(ConflictMode.ContinueOnConflict);
                    if (_db.ChangeConflicts.Count > 0)
                        System.Diagnostics.Debug.WriteLine(_db.ChangeConflicts);

                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        GetListItems(items);
                    });

                    if (UpdateListItemsCompleted != null)
                        UpdateListItemsCompleted();
                }
            };
            BeginRequestAsync(new Uri(string.Format(URL_FORMAT_ITEMS, "00000000-0000-0000-0000-000000000000")), state, "GET");
        }

        private void BeginRequestAsync(Uri url, RequestState state, string method)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.AllowReadStreamBuffering = false;
                request.Accept = "application/json";
                request.Method = method;
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
