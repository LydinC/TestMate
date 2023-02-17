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
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Reflection;
using TestMate.Common.Models.Devices;

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

        public async Task<APIResponse<TestRequest>> GetById(string id)
        {
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
                return new APIResponse<TestRequestCreateResultDTO>(Status.Error, ex.Message);
            }
        }

        public List<TestRun> GenerateTestRunEntities(TestRequest testRequest)
        {
            List<TestRun> testRuns = new List<TestRun>();

            DesiredDeviceProperties deviceProperties = testRequest.Configuration.DesiredDeviceProperties;

            //creating json options to ignore any null values in the desired device properties object
            JsonSerializerOptions options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            var jsonString = JsonSerializer.Serialize(testRequest.Configuration.DesiredDeviceProperties, options);
            var input = JsonSerializer.Deserialize<Dictionary<string, List<object>>>(jsonString);

            if (input != null)
            {
                List<Dictionary<string, object>> deviceFilters = GetDeviceFilterPermutations(input);
                _logger.LogDebug("List of device filters :" + deviceFilters.ToString());

                foreach (Dictionary<string, object> deviceFilter in deviceFilters)
                {
                   
                    testRuns.Add(new TestRun(testRequest.RequestId, deviceFilter, testRequest.Configuration.ApplicationUnderTest, testRequest.Configuration.TestSolutionPath));
                }
            }

            return testRuns;
        }

        public static List<Dictionary<string, object>> GetDeviceFilterPermutations(Dictionary<string, List<object>> input)
        {
            var permutations = new List<Dictionary<string, object>>();
            if (input == null || input.Count == 0)
            {
                return permutations;
            }
            var keys = input.Keys.ToArray();
            var values = input.Values.ToArray();

            var result = new Dictionary<string, object>();
            AddNextPermutation(keys, values, 0, result, permutations);
            return permutations;
        }

        private static void AddNextPermutation(string[] keys, List<object>[] values, int currentIndex,
                                                Dictionary<string, object> currentPermutation, List<Dictionary<string, object>> permutations)
        {
            // base case: when currentIndex is equal to the number of keys
            if (currentIndex == keys.Length)
            {
                var newPermutation = new Dictionary<string, object>(currentPermutation);
                permutations.Add(newPermutation);
                return;
            }

            // recursive case: iterate through all values for the current key and add to the permutation
            foreach (var value in values[currentIndex])
            {
                currentPermutation[keys[currentIndex]] = value;

                // add the current key-value pair to the permutation, and move on to the next key
                AddNextPermutation(keys, values, currentIndex + 1, currentPermutation, permutations);
            }
        }
    }
}


//    public static List<Dictionary<string, object>> GetDeviceFilterPermutations(Dictionary<string, List<object>> input)
//    {
//        // Create an empty list to store the permutations
//        List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

//        // If there are no more keys in the input dictionary, return a dictionary with an empty set of values
//        if (input.Count == 0)
//        {
//            result.Add(new Dictionary<string, object>());
//            return result;
//        }

//        // Get the first key in the input dictionary and its corresponding list of values
//        var firstKey = input.Keys.First();
//        var firstValues = input[firstKey];

//        // For each value in the list of values for the first key
//        foreach (var value in firstValues)
//        {
//            // Create a new dictionary to store the remaining input data
//            var remainingInput = new Dictionary<string, List<object>>(input);
//            // Remove the first key-value pair from the dictionary
//            remainingInput.Remove(firstKey);

//            // Recursively call the GetDeviceFilterPermutations method with the remaining input dictionary
//            foreach (var permutation in GetDeviceFilterPermutations(remainingInput))
//            {
//                // Add the first key-value pair to each permutation
//                permutation.Add(firstKey, value);
//                // Add the permutation to the list of results
//                result.Add(permutation);
//            }
//        }

//        // Return the list of permutations
//        return result;
//    }

//}

/*

//SECOND TRY
static List<Dictionary<string, object>> AllCombinations(object obj)
{
    var combos = new List<Dictionary<string, object>>();
    var properties = obj.GetType().GetProperties();

    foreach (var prop in properties)
    {
        if (prop.GetValue(obj) == null) {
            continue;
        }
        if (prop.PropertyType.GetInterface("IEnumerable") != null)
        {
            var values = prop.GetValue(obj) as IEnumerable<object>;
            if (!values.Any()) continue;

            var newCombos = new List<Dictionary<string, object>>();
            foreach (var value in values)
            {
                foreach (var combo in combos)
                {
                    var newCombo = new Dictionary<string, object>(combo);
                    newCombo.Add(prop.Name, value);
                    newCombos.Add(newCombo);
                }
            }
            combos = newCombos;
        }
    }

    return combos;
}


//MOST RECENT
public static List<Dictionary<string, object>> GenerateCombinations(DesiredDeviceProperties properties)
{
    var result = new List<Dictionary<string, object>>();

    var propertyNames = typeof(DesiredDeviceProperties).GetProperties().Select(p => p.Name);
    var propertyValues = propertyNames.Select(name => properties.GetType().GetProperty(name)?.GetValue(properties) as IList<object>);

    var combinations = CartesianProduct(propertyValues?.Where(p => p != null));
    foreach (var combination in combinations)
    {
        var dict = new Dictionary<string, object>();
        for (int i = 0; i < propertyNames.Count(); i++)
        {
            var value = combination.ElementAt(i);
            if (value != null)
            {
                dict.Add(propertyNames.ElementAt(i), value);
            }
        }
        result.Add(dict);
    }
    return result;
}
private static IEnumerable<IEnumerable<object>> CartesianProduct(IEnumerable<IEnumerable<object>> sequences)
{
    IEnumerable<IEnumerable<object>> emptyProduct = new[] { Enumerable.Empty<object>() };
    return sequences.Aggregate(
        emptyProduct,
        (accumulator, sequence) =>
            from accseq in accumulator
            from item in sequence
            select accseq.Concat(new[] { item }));
}


//FIRST TRY
public List<Dictionary<string, object>> ExtractDeviceCombinations(DesiredDeviceProperties properties)
{
    //Ignore null values
    //JsonSerializerOptions options = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

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
*/

