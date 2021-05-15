using Snowflake.FileStream.Model;
using Snowflake.FileStream.StorageClients.AmazonS3;
using System;

namespace Snowflake.FileStream
{
    public static class PutFiles
    {
        public static IPutFile Create(SnowflakePutResponse response, DateTime expiryDate)
        {
            return PutFileS3.Create(response, expiryDate);
        }
    }
}