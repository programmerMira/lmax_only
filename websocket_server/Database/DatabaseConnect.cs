using MySql.Data.MySqlClient;

namespace websocket_server
{
    internal class DatabaseConnect
    {
        public string Server { get; set; }
        public string DatabaseName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public MySqlConnection Connection { get; set; }

        private static DatabaseConnect _instance = null;
        public static DatabaseConnect Instance()
        {
            if (_instance == null)
                _instance = new DatabaseConnect();
            return _instance;
        }

        public bool IsConnect()
        {
            if (Connection == null)
            {
                //if (String.IsNullOrEmpty(DatabaseName))
                //    return false;
                string connstring = string.Format("Server={0}; database={1}; UID={2}; password={3}", "localhost", "prometeo_db", "root", "root");
                Connection = new MySqlConnection(connstring);
                Connection.Open();
            }

            return true;
        }

        public void Close()
        {
            Connection.Close();
        }
    }
}
