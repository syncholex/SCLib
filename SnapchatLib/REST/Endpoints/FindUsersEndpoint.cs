﻿using System;
using System.Threading.Tasks;
using SnapchatLib.Extras;
using SnapchatLib.Exceptions;
using SnapProto.Snapchat.Friending;
using System.Collections.Generic;
using System.Text.Json;

namespace SnapchatLib.REST.Endpoints;

public interface IFindUsersEndpoint
{
    Task<SCFriendingContactBookUploadResponse> FindUsersViaEmail(string email, string CountryCode, string randomfirstname);
    Task<SCFriendingContactBookUploadResponse> FindUsersViaPhone(string number, string CountryCode, string randomfirstname);
    Task<List<string>> ReturnUsernameViaPhone(string number, string CountryCode, string randomfirstname);
    Task<List<string>> ReturnUsernameViaEmail(string email, string CountryCode, string randomfirstname);
}
internal class FindUsersEndpoint : EndpointAccessor, IFindUsersEndpoint
{
    internal static readonly EndpointInfo FindFriendRegInfo = new() { Url = "/bq/find_friends_reg", Requirements = EndpointRequirements.XSnapchatUUID | EndpointRequirements.XSnapAccessToken, BaseEndpoint = RequestConfigurator.ApiGCPEast4Endpoint };
    public FindUsersEndpoint(SnapchatClient client, ISnapchatHttpClient httpClient, ISnapchatGrpcClient grpcClient, SnapchatLockedConfig config, IClientLogger logger, IUtilities utilities, IRequestConfigurator configurator) : base(client, httpClient, grpcClient, config, logger, utilities, configurator)
    {
    }

    public async Task<find_friends_reg> FindFriendsRegister(string CountryCode)
    {
        var parameters = new Dictionary<string, string>
        {
            {"countryCode", CountryCode},
            {"dtoken1i", Config.dtoken1i},
            {"is_full_sync", "true"},
            {"numbers", "{}"},
            {"should_recommend", "false"},
            {"snapchat_user_id", Config.user_id},
            {"source", "REGISTRATION"},
        };

        var response = await Send(FindFriendRegInfo, parameters);
        return JsonSerializer.Deserialize<find_friends_reg>(await response.Content.ReadAsStringAsync());
    }

    public async Task<SCFriendingContactBookUploadResponse> FindUsersViaPhone(string number, string CountryCode, string randomfirstname)
    {
        SCFriendingContactFromClient _SCFriendingContactFromClients = new()
        {
            DisplayName = randomfirstname
        };
        _SCFriendingContactFromClients.ContactMethodsArray.Add(new SCFriendingContactMethod { ContactMethodId = Guid.NewGuid().ToString().ToUpper().ToString() + ":ABPerson", PhoneNumber = number });
        var SCFriendingContactBookUploadRequest = new SCFriendingContactBookUploadRequest
        {
            CountryCode = CountryCode,
            ContactsArray = { _SCFriendingContactFromClients }
        };
        var reply = await SnapchatGrpcClient.FullSyncContactBookUploadAsync(SCFriendingContactBookUploadRequest);
        return reply;
    }
    public async Task<SCFriendingContactBookUploadResponse> FindUsersViaEmail(string email, string CountryCode, string randomfirstname)
    {
        SCFriendingContactFromClient _SCFriendingContactFromClients = new()
        {
            DisplayName = randomfirstname
        };
        SCFriendingContactMethod _SCFriendingContactMethod = new SCFriendingContactMethod();
        _SCFriendingContactMethod.ContactMethodId = Guid.NewGuid().ToString().ToUpper().ToString() + ":ABPerson";
        _SCFriendingContactMethod.EmailAddress = email;
        _SCFriendingContactFromClients.ContactMethodsArray.Add(_SCFriendingContactMethod);
        var _SCFriendingContactBookUploadRequest = new SCFriendingContactBookUploadRequest
        {
            CountryCode = CountryCode,
            ContactsArray = { _SCFriendingContactFromClients },
        };
        var reply = await SnapchatGrpcClient.FullSyncContactBookUploadAsync(_SCFriendingContactBookUploadRequest);
        return reply;
    }
    public async Task<List<string>> ReturnUsernameViaPhone(string number, string CountryCode, string randomfirstname)
    {
        var parsedData = await FindUsersViaPhone(number, CountryCode, randomfirstname);

        if (parsedData.SnapchattersArray.Count == 0) throw new UserNumberNotFoundException(number);

        List<string> snapchatters = new List<string>();

        foreach (var snapchatter in parsedData.SnapchattersArray)
            snapchatters.Add(snapchatter.Username);

        return snapchatters;
    }
    public async Task<List<string>> ReturnUsernameViaEmail(string email, string CountryCode, string randomfirstname)
    {
        var parsedData = await FindUsersViaEmail(email, CountryCode, randomfirstname);

        if (parsedData.SnapchattersArray.Count == 0) throw new UserNumberNotFoundException(email);

        List<string> snapchatters = new List<string>();

        foreach (var snapchatter in parsedData.SnapchattersArray)
            snapchatters.Add(snapchatter.Username);

        return snapchatters;
    }
}