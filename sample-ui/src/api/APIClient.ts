/* tslint:disable */

/**
 * This file was automatically generated by "Swaxios".
 * It should not be modified by hand.
 */

import axios, { AxiosInstance, AxiosRequestConfig } from 'axios';
import {
  ApplicationsService,
  ServicePrincipalsService,
  UsersService,
} from './rest/v1_0/';
import { AppRoleAssignmentsService, SearchService } from './rest/v1_0/Users/';
import {
  AppRoleAssignedToService,
  AppRolesService,
} from './rest/v1_0/servicePrincipals/';
import MsalHandler from '../components/auth/MsalHandler';

export class APIClient {
  private readonly httpClient: AxiosInstance;

  constructor(baseURL: string);
  constructor(config: AxiosRequestConfig);
  constructor(configOrBaseURL: AxiosRequestConfig | string) {
    if (typeof configOrBaseURL === 'string') {
      configOrBaseURL = { baseURL: configOrBaseURL };
    }

    const msalHandler = MsalHandler.getInstance();

    this.httpClient = axios.create(configOrBaseURL);
    this.httpClient.interceptors.request.use(
      async req => {
        console.log("api::interceptor: request.url: " + req.url);
        var token = await msalHandler.acquireAccessToken();
        req.headers["Authorization"] = "Bearer " + token;
        return req;
      }
    );
  }

  get rest() {
    return {
      v1_0: {
        applicationsService: new ApplicationsService(this.httpClient),
        servicePrincipalsService: new ServicePrincipalsService(this.httpClient),
        usersService: new UsersService(this.httpClient),
        users: {
          appRoleAssignmentsService: new AppRoleAssignmentsService(
            this.httpClient
          ),
          searchService: new SearchService(this.httpClient),
        },
        serviceprincipals: {
          appRoleAssignedToService: new AppRoleAssignedToService(
            this.httpClient
          ),
          appRolesService: new AppRolesService(this.httpClient),
        },
      },
    };
  }

  get defaults() {
    return this.httpClient.defaults;
  }

  get interceptors() {
    return this.httpClient.interceptors;
  }
}
