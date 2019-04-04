using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.Common
{
    public class FileStreamed
    {
        public string FileName { get; set; }
        public Stream FileStream { get; set; }


        public FileStreamed()
        {

        }

        public  FileStreamed(string filename, Stream filestream)
        {
            this.FileName = filename;
            this.FileStream = filestream;

        }
    }
}
