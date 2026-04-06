import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { AuthService } from '@auth0/auth0-angular';
import { vi } from 'vitest';

import { authErrorInterceptor } from './auth-error.interceptor';

function createMockAuthService() {
  return {
    logout: vi.fn(),
  };
}

describe('authErrorInterceptor', () => {
  let http: HttpClient;
  let httpTestingController: HttpTestingController;
  let mockAuthService: ReturnType<typeof createMockAuthService>;

  beforeEach(() => {
    mockAuthService = createMockAuthService();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authErrorInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: mockAuthService },
      ],
    });

    http = TestBed.inject(HttpClient);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify();
    vi.clearAllMocks();
  });

  describe('when the server responds with 401 Unauthorized', () => {
    it('should call logout', () => {
      // Arrange
      http.get('/api/test').subscribe({ error: () => {} });

      // Act
      httpTestingController.expectOne('/api/test').flush(null, { status: 401, statusText: 'Unauthorized' });

      // Assert
      expect(mockAuthService.logout).toHaveBeenCalled();
    });

    it('should call logout with returnTo set to the current origin', () => {
      // Arrange
      http.get('/api/test').subscribe({ error: () => {} });

      // Act
      httpTestingController.expectOne('/api/test').flush(null, { status: 401, statusText: 'Unauthorized' });

      // Assert
      expect(mockAuthService.logout).toHaveBeenCalledWith({
        logoutParams: { returnTo: window.location.origin },
      });
    });

    it('should propagate the error', () => {
      // Arrange
      let caughtError: unknown;

      // Act
      http.get('/api/test').subscribe({ error: (err) => { caughtError = err; } });
      httpTestingController.expectOne('/api/test').flush(null, { status: 401, statusText: 'Unauthorized' });

      // Assert
      expect(caughtError).toBeDefined();
    });
  });

  describe('when the server responds with 403 Forbidden', () => {
    it('should not call logout', () => {
      // Act
      http.get('/api/test').subscribe({ error: () => {} });
      httpTestingController.expectOne('/api/test').flush(null, { status: 403, statusText: 'Forbidden' });

      // Assert
      expect(mockAuthService.logout).not.toHaveBeenCalled();
    });

    it('should propagate the error', () => {
      // Arrange
      let caughtError: unknown;

      // Act
      http.get('/api/test').subscribe({ error: (err) => { caughtError = err; } });
      httpTestingController.expectOne('/api/test').flush(null, { status: 403, statusText: 'Forbidden' });

      // Assert
      expect(caughtError).toBeDefined();
    });
  });

  describe('when the server responds with a non-auth error', () => {
    it('should not call logout on 500', () => {
      // Act
      http.get('/api/test').subscribe({ error: () => {} });
      httpTestingController.expectOne('/api/test').flush(null, { status: 500, statusText: 'Internal Server Error' });

      // Assert
      expect(mockAuthService.logout).not.toHaveBeenCalled();
    });

    it('should propagate the error on 500', () => {
      // Arrange
      let caughtError: unknown;

      // Act
      http.get('/api/test').subscribe({ error: (err) => { caughtError = err; } });
      httpTestingController.expectOne('/api/test').flush(null, { status: 500, statusText: 'Internal Server Error' });

      // Assert
      expect(caughtError).toBeDefined();
    });
  });

  describe('when the request succeeds', () => {
    it('should not call logout', () => {
      // Act
      http.get('/api/test').subscribe();
      httpTestingController.expectOne('/api/test').flush({ data: 'ok' });

      // Assert
      expect(mockAuthService.logout).not.toHaveBeenCalled();
    });
  });
});
