using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APTSWebApp.API
{
    public class OicConnectionStringParser
    {
        private Dictionary<string, string> fields;

        public string Server
        {
            get
            {
                if (fields.ContainsKey("Server"))
                {
                    return fields["Server"];
                }
                else
                {
                    return null;
                }
            }
        }
        public string Database
        {
            get
            {
                if (fields.ContainsKey("Database"))
                {
                    return fields["Database"];
                }
                else
                {
                    return null;
                }
            }
        }
        public string UserID
        {
            get
            {
                if (fields.ContainsKey("User Id"))
                {
                    return fields["User Id"];
                }
                else
                {
                    return null;
                }
            }
        }
        public string Password
        {
            get
            {
                if (fields.ContainsKey("Password"))
                {
                    return fields["Password"];
                }
                else
                {
                    return null;
                }
            }
        }
        public string IntegratedSecurity
        {
            get
            {
                if (fields.ContainsKey("Integrated Security"))
                {
                    return fields["Integrated Security"];
                }
                else
                {
                    return null;
                }
            }
        }

        public OicConnectionStringParser(string connectionString)
        {
            try
            {
                fields = connectionString.Split(';').ToDictionary(s => s.Split('=')[0], s => s.Split('=')[1]);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }
}
