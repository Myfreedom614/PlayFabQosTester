using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.QoS;

namespace PlayFabQosTester
{
    /// <summary>
    ///   Simple executable that integrates with PlayFab's SDK.
    ///   It allocates a game server and makes an http request to that game server
    /// </summary>
    public class Program
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        
        public static Task Main(string[] args)
        {
            RootCommand rootCommand = RootCommandConfiguration.GenerateCommand(Run);

            return rootCommand.InvokeAsync(args);
        }

        private static async Task Run(string titleId, string playerId, bool chinaVer, bool listQosForTitle, bool verbose)
        {
            PlayFabApiSettings settings = new PlayFabApiSettings() {TitleId = titleId};
            PlayFabClientInstanceAPI clientApi = new PlayFabClientInstanceAPI(settings);

            // Login
            var loginRequest = new LoginWithCustomIDRequest()
            {
                CustomId = playerId,
                CreateAccount = true
            };
            PlayFabResult<LoginResult> login = await clientApi.LoginWithCustomIDAsync(loginRequest);
            if (login.Error != null)
            {
                Console.WriteLine(login.Error.ErrorMessage);
                throw new Exception($"Login failed with HttpStatus={login.Error.HttpStatus}");
            }
            Console.WriteLine($"Logged in player {login.Result.PlayFabId} (CustomId={playerId})");
            Console.WriteLine();

            // Measure QoS
            Stopwatch sw = Stopwatch.StartNew();
            PlayFabSDKWrapper.QoS.PlayFabQosApi qosApi = new PlayFabSDKWrapper.QoS.PlayFabQosApi(settings, clientApi.authenticationContext);
            PlayFabSDKWrapper.QoS.QosResult qosResult = await qosApi.GetQosResultAsync(250, degreeOfParallelism:4, pingsPerRegion:10, listQosForTitle: listQosForTitle, chinaVer: chinaVer);
            if (qosResult.ErrorCode != 0)
            {
                Console.WriteLine(qosResult.ErrorMessage);
                throw new Exception($"QoS ping failed with ErrorCode={qosResult.ErrorCode}");
            }
            
            Console.WriteLine($"Pinged QoS servers in {sw.ElapsedMilliseconds}ms with results:");

            if (verbose)
            {
                string resultsStr = JsonConvert.SerializeObject(qosResult.RegionResults, Formatting.Indented);
                Console.WriteLine(resultsStr);
            }

            int timeouts = qosResult.RegionResults.Sum(x => x.NumTimeouts);
            Console.WriteLine(string.Join(Environment.NewLine,
                qosResult.RegionResults.Select(x => $"{x.Region} - {x.LatencyMs}ms")));

            Console.WriteLine($"NumTimeouts={timeouts}");
            Console.WriteLine();

            Console.ReadKey();
        }
    }
}