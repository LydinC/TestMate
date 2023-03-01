using MongoDB.Driver;
using AutoMapper;
using TestMate.API.Settings;
using TestMate.Common.Models.TestRuns;
using Microsoft.Extensions.Options;
using TestMate.Common.DataTransferObjects.APIResponse;
using TestMate.Common.Enums;
using Microsoft.AspNetCore.Mvc;

namespace TestMate.API.Services
{
    public class TestRunsService
    {
        private readonly IMongoCollection<TestRun> _testRunsCollection;
        private readonly IMapper _mapper;
        private readonly ILogger<TestRunsService> _logger;

        public TestRunsService(IOptions<DatabaseSettings> databaseSettings, IMapper mapper, ILogger<TestRunsService> logger)
        {
            var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
            _testRunsCollection = mongoDatabase.GetCollection<TestRun>(databaseSettings.Value.TestRunsCollectionName);
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<APIResponse<IEnumerable<TestRun>>> GetTestRuns()
        {
            try
            {
                List<TestRun> testRuns = await _testRunsCollection.Find(_ => true).ToListAsync();
                return new APIResponse<IEnumerable<TestRun>>(testRuns);
            }
            catch (Exception ex)
            {
                return new APIResponse<IEnumerable<TestRun>>(Status.Error, ex.Message);
            }
        }

        public async Task<APIResponse<IEnumerable<TestRun>>> GetTestRunsByTestRequestID(Guid testRequestId)
        {
            try
            {
                List<TestRun> testRuns = await _testRunsCollection.Find(x => x.TestRequestID == testRequestId).ToListAsync();
                return new APIResponse<IEnumerable<TestRun>>(testRuns);
            }
            catch (Exception ex)
            {
                return new APIResponse<IEnumerable<TestRun>>(Status.Error, ex.Message);
            }
        }


        public async Task<APIResponse<TestRun>> GetTestRunDetails(string id)
        {
            try
            {
                TestRun testRun = await _testRunsCollection.Find(x => x.Id == id).SingleOrDefaultAsync();
                if(testRun == null)
                {
                    return new APIResponse<TestRun>(Status.Error, $"Test Run ID {id} does not exist.");
                }
                
                return new APIResponse<TestRun>(testRun);
            }
            catch (Exception ex)
            {
                return new APIResponse<TestRun>(Status.Error, ex.Message);
            }
        }


        public async Task<APIResponse<TestRunStatus>> GetStatus(string id)
        {
            try
            {
                TestRun testRun = await _testRunsCollection.Find(x => x.Id == id).SingleOrDefaultAsync();

                if(testRun == null)
                {
                    return new APIResponse<TestRunStatus>(Status.Error, $"Test Run ID {id} does not exist.");
                }
                return new APIResponse<TestRunStatus>(testRun.Status);
            }
            catch (Exception ex)
            {
                return new APIResponse<TestRunStatus>(Status.Error, ex.Message);
            }
        }

    }
}