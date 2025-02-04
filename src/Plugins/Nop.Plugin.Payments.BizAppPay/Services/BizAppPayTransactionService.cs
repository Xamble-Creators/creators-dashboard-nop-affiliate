using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Data;
using Nop.Plugin.Payments.BizAppPay.Domain;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.BizAppPay.Services
{
    public class BizAppPayTransactionService 
    {
        private readonly IRepository<BizAppPayTransaction> _bizAppPayTransactionRepository;

        public BizAppPayTransactionService(IRepository<BizAppPayTransaction> bizAppPayTransactionRepository)
        {
            this._bizAppPayTransactionRepository = bizAppPayTransactionRepository;
        }

        public virtual async Task<BizAppPayTransaction> GetBizAppPayOrderByCustomOrderNumberAsync(string customOrderNumber)
        {
            if (string.IsNullOrEmpty(customOrderNumber))
                return null;

            return await _bizAppPayTransactionRepository.Table
                .FirstOrDefaultAsync(o => o.CustomOrderNumber == customOrderNumber);
        }

        public virtual async Task InsertBizAppPayTransactionAsync(BizAppPayTransaction bizAppPayTransaction)
        {
            if (bizAppPayTransaction == null)
                throw new ArgumentNullException("bizAppPayTransaction");

            await _bizAppPayTransactionRepository.InsertAsync(bizAppPayTransaction);

        }
    }
}