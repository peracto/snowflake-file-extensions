using Snowflake.FileStream.Model;

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