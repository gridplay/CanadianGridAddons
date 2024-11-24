using Nini.Config;
using OpenMetaverse;
using OpenSim.Services.Base;
using MySql.Data.MySqlClient;
using System;

namespace Premium
{
    public class Premium : ServiceBase, IPremium
    {
        private string table = "premium";
        private string connString = string.Empty;
        private MySqlConnection dbcon;
        public Premium(IConfigSource config) : base(config)
        {
            IConfig dbConfig = config.Configs["DatabaseService"];
            if (dbConfig is not null)
            {
                if (connString.Length == 0)
                    connString = dbConfig.GetString("ConnectionString", string.Empty);
            }
        }

        public int GetPrem(UUID agent)
        {
            int prem = 0;
            int expire = 0;
            try
            {
                dbcon = new MySqlConnection(connString);
                try
                {
                    dbcon.Open();
                }
                catch (Exception e)
                {
                    throw new Exception("[PREMIUM]: Connection error while using connection string [" + connString + "]", e);
                }
            }

            catch (Exception e)
            {
                throw new Exception("[MONEY MANAGER]: Error initialising MySql Database: " + e.ToString());
            }
            string sql = "SELECT * FROM `" + table + "` WHERE uuid = ?uuid";
            MySqlCommand cmd = new MySqlCommand(sql, dbcon);
            cmd.Parameters.AddWithValue("?uuid", agent.ToString());
            using (MySqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    try
                    {
                        prem = (int)r["prem"];
                        expire = (int)r["expire"];
                    }
                    catch (Exception e)
                    {
                        throw new Exception("[PREMIUM]: Error checking tables" + e.ToString());
                    }
                }
                r.Close();
            }
            cmd.Dispose();

            dbcon.Close();

            if (prem == 0)
                return 0;

            if (expire >= int.Parse(DateTime.Now.ToString()))
                return prem;
         

            return 0;
        }
    }
}
