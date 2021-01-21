using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APTSWebApp.API
{
    public class OicConnectionStringParser
    {
        private readonly Dictionary<string, string> _fields;

        public string Server => _fields.ContainsKey("Server") ? _fields["Server"] : null;

        public string Database => _fields.ContainsKey("Database") ? _fields["Database"] : null;

        public string UserId => _fields.ContainsKey("User Id") ? _fields["User Id"] : null;

        public string Password => _fields.ContainsKey("Password") ? _fields["Password"] : null;

        public string IntegratedSecurity => _fields.ContainsKey("Integrated Security") ? _fields["Integrated Security"] : null;

        public OicConnectionStringParser(string connectionString)
        {
            _fields = connectionString
                .Split(';')
                .ToDictionary(s => s.Split('=')[0], 
                    s => s.Split('=')[1]);
        }
    }
}
