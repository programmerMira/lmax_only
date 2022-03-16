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
        private static Thread lmax_Thread;

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
        private static double[] __lmax_prices = new double[__INSTRUMENT_ID.Length];
        private static double[] __prev_lmax_prices = new double[__INSTRUMENT_ID.Length];
        private static long[] __timestamps = new long[__INSTRUMENT_ID.Length];
        #endregion
        
        #region private functions (library)
        private void __MarketDataUpdate(OrderBookEvent orderBookEvent)
        {
            __lmax_prices.CopyTo(__prev_lmax_prices, 0);

            for(int i = 0; i<__INSTRUMENT_ID.Length;i++)
            {
                if(orderBookEvent.InstrumentId==__INSTRUMENT_ID[i]){
                    __lmax_prices[i] = ((Decimal.ToDouble(orderBookEvent.ValuationBidPrice)+Decimal.ToDouble(orderBookEvent.ValuationAskPrice))/2);
                    __timestamps[i] = orderBookEvent.Timestamp;

                    using (StreamWriter writer = new StreamWriter("lMAX_output_data.txt", true)) //// true to append data to the file
                    {
                        writer.WriteLine("Name:" + __INSTRUMENT_NAMES[i]);
                        writer.WriteLine("Price:" + __lmax_prices[i].ToString());
                        writer.WriteLine("Timestamp:" + __timestamps[i].ToString());
                        writer.WriteLine();
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

            lmax_Thread = new Thread(new ThreadStart(StartLmax));
            lmax_Thread.Start(); // запускаем поток lmax_Thread
            Console.WriteLine("lMAX thread started...");
        }
        
        public static void StartLmax()
        {
            Server server = new Server();
            LmaxApi lmaxApi = new LmaxApi("https://api.lmaxtrader.com");
            lmaxApi.Login(new LoginRequest("Vadim112358", "Vadim112358"), server.__LoginCallback, __FailureCallback("log in"));
        }

        #endregion

        protected override void OnMessage(MessageEventArgs e)
        {
            try
            {
                string messageToClientLMAX = "{\"lmax\":[";

                for (int i = 0; i < __lmax_prices.Length; i++)
                {
                    messageToClientLMAX = messageToClientLMAX + 
                                        "{\"name\": \"" + __INSTRUMENT_NAMES[i] + 
                                        "\",\"impulse\": " + String.Format("{0:0.00000}", Math.Abs(__lmax_prices[i] - __prev_lmax_prices[i])) + 
                                        ",\"timestamp\": " + __timestamps[i] + "}";
                    
                    if (i < __lmax_prices.Length - 1)
                    {
                        messageToClientLMAX += ",";                                
                    }
                }
                string finMessage = messageToClientLMAX + "]}";
                Send(finMessage);

            } catch(Exception ex)
            {
                Send("{\"Error\": \"error occured\"}");
            }
        }
        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine("Closed with code "+e.Code+" and reason - "+e.Reason);
        }
        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            Console.WriteLine("ERROR: "+e.Message);
        }
        public Server(){}
    }
}
