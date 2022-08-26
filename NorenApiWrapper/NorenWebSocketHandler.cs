namespace NorenRestApiWrapper
{
    public delegate void OnStreamMesssage(NorenStreamMessage Feed);
    public class BaseWSMessage
    {
        public virtual void OnMessageNotify(byte[] Data, int Count, string MessageType)
        {

        }
    }
    public class NorenStreamMessage<T> : BaseWSMessage where T : NorenStreamMessage, new()
    {
        public OnStreamMesssage MessageHandler;
        public NorenStreamMessage(OnStreamMesssage Response)
        {
            MessageHandler = Response;
        }

    }

}
