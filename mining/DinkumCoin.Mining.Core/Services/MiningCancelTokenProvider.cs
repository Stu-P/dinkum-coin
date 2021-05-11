using System;
using System.Threading;

namespace DinkumCoin.Mining.Core.Services
{
    public class MiningCancelTokenProvider : ICancelTokenProvider
    {
        private CancellationTokenSource _cts;

        public MiningCancelTokenProvider()
        {
            _cts = new CancellationTokenSource();
        }
        public void CancelExisting()
        {
            _cts.Cancel();
        }

        public CancellationToken New()
        {
            return _cts.Token;
        }
    }
}
