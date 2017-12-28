﻿using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Responses.PendingEvents;

namespace Lykke.Service.BlockchainApi.Client.Results
{
    [PublicAPI]
    public class PendingCashoutFailedEvent : BasePendingEvent
    {
        public string ToAddress { get; set; }

        public PendingCashoutFailedEvent(PendingCashoutFailedEventContract apiResponse, int assetAccuracy) : 
            base(apiResponse, assetAccuracy)
        {
            ToAddress = apiResponse.ToAddress;
        }
    }
}