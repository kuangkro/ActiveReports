using JSViewer_MVC_Core.model;

namespace JSViewer_MVC_Core.utils
{
    public class RemoteSourceMock
    {

        public static ProccessResult<byte[]> getReportFile(string fplb)
        {
            return ProccessResult.OnOk<byte[]>(null);
        }
    }



    public class LoginInfo
    {
        public const string gs_dwmc = "mock-gs_dwmc";
        public const string bmbm = "mock-bmbm";

        public const string XPSM_CUD_ZTH = "mock-XPSM_CUD_ZTH";

        public const string Rybm = "mock-Rybm";


    }
}
