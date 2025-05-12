using System.Text;
using Moonlight.ApiServer.Interfaces;
using Moonlight.ApiServer.Models.Diagnose;

namespace Moonlight.ApiServer.Implementations.Diagnose;

public class CoreDiagnoseProvider : IDiagnoseProvider
{
    public DiagnoseEntry[] GetFiles()
    {
        return
        [
            new DiagnoseFile()
            {
                Name = "test.txt",
                GetContent = () => Encoding.UTF8.GetBytes("hello world")
            }
        ];
    }
}