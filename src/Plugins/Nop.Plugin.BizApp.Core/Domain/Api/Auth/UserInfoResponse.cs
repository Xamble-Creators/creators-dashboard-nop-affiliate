using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Nop.Plugin.BizApp.Core.Domain.Api.Auth
{
    public class UserInfoResponse : ApiResponse
    {
        public IList<UserData> UserInfo { get; set; }

        #region Nested classes

        public class UserData
        {
            public string HqPId { get; set; }

            public string Username { get; set; }

            public string FullName { get; set; }

            [JsonProperty(propertyName:"hpno")]
            public string PhoneNumber { get; set; }

            public string Email { get; set; }

            public string ProfilePhoto { get; set; }

            [JsonProperty(propertyName: "package_expire")]
            public DateTime PackageExpire { get; set; }

            [JsonProperty(propertyName: "max_sp")]
            public string MaxSp { get; set; }

            [JsonProperty(propertyName: "bizappay_secretkey")]
            public string BizAppPaySecretKey { get; set; }

            [JsonProperty(propertyName: "bizappay_categorycode")]
            public string BizAppPayCategoryCode { get; set; }

            [JsonProperty(propertyName: "bizappay_email")]
            public string BizAppPayEmail { get; set; }

        }

        #endregion
    }
}