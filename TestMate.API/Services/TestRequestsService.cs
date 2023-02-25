﻿using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using TestMate.API.Settings;
using TestMate.Common.DataTransferObjects.APIResponse;
using TestMate.Common.DataTransferObjects.TestRequests;
using TestMate.Common.Enums;
using TestMate.Common.Models.TestRequests;
using TestMate.Common.Models.TestRuns;

namespace TestMate.API.Services
{
    public class TestRequestsService
    {
        private readonly IMongoCollection<TestRequest> _testRequestsCollection;
        private readonly IMongoCollection<TestRun> _testRunsCollection;
        private readonly IMapper _mapper;
        private readonly ILogger<TestRequestsService> _logger;

        public TestRequestsService(IOptions<DatabaseSettings> databaseSettings, IMapper mapper, ILogger<TestRequestsService> logger)
        {
            var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
            _testRequestsCollection = mongoDatabase.GetCollection<TestRequest>(databaseSettings.Value.TestRequestsCollectionName);
            _testRunsCollection = mongoDatabase.GetCollection<TestRun>(databaseSettings.Value.TestRunsCollectionName);
            _mapper = mapper;
            _logger = logger;
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

        public async Task<APIResponse<TestRequest>> GetDetails(Guid id)
        {
            try
            {
                var testRequest = await _testRequestsCollection.Find(x => x.RequestId == id).FirstOrDefaultAsync();
                if (testRequest == null) {
                    return new APIResponse<TestRequest>(Status.Error, $"Test Request with RequestId {id} does not exist");
                }
                return new APIResponse<TestRequest>(testRequest);
            }
            catch (Exception ex)
            {
                return new APIResponse<TestRequest>(Status.Error, ex.Message);
            }
        }


        public async Task<APIResponse<TestRequestStatus>> GetStatus(Guid id)
        {
            try
            {
                var testRequest = await _testRequestsCollection.Find(x => x.RequestId == id).FirstOrDefaultAsync();
                if (testRequest == null)
                {
                    return new APIResponse<TestRequestStatus>(Status.Error, $"Could not get Status of Test Request with RequestId {id} as it does not exist");
                }
                return new APIResponse<TestRequestStatus>(testRequest.Status);
            }
            catch (Exception ex)
            {
                return new APIResponse<TestRequestStatus>(Status.Error, ex.Message);
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
                
                //Produce neccessary test run entities
                List<TestRun> testRuns = GenerateTestRunEntities(testRequest);
                _logger.LogInformation($"TestRequest {testRequest.RequestId} resolved in {testRuns.Count} Test Runs");

                if (testRuns.Count > 0)
                {
                    await _testRequestsCollection.InsertOneAsync(testRequest);
                    await _testRunsCollection.InsertManyAsync(testRuns);
                }
                else
                {
                    return new APIResponse<TestRequestCreateResultDTO>(Status.Error, "Failed to add Test Request as test run count is 0.");
                }

                return new APIResponse<TestRequestCreateResultDTO>(new TestRequestCreateResultDTO { Id = testRequest.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating test request.");
                return new APIResponse<TestRequestCreateResultDTO>(Status.Error, ex.Message);
            }
        }


        private Dictionary<string, List<object>>? DeserializeDesiredDeviceProperties(DesiredDeviceProperties desiredDeviceProperties)
        {
            if (desiredDeviceProperties == null)
            {
                return null;
            }

            var options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            var jsonString = JsonSerializer.Serialize(desiredDeviceProperties, options);
            return JsonSerializer.Deserialize<Dictionary<string, List<object>>>(jsonString);
        }

        public List<TestRun> GenerateTestRunEntities(TestRequest testRequest)
        {
            List<TestRun> testRuns = new List<TestRun>();

            var desiredDeviceProperties = DeserializeDesiredDeviceProperties(testRequest.Configuration.DesiredDeviceProperties);

            if (desiredDeviceProperties != null)
            {
                List<Dictionary<string, string>> deviceFilters = GetDeviceFilterPermutations(desiredDeviceProperties);
                _logger.LogDebug($"List of device filters: {deviceFilters.ToString()}");

                List<Dictionary<string, string>> configurations = new List<Dictionary<string, string>>();
                if (testRequest.Configuration.DesiredContextConfiguration != null)
                {
                    configurations = GetContextConfigurationPermutations(testRequest.Configuration.DesiredContextConfiguration);
                }

                foreach (Dictionary<string, string> deviceFilter in deviceFilters)
                {
                    //if no configuration is available, passing config = null into the single test run for the current device filter
                    var configurationsToUse = configurations != null && configurations.Count > 0
                        ? configurations
                        : new List<Dictionary<string, string>> { null };
                    foreach (var config in configurationsToUse)
                    {
                        var testRun = new TestRun(
                            testRequestID: testRequest.RequestId,
                            deviceFilter: deviceFilter,
                            apkPath: testRequest.Configuration.ApkPath,
                            testExecutablePath: testRequest.Configuration.TestExecutablePath,
                            contextConfiguration: config
                        );
                        testRuns.Add(testRun);
                    }
                }
            }

            return testRuns;
        }

        public static List<Dictionary<string, string>> GetDeviceFilterPermutations(Dictionary<string, List<object>> input)
        {
            var permutations = new List<Dictionary<string, string>>();
            if (input == null || input.Count == 0)
            {
                return permutations;
            }
            var keys = input.Keys.ToArray();
            var values = input.Values.ToArray();

            var result = new Dictionary<string, string>();
            AddNextPermutation(keys, values, 0, result, permutations);
            return permutations;
        }

        private static void AddNextPermutation(string[] keys, List<object>[] values, int currentIndex,
                                                Dictionary<string, string> currentPermutation, List<Dictionary<string, string>> permutations)
        {
            // base case: when currentIndex is equal to the number of keys
            if (currentIndex == keys.Length)
            {
                var newPermutation = new Dictionary<string, string>(currentPermutation);
                permutations.Add(newPermutation);
                return;
            }

            // recursive case: iterate through all values for the current key and add to the permutation
            foreach (var value in values[currentIndex])
            {
                //casting value into a string so that deviceFilter can handle all types of desiredDeviceElements
                currentPermutation[keys[currentIndex]] = value.ToString();

                // add the current key-value pair to the permutation, and move on to the next key
                AddNextPermutation(keys, values, currentIndex + 1, currentPermutation, permutations);
            }
        }


        public static List<Dictionary<string, string>> GetContextConfigurationPermutations(List<DesiredContextConfiguration> desiredContextConfiguration) 
        {

            JsonSerializerOptions options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            var desiredContextConfigurationJsonString = JsonSerializer.Serialize(desiredContextConfiguration, options);
            var contextConfigurations = JsonSerializer.Deserialize<List<Dictionary<string, List<object>>>>(desiredContextConfigurationJsonString);

            var allPermutations = new List<Dictionary<string, string>>();
            if (contextConfigurations.Count > 0)
            {
                foreach (var configuration in contextConfigurations)
                {
                    var permutations = GenerateCartesianProduct(configuration);
                    allPermutations.AddRange(permutations);
                }
            }

            return allPermutations;
        }

        public static IEnumerable<Dictionary<string, string>> GenerateCartesianProduct(Dictionary<string, List<object>> input)
        {
            string[] keys = input.Keys.ToArray();
            var values = input.Values.Select(list => list.ToArray()).ToArray();

            // Initialise an array of indices to keep track of the current index of each list of values.
            int[] indices = new int[keys.Length];

            for (var currentIndex = keys.Length - 1; currentIndex >= 0; currentIndex--)
            {
                while (indices[currentIndex] < values[currentIndex].Length)
                {
                    var combination = new Dictionary<string, string>();

                    // Loop through the keys and add the corresponding value at the current index to the dictionary.
                    for (var keyIndex = 0; keyIndex < keys.Length; keyIndex++)
                    {
                        combination[keys[keyIndex]] = values[keyIndex][indices[keyIndex]].ToString();
                    }
                    yield return combination;

                    indices[currentIndex]++;
                }
                indices[currentIndex] = 0;
            }
        }
    }
}

