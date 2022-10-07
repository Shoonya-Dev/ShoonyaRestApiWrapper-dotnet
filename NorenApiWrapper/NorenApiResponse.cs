using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Http;

namespace NorenRestApiWrapper
{
    public delegate void OnResponse(NorenResponseMsg Response, bool ok);
    
    public class BaseApiResponse
    {        
        public virtual void OnMessageNotify(HttpResponseMessage httpResponse, string data)
        {

        }
        public NorenResponseMsg GetNorenMessage(string data)
        {
            NorenResponseMsg msg = new NorenResponseMsg();

            try
            {
                msg = JsonConvert.DeserializeObject<NorenResponseMsg>(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing data {ex.ToString()}");
                return null;
            }
            return msg;
        }
    }
    
    public class NorenApiResponseList<T, U> : BaseApiResponse where T : NorenListResponseMsg<U>, new()
    {
        //watchers interested in the response
        public delegate void ResponseNotify(NorenResponseMsg httpResponse); // declare a delegate  
        public ResponseNotify ResponseNotifyInstance; // create a delegate instance  
        //the handler who will process the response
        OnResponse ResponseHandler;

        public NorenApiResponseList(OnResponse Response)
        {
            ResponseHandler = Response;
        }
        public override void OnMessageNotify(HttpResponseMessage httpResponse, string data)
        {
            //T Message = Helpers.ToObject<T>(PayLoad);
            T Message = new T();

            if (Message == null)
                return;

            if (httpResponse.IsSuccessStatusCode)
            {
                try
                {                    
                    if(data[0] == '[')
                    { 
                        //json lists begin with [
                        Message.list = JsonConvert.DeserializeObject<List<U>>(data);
                        Message.stat = "Ok";
                        Message.request_time = "";
                        Message.emsg = "";
                    }
                    else
                    {
                        //error messages
                        NorenResponseMsg msg = GetNorenMessage(data);
                        Message.Copy(msg);
                        Message.stat = "Not_Ok";
                        ResponseHandler(Message, false);
                        return;
                    }

                }
                catch (JsonReaderException ex)
                {
                    Console.WriteLine($"Message Received {data}");
                    Console.WriteLine($"Error deserializing data {ex.ToString()}");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deserializing data {ex.ToString()}");
                    return;
                }
                ResponseNotifyInstance?.Invoke(Message);
                ResponseHandler(Message, true);
            }            
            else if (httpResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                NorenResponseMsg msg = new NorenResponseMsg();

                try
                {
                    msg = JsonConvert.DeserializeObject<NorenResponseMsg>(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deserializing data {ex.ToString()}");
                    return;
                }
                Message.stat = msg.stat;
                Message.emsg = msg.emsg;

                ResponseHandler(Message, false);
            }
            else 
            {
                Message.stat = httpResponse.StatusCode.ToString();
                Message.emsg = data;
                ResponseHandler(Message, false);
            }
        }
    }

    public class NorenApiResponse<T> :  BaseApiResponse where T : NorenResponseMsg, new()
    {
        //watchers interested in the response
        public delegate void ResponseNotify(NorenResponseMsg httpResponse); // declare a delegate  
        public ResponseNotify ResponseNotifyInstance; // create a delegate instance  
        //the handler who will process the response
        OnResponse ResponseHandler;

        public NorenApiResponse(OnResponse Response)
        {
            ResponseHandler = Response;
        }
        public override void OnMessageNotify(HttpResponseMessage httpResponse, string data)
        {
            //T Message = Helpers.ToObject<T>(PayLoad);
            T Message = new T();

            if (Message == null)
                return;

            if (httpResponse.IsSuccessStatusCode)
            {
                try
                {                    
                    Message = JsonConvert.DeserializeObject<T>(data);
                    ResponseNotifyInstance?.Invoke(Message);
                    ResponseHandler(Message, true);                    
                } 
                catch(JsonReaderException  ex)
                {
                    Console.WriteLine($"Error deserializing data {ex.ToString()}");
                    return;
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error deserializing data {ex.ToString()}");
                    return;
                }
                
            }
            else if (httpResponse.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                NorenResponseMsg msg = GetNorenMessage(data);
                Message.stat = msg.stat;
                Message.emsg = msg.emsg;

                ResponseHandler(Message, false);
            }
            else
            {
                Message.stat = httpResponse.StatusCode.ToString();
                Message.emsg = data;
                ResponseHandler(Message, false);
            }

            
        }
    }

}
