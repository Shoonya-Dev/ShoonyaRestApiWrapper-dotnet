using Newtonsoft.Json;
using System;

namespace NorenRestApiWrapper
{
    public class NorenWebSocket
    {
        WebSocket _ws = new WebSocket();
        string _uid;
        string _susertoken;
        string _endpoint;

        public OnStreamConnect onStreamConnectCallback;
        public OnCloseHandler onStreamCloseCallback;
        public OnErrorHandler onStreamErrorCallback;

        public OnFeed OnFeedCallback;
        public OnOrderFeed OnOrderCallback;

        #region initialize
        public NorenWebSocket()
        {            
            // Add handlers to events
            _ws.OnConnect += _onConnect;
            _ws.OnData += _onData;
            _ws.OnClose += _onClose;
            _ws.OnError += _onError;           
        }

        public void Start(string url, string uid, string susertoken, OnFeed marketdataHandler, OnOrderFeed orderHandler)
        {
            //member init
            _endpoint = url;
            _uid = uid;
            _susertoken = susertoken;
           
            //app initializers
            OnFeedCallback = marketdataHandler;
            OnOrderCallback = orderHandler;
            
            _ws.Connect(_endpoint);
        }

        public void Stop()
        {
            _ws.Close();
        }
        
        private void _onError(string Message)
        {
            Console.WriteLine($"Error websocket: {Message}");
            onStreamErrorCallback?.Invoke(Message);
        }

        private void _onClose()
        {            
            Console.WriteLine("websocket closed");
            onStreamCloseCallback?.Invoke();
        }

        private void _onConnect()
        {
            //once websocket is connected, lets create a app session
            ConnectMessage connect = new ConnectMessage();
            connect.t = "c";
            connect.uid = _uid;
            connect.actid = _uid;
            connect.susertoken = _susertoken;
            _ws.Send(connect.toJson());
            Console.WriteLine($"Create Session: {connect.toJson()}");
        }

        /// <summary>
        /// Tells whether websocket is connected to server not.
        /// </summary>
        public bool IsConnected
        {
            get { return _ws.IsConnected(); }
        }
        #endregion
        public void Send(string data)
        {
            if (_ws.IsConnected())
                _ws.Send(data);
            else
                Console.WriteLine($"send failed as websocket is not connected: {data}");
        }
        public static T Deserialize<T>(byte[] data, int count) where T : class
        {
            string str = System.Text.Encoding.UTF8.GetString(data, 0, count);
            return JsonConvert.DeserializeObject<T>(str);
        }

        private void _onData(byte[] Data, int Count, string MessageType)
        {
            NorenStreamMessage wsmsg;
            try
            {
                if (Count == 0)
                    return;
                wsmsg = Deserialize<NorenStreamMessage>(Data, Count);

                if (wsmsg.t == "ck")
                {
                    Console.WriteLine("session established");
                    onStreamConnectCallback?.Invoke(wsmsg);
                }
                else if (wsmsg.t == "om" || wsmsg.t == "ok")
                {
                    NorenOrderFeed ordermsg = Deserialize<NorenOrderFeed>(Data, Count);
                    OnOrderCallback?.Invoke(ordermsg);
                }
                else
                {
                    NorenFeed feedmsg = Deserialize<NorenFeed>(Data, Count);
                    OnFeedCallback?.Invoke(feedmsg);
                }
            }
            catch (JsonReaderException ex)
            {
                Console.WriteLine($"Error deserializing data {ex.ToString()}");
                onStreamErrorCallback?.Invoke(ex.ToString());
                return;
            }           
        }
    }

}
