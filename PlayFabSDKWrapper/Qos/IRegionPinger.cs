using System.Threading.Tasks;

namespace PlayFabSDKWrapper.QoS
{
    public interface IRegionPinger
    {
        Task PingAsync();
        QosRegionResult GetResult();
    }
}
