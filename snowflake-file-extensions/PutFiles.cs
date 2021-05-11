using System;
using System.Collections.Generic;
using System.Linq;
using Snowflake.FileStream.Model;

namespace Snowflake.FileStream
{
    public static class PutFiles
    {
        public static IEnumerable<string> ExpandFileList(IList<string> files)
        {
            var result = Glob.ExpandFileNames(files[0]);
            for (var i = 1; i < files.Count; i++)
                result = result.Union(Glob.ExpandFileNames(files[i]));
            return result;
        }

        public static IEnumerable<IFileTask> BuildPutFileTasks(
            SnowflakePutResponse response,
            IEnumerable<PutFileItem> files = null,
            Action<PutFileEvent> callback = null
        )
        {
            return PutFileHeaderS3.Create(
                response,
                callback,
                files: files??PutFiles.ExpandFileList(response.SourceLocations).Select(e=>new PutFileItem(e))
            ).ToArray();
        }
    }
}