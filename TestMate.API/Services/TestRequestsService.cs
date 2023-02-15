using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TestMate.API.Settings;
using TestMate.Common.DataTransferObjects.APIResponse;
using TestMate.Common.DataTransferObjects.TestRequests;
using TestMate.Common.Enums;
using TestMate.Common.Models.TestRequests;
using Microsoft.IdentityModel.JsonWebTokens;


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
                return new APIResponse<TestRequestCreateResultDTO>(new TestRequestCreateResultDTO { Id = testRequest.Id });
            }
            catch (Exception ex)
            {
                return new APIResponse<TestRequestCreateResultDTO>(Status.Error, ex.Message);
            }
        }

    }
}
