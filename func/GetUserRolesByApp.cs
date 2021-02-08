using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.Graph;

namespace AzureAdB2CAppRoleShim
{
    public class UserAppRoleRequest
    {
        public string UserId { get; set; }
        public string ApplicationId { get; set; }

    }

    public class Functions
    {
        private readonly GraphServiceClient _graphClient;
        private readonly ILogger<Functions> _log;
        public Functions(GraphServiceClient client, ILoggerFactory loggerFactory)
        {
            _graphClient = client;
            _log = loggerFactory.CreateLogger<Functions>();
        }

        [FunctionName("GetUserRolesByApp")]
        public async Task<IActionResult> GetUserRolesByApp([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "users/{userId}/appRoleAssignments/{applicationId}")] UserAppRoleRequest req)
        {
            _log.LogDebug($"Received request: app {req.ApplicationId}; user {req.UserId}");
            try
            {
                // https://graph.microsoft.com/v1.0/servicePrincipals?$filter=appId eq '05c77d87-2fcc-44fd-b385-ae8080afa879'&$select=id
                var servicePrincipalSearch = await _graphClient.ServicePrincipals.Request().Select(x => new { x.Id, x.AppRoles }).Filter($"appId eq '{req.ApplicationId}'").GetAsync();
                if (servicePrincipalSearch.Count != 1)
                {
                    _log.LogError($"Can't find service principal for app id {req.ApplicationId}, exiting");
                    //throw new Exception("SP ID is returning too many or not enough - weird");
                    return new ConflictObjectResult(new { Message = $"Can't find the service principal corresponding to app ID {req.ApplicationId}" });
                }

                var spOfApplication = servicePrincipalSearch.Single();
                _log.LogDebug($"Found service principal {spOfApplication.Id} with {spOfApplication.AppRoles.Count()} app roles");

                var availableRoles = spOfApplication.AppRoles;

                // /users/39a0e707-d275-4860-b534-5cd1c2d2dbe1/appRoleAssignments?$filter=resourceId eq fd076aa7-1423-4587-b0f1-e160f49f679f'
                // $"users/{userObjectId}/appRoleAssignments?$filter=resourceId eq {servicePrincipalId}&$select=principalId,resourceId,appRoleId"
                _log.LogDebug($"Checking user's appRole assignment...");
                var userAppRoleAssignmentListRequest = await _graphClient.Users[req.UserId]
                    .AppRoleAssignments.Request()
                        .Filter($"resourceId eq {spOfApplication.Id}")
                        .Select(y => new { y.PrincipalId, y.ResourceId, y.AppRoleId })
                    .GetAsync();

                var userAppRoleAssignmentList = userAppRoleAssignmentListRequest.ToList();
                _log.LogDebug($"User is a member of {userAppRoleAssignmentList.Count} appRoles");

                //$"servicePrincipal/{servicePrincipalId}/appRoles"
                var listOfAppRoleValuesUserIsAMemberOf = spOfApplication.AppRoles.Where(x => x.IsEnabled ?? false && userAppRoleAssignmentList.Select(x => x.AppRoleId).Contains(x.Id))
                            .Select(appRole => appRole.Value);

                _log.LogDebug($"Resolved {listOfAppRoleValuesUserIsAMemberOf.Count()} appRole values: {string.Join(',', listOfAppRoleValuesUserIsAMemberOf)}");

                return new OkObjectResult(new { Roles = listOfAppRoleValuesUserIsAMemberOf });
            }
            catch (ServiceException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ConflictObjectResult(new { Message = "Not found - have you registered yet?" });
                }
                return new ConflictObjectResult(ex.Message);
            }
        }
    }
}