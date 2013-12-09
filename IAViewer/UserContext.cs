using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IAViewer
{
    public class UserContext
    {
        String _userName;
        String _userGUID;
        char[] _password;

        

        public UserContext()
        {

        }

        public static String GetPasswordString(char[] password)
        {
            return new String(password);
        }
    }
}
