using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DinkumCoin.Mining.Core.Models;
using Newtonsoft.Json;

namespace DinkumCoin.Mining.Core.Clients
{
    public class BlockchainApiClient : IBlockchainApiClient
    {
        private readonly HttpClient _apiClient;

        public BlockchainApiClient(HttpClient httpClient)
        {
            _apiClient = httpClient;
        }
        public async Task<BlockProof> GetLastBlock()
        {
            var response = await _apiClient.GetAsync("api/blocks/last");

            response.EnsureSuccessStatusCode();

            return JsonConvert.DeserializeObject<BlockProof>(await response.Content.ReadAsStringAsync());
        }

        public async Task SubmitSolution(SolutionAttempt attempt)
        {
            var requestBody = new StringContent(JsonConvert.SerializeObject(attempt), Encoding.UTF8, "application/json");
            var response = await _apiClient.PostAsync("api/blocks/add", requestBody);
            response.EnsureSuccessStatusCode();
        }
    }
}
