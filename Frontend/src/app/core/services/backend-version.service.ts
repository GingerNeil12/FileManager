import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';

import { catchError, forkJoin, map, Observable, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { API_PATHS } from '../api-paths';
import { ServiceInfo } from '../models/service-info.model';

interface VersionResponse {
  name: string;
  version: string;
}

@Injectable({ providedIn: 'root' })
export class BackendVersionService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = environment.apiBaseUrl;

  getServiceInfo(): Observable<ServiceInfo> {
    return forkJoin({
      version: this.http.get<VersionResponse>(`${this.apiBaseUrl}${API_PATHS.version}`),
      health: this.http.get(`${this.apiBaseUrl}${API_PATHS.health}`, { responseType: 'text' }),
    }).pipe(
      map(({ version, health }) => ({
        name: version.name,
        version: version.version,
        healthStatus: health,
      })),
      catchError((error: HttpErrorResponse) =>
        throwError(() => new Error(`Unable to reach backend service. (${error.status})`)),
      ),
    );
  }
}
