using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class TokenResponse
    {
        private string access_token;
        private string token_type;
        private string refresh_token;
        private int expires_in;
        private string scope;

        public TokenResponse() { }

        public string Access_token
        {
            get
            {
                return access_token;
            }

            set
            {
                access_token = value;
            }
        }

        public string Token_type
        {
            get
            {
                return token_type;
            }

            set
            {
                token_type = value;
            }
        }

        public string Refresh_token
        {
            get
            {
                return refresh_token;
            }

            set
            {
                refresh_token = value;
            }
        }

        public int Expires_in
        {
            get
            {
                return expires_in;
            }

            set
            {
                expires_in = value;
            }
        }

        public string Scope
        {
            get
            {
                return scope;
            }

            set
            {
                scope = value;
            }
        }

        public struct ErrorResponse
        {
            private string error;
            private string error_description;

            public string Error
            {
                get
                {
                    return error;
                }

                set
                {
                    error = value;
                }
            }

            public string Error_description
            {
                get
                {
                    return error_description;
                }

                set
                {
                    error_description = value;
                }
            }
        }
    }
}
