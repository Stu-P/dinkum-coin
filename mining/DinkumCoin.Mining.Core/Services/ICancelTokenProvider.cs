using System;
using System.Threading;

namespace DinkumCoin.Mining.Core.Services
{
    public interface ICancelTokenProvider
    {
        CancellationToken New();

        void CancelExisting();

    }
}
