#region usings
using System;
using WebSocketSharp;
using WebSocketSharp.Server;
#endregion

namespace websocket_server
{
    public class Details : WebSocketBehavior
    {
        private string[] brokers = {"AlpariForex","PocketOption"};
        
        //GENERALS
        /*private string[] quatations = {"EUR/USD", "GBP/USD", "USD/CHF", "USD/JPY", "USD/CAD", "AUD/USD","NZD/USD",
        "EUR/JPY","EUR/GBP","GBP/JPY","EUR/CAD","EUR/AUD","AUD/JPY","GBP/AUD","GBP/CAD","GBP/CHF","NZD/JPY","XAU/USD",
        "XAG/USD","XRP/USD", "AUD/CAD", "AUD/CHF", "CAD/JPY", "USD/RUB", "EUR/RUB", "EUR/CHF", "CHF/JPY"};*/
        
        private string[] quatations_alpari = 
            {
                "EUR/USD", "GBP/USD", "USD/CHF", "USD/JPY", "USD/CAD", 
                "AUD/USD", "NZD/USD", "EUR/JPY", "EUR/GBP", "GBP/JPY",
                "EUR/CAD", "EUR/AUD", "AUD/JPY", "GBP/AUD", "GBP/CAD",
                "GBP/CHF", "NZD/JPY", "XAU/USD", "XAG/USD", "XRP/USD"
            };
        
        //****************************************CHECK IT*************************
        private string[] quatations_pocketoption = {
            "AUD/CAD", "AUD/CHF", "AUD/JPY", "AUD/USD",
            "CAD/JPY",
            "CHF/JPY",
            "EUR/AUD", "EUR/CAD", "EUR/CHF", "EUR/GBP", "EUR/JPY", "EUR/RUB", "EUR/USD",
            "GBP/AUD", "GBP/CAD", "GBP/CHF", "GBP/JPY", "GBP/USD",
            "NZD/JPY", "NZD/USD",
            "USD/CAD", "USD/CHF", "USD/JPY", "USD/RUB"
        };
        
        protected override void OnMessage(MessageEventArgs e)
        {
            /*{"brokers":[
                         {"name":"AlpariForex", "quatations":[...]},
                         {"name":"PocketOption","quatations":[...]}
                        ]
            }*/
            
            string broker_part = "{\"brokers\":[";

            broker_part+="{\"name\":\""+brokers[0]+"\",\"quatations\":[";
            for(int j = 0; j<quatations_alpari.Length;j++)
            {
                if(j<1)
                    broker_part += "\""+quatations_alpari[j]+"\"";
                else
                    broker_part += ",\""+quatations_alpari[j]+"\"";
            }

            broker_part+="]},{\"name\":\""+brokers[1]+"\",\"quatations\":[";
            for(int j = 0; j<quatations_pocketoption.Length;j++)
            {
                if(j<1)
                    broker_part += "\""+quatations_pocketoption[j]+"\"";
                else
                    broker_part += ",\""+quatations_pocketoption[j]+"\"";
            }

            Send(broker_part+"]}]}");
        }
        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine("Closed with code"+e.Code+" and reason - "+e.Reason);
        }
        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            Console.WriteLine("ERROR:"+e.Message);
        }
    }
}