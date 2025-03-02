﻿using System;
using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.BizAppPay
{
    public class BizAppPayPaymentSettings : ISettings
    {
        public string Endpoint { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
        /// <summary>
        /// Additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }
    }
}