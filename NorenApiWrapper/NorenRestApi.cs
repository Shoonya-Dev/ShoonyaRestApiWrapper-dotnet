
using System;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace NorenRestApiWrapper
{
    /// <summary>
    /// Callback when feed is received
    /// </summary>
    /// <param name="Feed"></param>
    public delegate void OnFeed(NorenFeed Feed);
    /// <summary>
    /// callback when order updates are received
    /// </summary>
    /// <param name="Feed"></param>
    public delegate void OnOrderFeed(NorenOrderFeed Feed);
    /// <summary>
    /// only after this call back feed and orders are to be subscribed
    /// </summary>
    /// <param name="msg"></param>
    public delegate void OnStreamConnect(NorenStreamMessage msg);    
    public delegate void OnCloseHandler();
    public delegate void OnErrorHandler(string Message);

    public class NorenRestApi
    {
        RESTClient rClient;
        NorenWebSocket wsclient;
        
        LoginResponse loginResp;
        LoginMessage loginReq;

        
        public OnResponse SessionCloseCallback
        {
            set
            {
                rClient.onSessionClose = value;
            }
        }
        public NorenRestApi()
        {            
            rClient = new RESTClient();            
        }

        public string UserToken
        {
            get
            {
                return loginResp?.susertoken;
            }
        } 

        private string getJKey
        {
            get
            {
                return "jKey=" + loginResp?.susertoken;
            }
        }
        #region response handlers
        
        internal void OnLoginResponseNotify(NorenResponseMsg responseMsg)
        {
            loginResp = responseMsg as LoginResponse;
        }
        #endregion
        #region helpers
        string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
        #endregion
        #region user login/logout
        /// <summary>
        /// This should be the first request. No further requests to be sent before login success
        /// </summary>
        /// <param name="response"></param>
        /// <param name="endPoint"></param>
        /// <param name="login"></param>
        /// <returns></returns>
        public bool SendLogin(OnResponse response, string endPoint,LoginMessage login)
        {
            loginReq = login;
            login.pwd = ComputeSha256Hash(login.pwd);
            

            login.appkey = ComputeSha256Hash(login.uid + "|" + login.appkey);

            rClient.endPoint = endPoint;
            string uri = "QuickAuth";
            var ResponseHandler = new NorenApiResponse<LoginResponse>(response);
            ResponseHandler.ResponseNotifyInstance += OnLoginResponseNotify;


            rClient.makeRequest(ResponseHandler, uri, login.toJson());
            return true;
        }

        public bool SetSession(string endpoint, string uid, string pwd, string usertoken)
        {
            rClient.endPoint = endpoint;

            loginReq = new LoginMessage();
            loginReq.uid = uid;
            loginReq.pwd = pwd;
            loginReq.source = "API";

            loginResp = new LoginResponse();                      
            loginResp.actid = uid;
            loginResp.susertoken = usertoken;

            return true;
        }
        /// <summary>
        /// Logout the current user
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public bool SendLogout(OnResponse response)
        {
            if (loginResp == null)
                return false;

            LogoutMessage logout = new LogoutMessage();
            logout.uid = loginReq.uid;
            
            string uri = "Logout";
            var ResponseHandler = new NorenApiResponse<LogoutResponse>(response);
            rClient.makeRequest(ResponseHandler, uri, logout.toJson(), getJKey);
            return true;
        }

        public bool Changepwd(OnResponse response, Changepwd changepwd)
        {
            if (loginResp == null)
                return false;

            
            changepwd.uid = loginReq.uid;
            changepwd.oldpwd = ComputeSha256Hash(changepwd.oldpwd);
            string uri = "Changepwd";
            var ResponseHandler = new NorenApiResponse<ChangepwdResponse>(response);
            rClient.makeRequest(ResponseHandler, uri, changepwd.toJson());
            return true;
        }
        public bool SendProductConversion(OnResponse response, ProductConversion productConversion)
        {
            if (loginResp == null)
                return false;

            productConversion.uid = loginReq.uid;
            
            string uri = "ProductConversion";
            var ResponseHandler = new NorenApiResponse<ProductConversionResponse>(response);
            rClient.makeRequest(ResponseHandler, uri, productConversion.toJson(), getJKey);
            return true;
        }
        public bool SendForgotPassword(OnResponse response, string endpoint, string user, string pan, string dob)
        {            
            ForgotPassword forgotPassword = new ForgotPassword();
            forgotPassword.uid = user;
            forgotPassword.pan = pan;
            forgotPassword.dob = dob;

            string uri = "ForgotPassword";
            rClient.endPoint = endpoint;
            var ResponseHandler = new NorenApiResponse<ForgotPasswordResponse>(response);
            rClient.makeRequest(ResponseHandler, uri, forgotPassword.toJson());
            return true;
        }

        #endregion
        public bool SendGetUserDetails(OnResponse response)
        {
            if (loginResp == null)
                return false;

            UserDetails userDetails = new UserDetails();
            userDetails.uid  = loginReq.uid;
            string uri = "UserDetails";
            
            rClient.makeRequest(new NorenApiResponse<UserDetailsResponse>(response), uri, userDetails.toJson(), getJKey);
            return true;
        }
        public bool SendGetMWList(OnResponse response)
        {
            if (loginResp == null)
                return false;

            UserDetails userDetails = new UserDetails();
            userDetails.uid = loginReq.uid;
            string uri = "MWList";

            rClient.makeRequest(new NorenApiResponse<MWListResponse>(response), uri, userDetails.toJson(), getJKey);
            return true;
        }
        public bool SendSearchScrip(OnResponse response, string exch, string searchtxt)
        {
            if (loginResp == null)
                return false;

            SearchScrip searchScrip = new SearchScrip();
            searchScrip.uid = loginReq.uid;
            searchScrip.exch = exch;
            searchScrip.stext = searchtxt;
            string uri = "SearchScrip";

            rClient.makeRequest(new NorenApiResponse<SearchScripResponse>(response), uri, searchScrip.toJson(), getJKey);
            return true;
        }
        
        public bool SendGetSecurityInfo(OnResponse response, string exch, string token)
        {
            if (loginResp == null)
                return false;

            GetSecurityInfo getsecurityinfo = new GetSecurityInfo();
            getsecurityinfo.uid = loginReq.uid;
            getsecurityinfo.exch = exch;
            getsecurityinfo.token = token;
            string uri = "GetSecurityInfo";

            rClient.makeRequest(new NorenApiResponse<GetSecurityInfoResponse>(response), uri, getsecurityinfo.toJson(), getJKey);
            return true;
        }
        public bool SendAddMultiScripsToMW(OnResponse response, string watchlist, string scrips)
        {
            if (loginResp == null)
                return false;

            AddMultiScripsToMW addMultiScripsToMW = new AddMultiScripsToMW();
            addMultiScripsToMW.uid  = loginReq.uid;
            addMultiScripsToMW.wlname = watchlist;
            addMultiScripsToMW.scrips = scrips;
            string uri = "AddMultiScripsToMW";

            rClient.makeRequest(new NorenApiResponse<StandardResponse>(response), uri, addMultiScripsToMW.toJson(), getJKey);
            return true;
        }
        public bool SendDeleteMultiMWScrips(OnResponse response, string watchlist, string scrips)
        {
            if (loginResp == null)
                return false;

            AddMultiScripsToMW addMultiScripsToMW = new AddMultiScripsToMW();
            addMultiScripsToMW.uid = loginReq.uid;
            addMultiScripsToMW.wlname = watchlist;
            addMultiScripsToMW.scrips = scrips;
            string uri = "DeleteMultiMWScrips";

            rClient.makeRequest(new NorenApiResponse<StandardResponse>(response), uri, addMultiScripsToMW.toJson(), getJKey);
            return true;
        }
        #region order methods
        public bool SendPlaceOrder(OnResponse response ,PlaceOrder order)
        {
            if (loginResp == null)
                return false;

            string uri = "PlaceOrder";

            rClient.makeRequest(new NorenApiResponse<PlaceOrderResponse>(response), uri, order.toJson(), getJKey);
            return true;
        }
        public bool SendModifyOrder(OnResponse response, ModifyOrder order)
        {
            if (loginResp == null)
                return false;

            order.uid = loginReq.uid;
            string uri = "ModifyOrder";

            rClient.makeRequest(new NorenApiResponse<ModifyOrderResponse>(response), uri, order.toJson(), getJKey);
            return true;            
        }
        public bool SendCancelOrder(OnResponse response, string norenordno)
        {
            if (loginResp == null)
                return false;

            string uri = "CancelOrder";
            CancelOrder cancelOrder = new CancelOrder();
            cancelOrder.norenordno = norenordno;
            cancelOrder.uid = loginReq.uid;

            rClient.makeRequest(new NorenApiResponse<CancelOrderResponse>(response), uri, cancelOrder.toJson(), getJKey);
            return true;
        }
        public bool SendExitSNOOrder(OnResponse response, string norenordno, string product)
        {
            if (loginResp == null)
                return false;

            string uri = "ExitSNOOrder";
            ExitSNOOrder exitSNOOrder = new ExitSNOOrder();
            exitSNOOrder.norenordno = norenordno;
            exitSNOOrder.prd = product;
            exitSNOOrder.uid = loginReq.uid;

            rClient.makeRequest(new NorenApiResponse<ExitSNOOrderResponse>(response), uri, exitSNOOrder.toJson(), getJKey);
            return true;
        }
        public bool SendGetOrderBook(OnResponse response, string product)
        {
            if (loginResp == null)
                return false;

            string uri = "OrderBook";
            OrderBook orderbook = new OrderBook();
            orderbook.uid = loginReq.uid;
            orderbook.prd = product;
            rClient.makeRequest(new NorenApiResponseList<OrderBookResponse, OrderBookItem>(response), uri, orderbook.toJson(), getJKey);
            return true;
        }

        public bool SendGetMultiLegOrderBook(OnResponse response, string product)
        {
            if (loginResp == null)
                return false;

            string uri = "MultiLegOrderBook";
            MultiLegOrderBook mlorderbook = new MultiLegOrderBook();
            mlorderbook.uid = loginReq.uid;
            mlorderbook.prd = product;

            rClient.makeRequest(new NorenApiResponseList<MultiLegOrderBookResponse, MultiLegOrderBookItem>(response), uri, mlorderbook.toJson(), getJKey);
            return true;
        }
        public bool SendGetTradeBook(OnResponse response, string account)
        {
            if (loginResp == null)
                return false;

            string uri = "TradeBook";
            TradeBook tradebook = new TradeBook();
            tradebook.uid = loginReq.uid;
            tradebook.actid = account;
            //tradebook.prd = product;            

            rClient.makeRequest(new NorenApiResponseList<TradeBookResponse, TradeBookItem>(response), uri, tradebook.toJson(), getJKey);
            return true;
        }
        public bool SendGetOrderHistory(OnResponse response, string norenordno)
        {
            if (loginResp == null)
                return false;

            string uri = "SingleOrdHist";
            SingleOrdHist orderhistory = new SingleOrdHist();
            orderhistory.uid = loginReq.uid;
            orderhistory.norenordno = norenordno;

            rClient.makeRequest(new NorenApiResponseList<OrderHistoryResponse, SingleOrdHistItem>(response), uri, orderhistory.toJson(), getJKey);
            return true;
        }
        public bool SendGetPositionBook(OnResponse response, string account)
        {
            if (loginResp == null)
                return false;

            string uri = "PositionBook";
            PositionBook positionBook = new PositionBook();
            positionBook.uid   = loginReq.uid;
            positionBook.actid = account;

            rClient.makeRequest(new NorenApiResponseList<PositionBookResponse, PositionBookItem>(response), uri, positionBook.toJson(), getJKey);
            return true;
        }
        public bool SendGetHoldings(OnResponse response, string account, string product)
        {
            if (loginResp == null)
                return false;

            string uri = "Holdings";
            Holdings holdings = new Holdings();
            holdings.uid = loginReq.uid;
            holdings.actid = account;
            holdings.prd = product;

            rClient.makeRequest(new NorenApiResponseList<HoldingsResponse, HoldingsItem>(response), uri, holdings.toJson(), getJKey);
            return true;
        }
        public bool SendGetLimits(OnResponse response, string account, string product = "", string segment = "", string exchange = "")
        {
            if (loginResp == null)
                return false;

            Limits limits = new Limits();
            limits.actid = account;
            limits.uid = loginReq.uid;
            if(product != "")
                limits.prd = product;
            if (segment != "")
                limits.seg = segment;
            if (exchange != "")
                limits.exch = exchange;
            string uri = "Limits";

            rClient.makeRequest(new NorenApiResponse<LimitsResponse>(response), uri, limits.toJson(), getJKey);
            return true;
        }

        public bool SendGetOrderMargin(OnResponse response, OrderMargin order)
        {
            if (loginResp == null)
                return false;
            string uri = "GetOrderMargin";
            rClient.makeRequest(new NorenApiResponse<GetOrderMarginResponse>(response), uri, order.toJson(), getJKey);
            return true;
        }

        public bool SendGetBasketMargin(OnResponse response, BasketMargin basket)
        {
            if (loginResp == null)
                return false;
            string uri = "GetBasketMargin";

            rClient.makeRequest(new NorenApiResponse<GetBasketMarginResponse>(response), uri, basket.toJson(), getJKey);
            return true;
        }

        #endregion
        #region others
        public bool SendGetExchMsg(OnResponse response, ExchMsg exchmsg)
        {
            if (loginResp == null)
                return false;

            string uri = "ExchMsg";

            rClient.makeRequest(new NorenApiResponseList<ExchMsgResponse, ExchMsgItem>(response), uri, exchmsg.toJson(), getJKey);
            return true;
        }
        #endregion
        #region feed methods
        public bool SendGetQuote(OnResponse response, string exch, string token)
        {
            if (loginResp == null)
                return false;
            string uri = "GetQuotes";

            Quote quote = new Quote();

            quote.uid = loginReq.uid;
            quote.exch = exch;
            quote.token = token;

            rClient.makeRequest(new NorenApiResponse<GetQuoteResponse>(response), uri, quote.toJson(), getJKey);
            return true;
        }

        public bool SendGetTPSeries(OnResponse response, string exch, string token, string starttime = null, string endtime = null, string interval = null)
        {
            if (loginResp == null)
                return false;
            string uri = "TPSeries";

            TPSeries quote = new TPSeries();

            quote.uid = loginReq.uid;
            quote.exch = exch;
            quote.token = token;
            if (String.IsNullOrEmpty(starttime) != true)
                quote.st = starttime;
            if(String.IsNullOrEmpty(endtime) != true)
                quote.et = endtime;
            if (String.IsNullOrEmpty(interval) != true)
                quote.intrv = interval;

            rClient.makeRequest(new NorenApiResponseList<GetTPSeriesResponse, TPSeriesItem>(response), uri, quote.toJson(), getJKey);
            return true;
        }

        public bool SendGetIndexList(OnResponse response, string exch)
        {
            if (loginResp == null)
                return false;

            string uri = "GetIndexList";

            IndexList index = new IndexList();

            index.uid = loginReq.uid;
            index.exch = exch;

            rClient.makeRequest(new NorenApiResponse<GetIndexListResponse>(response), uri, index.toJson(), getJKey);
            return true;
        }

        public bool GetDailyTPSeries(OnResponse response, string endpoint, string exch, string token, string starttime, string endtime)
        {           
            return true;
        }

        public bool SendGetOptionChain(OnResponse response, string exch, string tsym, string strprc, int count)
        {
            if (loginResp == null)
                return false;

            string uri = "GetOptionChain";

            OptionChain options = new OptionChain();

            options.uid = loginReq.uid;
            options.exch = exch;
            options.tsym = tsym;
            options.strprc = strprc;
            options.cnt = count.ToString();

            rClient.makeRequest(new NorenApiResponse<OptionChainResponse>(response), uri, options.toJson(), getJKey);
            return true;
        }

        #endregion

        public bool ConnectWatcher(string url, OnFeed marketdataHandler, OnOrderFeed orderHandler)
        {
            wsclient = new NorenWebSocket();
            wsclient.onStreamConnectCallback = this.onStreamConnectCallback;
            wsclient.onStreamCloseCallback = this.onStreamCloseCallback;
            wsclient.onStreamErrorCallback = this.onStreamErrorCallback;

            wsclient.Start(url, loginReq.uid, loginResp?.susertoken, marketdataHandler, orderHandler);

            return true;
        }

        public void CloseWatcher()
        {
            wsclient?.Stop();            
        }

        public OnStreamConnect onStreamConnectCallback
        {
            get;
            set;
        }
       
        public OnCloseHandler onStreamCloseCallback
        {
            get;
            set;
        }
        public OnErrorHandler onStreamErrorCallback
        {
            get;
            set;
        }

        /// <summary>
        /// Subscribes to the token of interest
        /// </summary>
        /// <param name="exch"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool SubscribeToken(string exch, string token)
        {
            SubscribeTouchline subs = new SubscribeTouchline();

            subs.k = exch + "|" + token;

            wsclient.Send(subs.toJson());
            Console.WriteLine($"Sub Token: {subs.toJson()}");
            return true;
        }

        /// <summary>
        /// Subscribe 
        /// </summary>
        /// <param name="tokenlist"></param>
        /// <returns></returns>
        public bool SubscribeToken(List<Quote> tokenlist)
        {
            SubscribeTouchline subs = new SubscribeTouchline();
            subs.k = String.Empty;
            foreach (var quote in tokenlist)
            {
                if (String.IsNullOrEmpty(subs.k))
                    subs.k = quote.exch + "|" + quote.token;
                else
                    subs.k += "#" + quote.exch + "|" + quote.token;
            }
            wsclient.Send(subs.toJson());
            Console.WriteLine($"Sub Token: {subs.toJson()}");
            return true;
        }

        /// <summary>
        /// Subscribe 
        /// </summary>
        /// <param name="tokenlist"></param>
        /// <returns></returns>
        public bool SubscribeTokenDepth(List<Quote> tokenlist)
        {
            SubscribeDepth subs = new SubscribeDepth();
            subs.k = String.Empty;
            foreach (var quote in tokenlist)
            {
                if (String.IsNullOrEmpty(subs.k))
                    subs.k = quote.exch + "|" + quote.token;
                else
                    subs.k += "#" + quote.exch + "|" + quote.token;
            }
            wsclient.Send(subs.toJson());
            Console.WriteLine($"Sub Depth: {subs.toJson()}");
            return true;
        }

        /// <summary>
        /// Subscribes to the token of interest
        /// </summary>
        /// <param name="exch"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool SubscribeTokenDepth(string exch, string token)
        {
            SubscribeDepth subs = new SubscribeDepth();

            subs.k = exch + "|" + token;

            wsclient.Send(subs.toJson());
            Console.WriteLine($"Sub Token Depth: {subs.toJson()}");
            return true;
        }

        /// <summary>
        /// Subscribes to the token of interest
        /// </summary>
        /// <param name="exch"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool UnSubscribeToken(string exch, string token)
        {
            UnSubscribeTouchline subs = new UnSubscribeTouchline();

            subs.k = exch + "|" + token;

            wsclient.Send(subs.toJson());
            Console.WriteLine($"UnSub Token: {subs.toJson()}");
            return true;
        }

        /// <summary>
        /// Subscribes to the token of interest
        /// </summary>
        /// <param name="exch"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool UnSubscribeTokenDepth(string exch, string token)
        {
            UnSubscribeDepth subs = new UnSubscribeDepth();

            subs.k = exch + "|" + token;

            wsclient.Send(subs.toJson());
            Console.WriteLine($"UnSub Token Depth: {subs.toJson()}");
            return true;
        }

        public bool UnSubscribe(List<Quote> tokenlist)
        {
            UnSubscribeTouchline subs = new UnSubscribeTouchline();
            subs.k = String.Empty;
            foreach (var quote in tokenlist)
            {
                if(String.IsNullOrEmpty(subs.k))
                    subs.k = quote.exch + "|" + quote.token;
                else
                    subs.k += "#" + quote.exch + "|" + quote.token;
            }
            wsclient.Send(subs.toJson());
            Console.WriteLine($"UnSub Token: {subs.toJson()}");
            return true;
        }


        public bool SubscribeOrders(OnOrderFeed orderFeed, string account)
        {            
            OrderSubscribeMessage orderSubscribe = new OrderSubscribeMessage();
            orderSubscribe.actid = account;
            wsclient.Send(orderSubscribe.toJson());

            Console.WriteLine($"Sub Order: {orderSubscribe.toJson()}");
            return true;
        }
        
        
    }
}
