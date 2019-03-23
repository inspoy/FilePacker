using System.IO;
using System.Text;

namespace Instech.FilePacker
{
    internal class PackTool
    {
        private stirng _ipkPath;
        public PackTool(string ipkPath)
        {
            
        }

        public void Create()
        {
            File.Create(_ipkPath);
        }
    }
}