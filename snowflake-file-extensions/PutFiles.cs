using Snowflake.FileStream.Model;
using Snowflake.FileStream.StorageClients.AmazonS3;

namespace Snowflake.FileStream
{
    public static class PutFiles
    {
        public static IPutFile Create(SnowflakePutResponse response)
        {
            return PutFileS3.Create(response);
        }
    }
}