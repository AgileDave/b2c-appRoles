/* tslint:disable */

/**
 * This file was automatically generated by "Swaxios".
 * It should not be modified by hand.
 */

import {AxiosInstance, AxiosRequestConfig} from 'axios';
import {AppRoleAssignment} from '../../../interfaces/';

export class AppRoleAssignmentsService {
  private readonly apiClient: AxiosInstance;

  constructor(apiClient: AxiosInstance) {
    this.apiClient = apiClient;
  }

  getByUserId = async (userId: string): Promise<Array<AppRoleAssignment>> => {
    const config: AxiosRequestConfig = {
      method: 'get',
      url: `/v1.0/Users/${userId}/appRoleAssignments`,
    };
    const response = await this.apiClient.request<Array<AppRoleAssignment>>(
      config
    );
    return response.data;
  };
}
