using System.Collections.Generic;

namespace Nop.Plugin.BizApp.Core.Domain.Api.Auth
{
    public class TokenResponse : ApiResponse
    {
        public IList<TokenData> Token { get; set; }


        #region Nested classes

        public class TokenData
        {
            public string AccessToken { get; set; }

            public int Expire { get; set; }

            public string RefreshToken { get; set; }
        }

        #endregion
    }
}