using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TestMate.API.Settings;
using TestMate.Common.DataTransferObjects.TestRequests;
using TestMate.Common.Models.TestRequests;

namespace TestMate.API.Services
{
    public class TestRequestsService
    {
        private readonly IMongoCollection<TestRequest> _testRequestsCollection;
        private readonly IMapper _mapper;

        public TestRequestsService(IOptions<DatabaseSettings> databaseSettings, IMapper mapper)
        {
            var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
            var mongoDatabase = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
            _testRequestsCollection = mongoDatabase.GetCollection<TestRequest>(databaseSettings.Value.TestRequestsCollectionName);
            _mapper = mapper;        
        }

        //Returns all test requests
        public async Task<List<TestRequest>> GetAsync() => await _testRequestsCollection.Find(_ => true).ToListAsync();


        //Returns a test request by id or null if not found
        public async Task<TestRequest?> GetAsync(string id) => await _testRequestsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();


        //Creates a new test request
        public async Task<TestRequestCreateResultDTO> CreateAsync(TestRequestCreateDTO testRequestCreateDTO)
        {
            TestRequest testRequest = _mapper.Map<TestRequest>(testRequestCreateDTO);

            //TODO: if(testRequest.Requestor != //todo: user submitting the request?)
            testRequest.Timestamp = DateTime.UtcNow;
            testRequest.Status = "NEW";
            testRequest.Requestor = "TEST"; //TODO: UPDATE TO ACTUAL LOGGED IN DEVELOPER?
            await _testRequestsCollection.InsertOneAsync(testRequest);

            return new TestRequestCreateResultDTO { Id = testRequest.Id };
            
        }

        //Updates test request (by id) 
        public async Task UpdateAsync(string id, TestRequest updatedTestRequest)
        {
            await _testRequestsCollection.ReplaceOneAsync(x => x.Id == id, updatedTestRequest);
        }

        //Removes test request (by username)
        public async Task RemoveAsync(string id)
        {
            await _testRequestsCollection.DeleteOneAsync(x => x.Id == id);
        }

    }
}
