#region usings
using Com.Lmax.Api;
using Com.Lmax.Api.OrderBook;
using System;
using System.Threading;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Generic;
using System.IO;
#endregion

namespace websocket_server
{
    class Server : WebSocketBehavior
    {
        private static int[] debag_array = {7, 5, 1, 1, 1, 0, 4, 3, 3, 4};
        private static int mode;

        #region database
        private DatabaseConnect db_connect;
        private DatabaseOperations db_operations;
        private Dictionary<string, string> __tokens = new Dictionary<string, string>();
        private Dictionary<string, string> __tokensToClose = new Dictionary<string, string>();
        private static Thread alpariforex_Thread;
        private static Thread lmax_Thread;
        #endregion
        
        #region private variables (library)
        private ISession __session;
        private static long[] __INSTRUMENT_ID = 
            { 
                4001, 4002, 4010, 4004, 4013, 4007, 100613, 
                4006, 4003, 4005, 4015, 4016, 4008, 4017, 
                4014, 4012, 100617, 100637, 100639, 100938, 100667,
                100619, 100669, 100681, 100679, 4011, 4009
            };
        private static string[] __INSTRUMENT_NAMES = 
            {
                "EUR/USD", "GBP/USD", "USD/CHF", "USD/JPY", "USD/CAD", "AUD/USD","NZD/USD",
                "EUR/JPY", "EUR/GBP", "GBP/JPY", "EUR/CAD", "EUR/AUD", "AUD/JPY", "GBP/AUD",
                "GBP/CAD", "GBP/CHF", "NZD/JPY", "XAU/USD", "XAG/USD", "XRP/USD", "AUD/CAD", 
                "AUD/CHF", "CAD/JPY", "USD/RUB", "EUR/RUB", "EUR/CHF", "CHF/JPY"
            };
        #endregion
        
        #region private variables (general)
        private static string __brokers_url = "ws://188.119.113.137:5001/Server";
        private static volatile double[] __alpariforex_prices = new double[__INSTRUMENT_ID.Length];
        private static volatile double[] __prev_alpariforex_prices = new double[__INSTRUMENT_ID.Length];
        private static volatile double[] __lmax_prices = new double[__INSTRUMENT_ID.Length];
        private static volatile double[] __prev_lmax_prices = new double[__INSTRUMENT_ID.Length];
        private static volatile long[] __timestamps = new long[__INSTRUMENT_ID.Length];
        private static volatile long[] __prev_timestamps = new long[__INSTRUMENT_ID.Length];

        //POCKETOPTION
        private static volatile JArray __pocketOptionData;

        private static int messageWindow = 1;
        #endregion
        
        #region private functions (library)
        private void __MarketDataUpdate(OrderBookEvent orderBookEvent)
        {
			
            for(int i = 0; i<__INSTRUMENT_ID.Length;i++)
            {
                if(orderBookEvent.InstrumentId==__INSTRUMENT_ID[i]){
                    __lmax_prices[i] = ((Decimal.ToDouble(orderBookEvent.ValuationBidPrice)+Decimal.ToDouble(orderBookEvent.ValuationAskPrice))/2);
                    __timestamps[i] = orderBookEvent.Timestamp;
                }
				
				if(i == 0 && orderBookEvent.InstrumentId==__INSTRUMENT_ID[i]){
					using (StreamWriter writer = new StreamWriter("lMAX_OUTPUT.txt", true))
					{
						writer.WriteLine("CUR:"+__lmax_prices[0].ToString());
						if(__prev_lmax_prices.Length > 0){
							writer.WriteLine("PREV:"+__prev_lmax_prices[0].ToString());
                            writer.WriteLine("Diff:"+ String.Format("{0:0}", (__lmax_prices[0] - __prev_lmax_prices[0]) * 100000));
							writer.WriteLine();
						}
					}
				}
            }
        }
        private void __LoginCallback(ISession session)
        {
            __session = session; 
            __session.MarketDataChanged += __MarketDataUpdate;

            /*string query = ""; // see above for how to do a more specific search
            long offsetInstrumentId = 100937; // see above for more details on this offset parameter           
            session.SearchInstruments(new SearchRequest(query, offsetInstrumentId), SearchCallback,
                failureResponse => Console.Error.WriteLine("Failed to subscribe: {0}", failureResponse));
            */
            
            foreach(long item in __INSTRUMENT_ID)
            {
                __session.Subscribe(new OrderBookSubscriptionRequest(item),
                    () => Console.WriteLine("Successful subscription"),
                    failureResponse => Console.Error.WriteLine("Failed to subscribe: {0}", failureResponse));
            }

            __session.Start();
        }
        /*private void SearchCallback(List<Instrument> instruments, bool hasMoreResults)
        {
            Console.WriteLine("Instruments Retrieved: {0}", instruments);
            foreach (var item in instruments)
            {
                using (StreamWriter writer = new StreamWriter("lMAX_Instruments.txt", true)) //// true to append data to the file
                {
                    writer.WriteLine(item.ToString()); 
                }
            }
            if(hasMoreResults)
            {
                Console.WriteLine("To continue retrieving all instruments please start next search from: {0}", instruments[instruments.Count - 1]);
            }
        }*/
        private static OnFailure __FailureCallback(string failedFunction)
        {
            return failureResponse => Console.Error.WriteLine("Failed to " + failedFunction + " due to: " + failureResponse.Message);
        }
        #endregion
        
        #region public start functions
        public static void StartThreads()
        {
            Console.WriteLine("Server started...");

            //alpariforex_Thread = new Thread(new ThreadStart(StartBrokers));
            //alpariforex_Thread.Start();
            //Console.WriteLine("brokers thread started...");

            //lmax_Thread = new Thread(new ThreadStart(StartLmax));
            //lmax_Thread.Start();
            //Console.WriteLine("lMAX thread started...");
        }
        public static void StartLmax()
        {
            Server server = new Server();
            LmaxApi lmaxApi = new LmaxApi("https://web-order.london-demo.lmax.com");
            lmaxApi.Login(new LoginRequest("paulmindal", "kurWave2319!"), server.__LoginCallback, __FailureCallback("log in"));
        }
        public static void StartBrokers()
        {
            Console.WriteLine("StartBrokers");
            WebSocket ws = new WebSocket(url: __brokers_url);
            
            ws.EnableRedirection=true;
            ws.OnMessage += (sender, e) =>{				
                try
                {
                    JObject data = JObject.Parse(e.Data);
                    JObject json = JObject.Parse(data["alpariforex"].ToString());
                    __pocketOptionData = JArray.Parse(data["pocketOption"].ToString());

                    for(int i = 0; i<__INSTRUMENT_ID.Length;i++)
                    {
                        if(i<19 && json["body"]["data"]!=null)
                        {
                            __alpariforex_prices[i] = Convert.ToDouble(json["body"]["data"][i][1]);
                        }
                        else if (i==19)
                        {
                            JArray tmp_length = (JArray)json["body"]["data"];
                            if(tmp_length!=null && tmp_length.Count>0)
                            {
                                __alpariforex_prices[i] = Convert.ToDouble(json["body"]["data"][tmp_length.Count-1][1]);
                            }
                        }
                    }
                }
                catch(Exception ex){
                    Console.WriteLine("Server.cs - Server - StartAlpariForex - EXCEPTION:{0}",ex);
                };

                if(ws.IsAlive){
					try{
						ws.Send("click-msg");
					}
					catch(Exception ex){
						ws.Connect();
						ws.Send("click-msg");
					};
				}
				else
				{
					try{
						Console.WriteLine("CLOSED broker connetion, trying to reconnect...");
						ws.Connect();
					}
					catch(Exception ex){
						
					};
				}
				
            };
            ws.OnError += (sender, e) => {
                Console.WriteLine("ERROR:Sender: {0}, e: {1}", sender, e);
            };
            ws.OnClose += (sender, e) => {
                Console.WriteLine("CLOSED:Sender: {0}, e: {1} {2}", sender, e.Code, e.Reason); 
            };
            ws.Connect();
            if(ws.IsAlive){
                ws.Send("click-msg");
			}
			else{
				try{
					Console.WriteLine("CLOSED broker connetion, trying to reconnect...");
					Thread.Sleep(10);
					ws.Connect();
				}
				catch(Exception ex){
					
				};
			}
        }
        #endregion
        
        protected override void OnMessage(MessageEventArgs e)
        {
            if(e.Data!=null)
            {   try
                {
                    if(mode>9)
                        mode = 0;
                    else
                        mode++;
                    
                    //string formatted = e.Data.Replace(": ,", ":0,");

                    //JObject json = JObject.Parse(e.Data);
                    if(true/*json.ContainsKey("token") && json.ContainsKey("ip")*/){
                        //Console.WriteLine(json);
                        if(false/*!__tokens.ContainsKey(json["token"].ToString())*/)
                        {
                            /*db_connect = new DatabaseConnect();
                            if(db_connect.IsConnect())
                            {
                                db_operations = new DatabaseOperations(db_connect.Connection);
                                
                                if(db_operations.FindByKey(json["token"].ToString(), true))
                                {
                                    __tokens.Add(json["token"].ToString(), json["ip"].ToString());
                                }
                                else
                                {
                                    Send("{\"Error\": \"wrong token sent\"}");
                                }
                                
                                db_connect.Close();
                            }
                            else
                            {
                                Send("{\"Error\": \"no db connection\"}");
                            }*/
                        }
                        if (true/*__tokens.ContainsKey(json["token"].ToString()) && __tokens[json["token"].ToString()] == json["ip"].ToString()*/)
                        {
                            int messagesCounter = 0;
                            
                            string messageToClientLMAX = "{\"lmax\":[";
                            string messageToClientBroker = "],\"AlpariForex\":[";
                            string messageToClientPocketOption = "],\"PocketOption\":[";

                            for (int i = 0; i < __INSTRUMENT_ID.Length; i++)
                            {
                                double lmaxImpulse = debag_array[mode];//__lmax_prices[i] - __prev_lmax_prices[i];

                                messageToClientLMAX = messageToClientLMAX + 
                                    "{\"name\": \"" + __INSTRUMENT_NAMES[i] + 
                                    "\",\"impulse\": " + String.Format("{0:0.0}", lmaxImpulse) + 
                                    ",\"timestamp\": " + DateTimeOffset.Now.ToUnixTimeSeconds() + "}";
                                
                                double alpariForexImpulse = debag_array[mode] - 2;
                                double alpariForexError = debag_array[mode] - 1;
                                double alpariForexImpulseDifference = lmaxImpulse - alpariForexImpulse - alpariForexError;

                                messageToClientBroker = messageToClientBroker + 
                                    "{\"name\": \"" + __INSTRUMENT_NAMES[i] + 
                                    "\",\"impulse\": " + String.Format("{0:0.0}", alpariForexImpulse) + 
                                    ",\"timestamp\": " + DateTimeOffset.Now.ToUnixTimeSeconds() + 
                                    ",\"error\":" + String.Format("{0:0.00}", alpariForexError) + 
                                    ",\"impulse_difference\": " + String.Format("{0:0.0}", alpariForexImpulseDifference) + "}";

                                #region --- PocketOption part ---
                                
                                var currentData = debag_array[mode] - 0.5;
                                var previousData = debag_array[mode] + 1;
                                var timestampPO = DateTimeOffset.Now.ToUnixTimeSeconds();

                                bool pocketOptionMessageEmpty = false;
                                if (currentData.ToString() != "" && previousData.ToString() != "")
                                {
                                    double pocketOptionImpulse = Convert.ToDouble(currentData) - Convert.ToDouble(previousData);
                                    double pocketOptionError = debag_array[mode] - Convert.ToDouble(currentData);
                                    double pocketOptionImpulseDifference = lmaxImpulse - pocketOptionImpulse - pocketOptionError;

                                    messageToClientPocketOption +=
                                        "{\"name\": \"" + __INSTRUMENT_NAMES[i] +
                                        "\",\"impulse\": " + String.Format("{0:0.0}", pocketOptionImpulse) +
                                        ",\"timestamp\": " + DateTimeOffset.Now.ToUnixTimeSeconds() +
                                        ",\"error\":" + String.Format("{0:0.0}", pocketOptionError) +
                                        ",\"impulse_difference\": " + String.Format("{0:0.0}", pocketOptionImpulseDifference) + "}";
                                    
                                    //DateTime today = DateTime.Today;
                                    //Directory.CreateDirectory(Path.GetDirectoryName("logs/"+today.ToString()));
                                    /*REWRITE BEFORE START!!!!!
                                    using (StreamWriter writer = new StreamWriter("logs/"+json["token"].ToString() + ".txt", true))
                                    {
                                        writer.WriteLine("Name: " + __INSTRUMENT_NAMES[i]);
                                        writer.WriteLine("LMAX (curr:prev): "+(__lmax_prices[i].ToString()+":"+__prev_lmax_prices[i].ToString()).ToString());
                                        writer.WriteLine("LMAX (impulse): "+String.Format("{0:0.00000}", (__lmax_prices[i] - __prev_lmax_prices[i])));
                                        writer.WriteLine();
                                        writer.WriteLine("Alpari (curr:prev): "+(__alpariforex_prices[i].ToString()+":"+__prev_alpariforex_prices[i].ToString()).ToString());
                                        writer.WriteLine("Alpari (impulse): "+String.Format("{0:0.00000}", (__alpariforex_prices[i] - __prev_alpariforex_prices[i])));
                                        writer.WriteLine("Alpari (impulse_difference): "+String.Format("{0:0.00000}", ((__lmax_prices[i] - __prev_lmax_prices[i]) - (__alpariforex_prices[i] - __prev_alpariforex_prices[i]) - __prices[i])));
                                        writer.WriteLine("Alpari (error): "+String.Format("{0:0.00000}", (__prices[i])));
                                        
                                        writer.WriteLine();
                                        writer.WriteLine("PocketOption (curr:prev): "+(currentData.ToString()+":"+previousData.ToString()));
                                        writer.WriteLine("PocketOption (impulse): "+String.Format("{0:0.00000}", (pocketOptionImpulse)));
                                        writer.WriteLine("PocketOption (impulse_difference): "+String.Format("{0:0.00000}", (pocketOptionImpulseDifference)));
                                        writer.WriteLine("PocketOption (error): "+String.Format("{0:0.00000}", (pocketOptionError)));
                                        writer.WriteLine("PocketOption (timestamp): "+timestampPO.ToString());
                                        
                                        writer.WriteLine();
                                        writer.WriteLine("-----------------------------------------------------------------------------------");
                                    }*/
                                }
                                else
                                {
                                    pocketOptionMessageEmpty = true;
                                }
                                
                                #endregion --- end of PocketOption part ---

                                if (i < __INSTRUMENT_ID.Length - 1)
                                {
                                    messageToClientLMAX += ",";
                                    messageToClientBroker += ",";
                                    if (!pocketOptionMessageEmpty)
                                    {
                                        messageToClientPocketOption += ",";
                                    }
                                    
                                }
                            }
                            
                            string finMessage = messageToClientLMAX + messageToClientBroker + messageToClientPocketOption + "]}";
                            
                            Send(finMessage);

                            
                            //__lmax_prices.CopyTo(__prev_lmax_prices, 0);
                            //__timestamps.CopyTo(__prev_timestamps, 0);
				            //__alpariforex_prices.CopyTo(__prev_alpariforex_prices, 0);
                        }
                        else
                        {
                            Send("{\"Error\": \"Wrong token sent or its already in use by other computer.\"}");
                        }
                    }
                    else if(false/*json.ContainsKey("closeToken") && json.ContainsKey("ip")*/)
                    {
                        /*if(__tokens[json["closeToken"].ToString()] == json["ip"].ToString())
                        {
                            __tokens.Remove(json["closeToken"].ToString());
                        }*/
                    }
                    else 
                    {
                        Send("{\"Error\": \"No token or ip value sent\"}");
                    }
                } 
                catch(Newtonsoft.Json.JsonReaderException ex)
                {
                    Send("{\"Error\": \"Wrong json-message format!\"}");
                }
                
            } 
            else 
            {
                Send("{\"Error\": \"No data sent\"}");
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine("Closed with code " + e.Code + " and reason - " + e.Reason);
        }

        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            Console.WriteLine("ERROR: " + e.Message);
        }
        
        public Server(){}
    }
}