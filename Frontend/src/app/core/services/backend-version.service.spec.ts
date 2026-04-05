import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { environment } from '../../../environments/environment';
import { ServiceInfo } from '../models/service-info.model';
import { BackendVersionService } from './backend-version.service';

describe('BackendVersionService', () => {
  let service: BackendVersionService;
  let httpTestingController: HttpTestingController;

  const VERSION_URL = `${environment.apiBaseUrl}/api/version`;
  const HEALTH_URL = `${environment.apiBaseUrl}/health`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });

    service = TestBed.inject(BackendVersionService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
  });

  describe('getServiceInfo', () => {
    describe('requests', () => {
      it('should call the version endpoint with GET', () => {
        // Arrange
        service.getServiceInfo().subscribe();

        // Act
        const req = httpTestingController.expectOne(VERSION_URL);
        httpTestingController.expectOne(HEALTH_URL).flush('Healthy');
        req.flush({ name: 'backend', version: '1.0.0' });

        // Assert
        expect(req.request.method).toBe('GET');
      });

      it('should call the health endpoint with GET', () => {
        // Arrange
        service.getServiceInfo().subscribe();

        // Act
        const req = httpTestingController.expectOne(HEALTH_URL);
        httpTestingController.expectOne(VERSION_URL).flush({ name: 'backend', version: '1.0.0' });
        req.flush('Healthy');

        // Assert
        expect(req.request.method).toBe('GET');
      });
    });

    describe('response mapping', () => {
      it('should map the version response name to ServiceInfo name', () => {
        // Arrange
        let result: ServiceInfo | undefined;
        service.getServiceInfo().subscribe((info) => (result = info));

        // Act
        httpTestingController.expectOne(VERSION_URL).flush({ name: 'filemanager-backend', version: '1.0.0' });
        httpTestingController.expectOne(HEALTH_URL).flush('Healthy');

        // Assert
        expect(result?.name).toBe('filemanager-backend');
      });

      it('should map the version response version to ServiceInfo version', () => {
        // Arrange
        let result: ServiceInfo | undefined;
        service.getServiceInfo().subscribe((info) => (result = info));

        // Act
        httpTestingController.expectOne(VERSION_URL).flush({ name: 'backend', version: '2.5.1' });
        httpTestingController.expectOne(HEALTH_URL).flush('Healthy');

        // Assert
        expect(result?.version).toBe('2.5.1');
      });

      it('should map the health response text to ServiceInfo healthStatus', () => {
        // Arrange
        let result: ServiceInfo | undefined;
        service.getServiceInfo().subscribe((info) => (result = info));

        // Act
        httpTestingController.expectOne(VERSION_URL).flush({ name: 'backend', version: '1.0.0' });
        httpTestingController.expectOne(HEALTH_URL).flush('Degraded');

        // Assert
        expect(result?.healthStatus).toBe('Degraded');
      });
    });

    describe('error handling', () => {
      it('should throw a clean error when the version endpoint returns 500', () => {
        // Arrange
        let error: Error | undefined;
        service.getServiceInfo().subscribe({ error: (e: Error) => (error = e) });

        // Act
        httpTestingController.expectOne(VERSION_URL).flush(null, { status: 500, statusText: 'Internal Server Error' });
        // forkJoin cancels the health request when version errors; drain it so verify() passes
        httpTestingController.match(HEALTH_URL);

        // Assert
        expect(error?.message).toBe('Unable to reach backend service. (500)');
      });

      it('should throw a clean error when the health endpoint returns 503', () => {
        // Arrange
        let error: Error | undefined;
        service.getServiceInfo().subscribe({ error: (e: Error) => (error = e) });

        // Act — both requests complete; no dangling requests
        httpTestingController.expectOne(VERSION_URL).flush({ name: 'backend', version: '1.0.0' });
        httpTestingController.expectOne(HEALTH_URL).flush(null, { status: 503, statusText: 'Service Unavailable' });

        // Assert
        expect(error?.message).toBe('Unable to reach backend service. (503)');
      });

      it('should throw a clean error when the version endpoint is unreachable (status 0)', () => {
        // Arrange
        let error: Error | undefined;
        service.getServiceInfo().subscribe({ error: (e: Error) => (error = e) });

        // Act
        httpTestingController.expectOne(VERSION_URL).error(new ProgressEvent('error'));
        httpTestingController.match(HEALTH_URL);

        // Assert
        expect(error?.message).toBe('Unable to reach backend service. (0)');
      });

      it('should not expose raw HttpErrorResponse details in the thrown error', () => {
        // Arrange
        let error: Error | undefined;
        service.getServiceInfo().subscribe({ error: (e: Error) => (error = e) });

        // Act
        httpTestingController.expectOne(VERSION_URL).flush(null, { status: 404, statusText: 'Not Found' });
        httpTestingController.match(HEALTH_URL);

        // Assert
        expect(error).toBeInstanceOf(Error);
        expect(error?.message).not.toContain('HttpErrorResponse');
      });
    });
  });
});
