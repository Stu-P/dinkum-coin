using System;
using System.Globalization;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Util;

namespace DinkumCoin.Blockchain.Data.Documents
{
    public class UtcDateTimeConverter : IPropertyConverter
    {
        public DynamoDBEntry ToEntry(object value)
        {
            return ((DateTime)value).ToString(AWSSDKUtils.ISO8601DateFormat, CultureInfo.InvariantCulture);
        }

        public object FromEntry(DynamoDBEntry entry)
        {
            return DateTimeOffset.Parse(entry, CultureInfo.InvariantCulture).UtcDateTime;
        }
    }
}
