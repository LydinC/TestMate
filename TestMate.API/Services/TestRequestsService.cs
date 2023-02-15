using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TestMate.API.Settings;
using TestMate.Common.DataTransferObjects.APIResponse;
using TestMate.Common.DataTransferObjects.TestRequests;
using TestMate.Common.Enums;
using TestMate.Common.Models.TestRequests;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Text.Json;

namespace TestMate.API.Services
{
    public class TestRequestsService
    {
        private readonly IMongoCollection<TestRequest> _testRequestsCollection;
        private readonly IMongoCollection<TestRun> _testRunsCollection; 
        private readonly IMapper _mapper;

        public TestRequestsService(IOptions<DatabaseSettings> databaseSettings, IMapper mapper)
        {
            var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
            _testRequestsCollection = mongoDatabase.GetCollection<TestRequest>(databaseSettings.Value.TestRequestsCollectionName);
            _testRunsCollection = mongoDatabase.GetCollection<TestRun>(databaseSettings.Value.TestRunsCollectionName);
            _mapper = mapper;
        }

        public async Task<APIResponse<IEnumerable<TestRequest>>> GetTestRequests()
        {
            try
            {
                var testRequests = await _testRequestsCollection.Find(_ => true).ToListAsync();
                return new APIResponse<IEnumerable<TestRequest>>(testRequests);
            }
            catch (Exception ex)
            {
                return new APIResponse<IEnumerable<TestRequest>>(Status.Error, ex.Message);
            }
        }

        
        public async Task<APIResponse<TestRequest>> GetById(string id) {
            try
            {
                var testRequest = await _testRequestsCollection.Find(x => x.Id == id).FirstAsync();
                return new APIResponse<TestRequest>(testRequest);
            }
            catch (Exception ex)
            {
                return new APIResponse<TestRequest>(Status.Error, ex.Message);
            }
            
        }

        public async Task<APIResponse<TestRequestCreateResultDTO>> CreateAsync(string requestor, TestRequestCreateDTO testRequestCreateDTO)
        {

            try
            {
                //Validate TestRequestID is not duplicate
                if (_testRequestsCollection.Find(x => x.RequestId == testRequestCreateDTO.RequestId).FirstOrDefault() != null) 
                {
                    return new APIResponse<TestRequestCreateResultDTO>(Status.Error, $"TestRequestID ({testRequestCreateDTO.RequestId}) already exists!");
                }

                TestRequest testRequest = _mapper.Map<TestRequest>(testRequestCreateDTO);
                testRequest.Requestor = requestor;

                await _testRequestsCollection.InsertOneAsync(testRequest);

                //Produce neccessary test run entities
                List<TestRun> testRuns = GenerateTestRunEntities(testRequest);
                await _testRunsCollection.InsertManyAsync(testRuns);

                return new APIResponse<TestRequestCreateResultDTO>(new TestRequestCreateResultDTO { Id = testRequest.Id });
            }
            catch (Exception ex)
            {
                return new APIResponse<TestRequestCreateResultDTO>(Status.Error, ex.Message);
            }
        }

        public List<TestRun> GenerateTestRunEntities(TestRequest testRequest) {

            DesiredDeviceProperties deviceProperties = testRequest.TestRequestConfiguration.DesiredDeviceProperties;

            List<TestRun> testRuns = new List<TestRun>();
            List<Dictionary<string, object>> deviceFilters = ExtractDeviceCombinations(deviceProperties);
            
            foreach(var deviceFilter in deviceFilters)
            {
                testRuns.Add(new TestRun(testRequest.RequestId, deviceFilter, testRequest.TestRequestConfiguration.ApplicationUnderTest, testRequest.TestRequestConfiguration.TestSolutionPath));
            }

            return testRuns;
        }


        public List<Dictionary<string, object>> ExtractDeviceCombinations(DesiredDeviceProperties properties)
        {
            var jsonString = JsonSerializer.Serialize(properties);
            var jsonObject = JsonSerializer.Deserialize<JsonElement>(jsonString);

            var combinations = new List<Dictionary<string, object>>();
            AddCombinations(jsonObject, new Dictionary<string, object>(), combinations);

            return combinations;
        }

        private void AddCombinations(JsonElement jsonElement, Dictionary<string, object> currentCombination, List<Dictionary<string, object>> combinations)
        {
            if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var property in jsonElement.EnumerateObject())
                {
                    var propertyName = property.Name;
                    var propertyValue = property.Value;

                    if (propertyValue.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var element in propertyValue.EnumerateArray())
                        {
                            var newCombination = new Dictionary<string, object>(currentCombination);
                            newCombination[propertyName] = element.GetString();
                            AddCombinations(element, newCombination, combinations);
                        }
                    }
                    else if (propertyValue.ValueKind == JsonValueKind.String)
                    {
                        currentCombination[propertyName] = propertyValue.GetString();
                    }
                }

                if (currentCombination.Count > 0)
                {
                    combinations.Add(currentCombination);
                }
            }
        }


    }
}
