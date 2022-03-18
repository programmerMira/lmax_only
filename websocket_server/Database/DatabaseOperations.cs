using System;
using MySql.Data.MySqlClient;

namespace websocket_server
{
    internal class DatabaseOperations
    {
        private MySqlConnection __connection;

        public DatabaseOperations(MySqlConnection Connection) 
        {
            __connection = Connection;
        }
        public bool FindByKey(string key, bool connection)
        {
            string query = String.Format("UPDATE user_to_tokens SET is_connected={0} WHERE access_token = '{1}' and is_connected={2} and deleted_at is null", false, key, Convert.ToInt16(!connection));

            var cmd = new MySqlCommand(query, __connection);
            var result = cmd.ExecuteNonQuery();

            if(result > 0)
                return true;
            return false;
        }
    }
}
