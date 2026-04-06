import { TestBed } from '@angular/core/testing';
import { AuthService } from '@auth0/auth0-angular';
import { BehaviorSubject, Subject } from 'rxjs';
import { vi } from 'vitest';

import { authGuard } from './auth.guard';

function createMockAuthService(isAuthenticated: boolean, isLoading: boolean) {
  return {
    isAuthenticated$: new BehaviorSubject<boolean>(isAuthenticated),
    isLoading$: new BehaviorSubject<boolean>(isLoading),
    loginWithRedirect: vi.fn(),
    logout: vi.fn(),
  };
}

describe('authGuard', () => {
  afterEach(() => vi.clearAllMocks());

  describe('when SDK is still loading', () => {
    it('should not emit while isLoading$ is true', () => {
      // Arrange
      const isLoading$ = new Subject<boolean>();
      const mockAuthService = {
        isAuthenticated$: new BehaviorSubject<boolean>(false),
        isLoading$,
        loginWithRedirect: vi.fn(),
        logout: vi.fn(),
      };

      TestBed.configureTestingModule({
        providers: [{ provide: AuthService, useValue: mockAuthService }],
      });

      let emitted = false;

      // Act
      TestBed.runInInjectionContext(() => authGuard()).subscribe(() => {
        emitted = true;
      });

      // Assert — isLoading$ has not emitted false yet, so the guard should not have emitted
      expect(emitted).toBe(false);
    });
  });

  describe('when SDK finishes loading and user is unauthenticated', () => {
    it('should return false', () => {
      // Arrange
      const mockAuthService = createMockAuthService(false, false);

      TestBed.configureTestingModule({
        providers: [{ provide: AuthService, useValue: mockAuthService }],
      });

      let result: boolean | undefined;

      // Act
      TestBed.runInInjectionContext(() => authGuard()).subscribe((value) => {
        result = value;
      });

      // Assert
      expect(result).toBe(false);
    });

    it('should call loginWithRedirect', () => {
      // Arrange
      const mockAuthService = createMockAuthService(false, false);

      TestBed.configureTestingModule({
        providers: [{ provide: AuthService, useValue: mockAuthService }],
      });

      // Act
      TestBed.runInInjectionContext(() => authGuard()).subscribe();

      // Assert
      expect(mockAuthService.loginWithRedirect).toHaveBeenCalledWith();
    });

    it('should call loginWithRedirect exactly once', () => {
      // Arrange
      const mockAuthService = createMockAuthService(false, false);

      TestBed.configureTestingModule({
        providers: [{ provide: AuthService, useValue: mockAuthService }],
      });

      // Act
      TestBed.runInInjectionContext(() => authGuard()).subscribe();

      // Assert
      expect(mockAuthService.loginWithRedirect).toHaveBeenCalledTimes(1);
    });
  });

  describe('when SDK finishes loading and user is authenticated', () => {
    it('should return true', () => {
      // Arrange
      const mockAuthService = createMockAuthService(true, false);

      TestBed.configureTestingModule({
        providers: [{ provide: AuthService, useValue: mockAuthService }],
      });

      let result: boolean | undefined;

      // Act
      TestBed.runInInjectionContext(() => authGuard()).subscribe((value) => {
        result = value;
      });

      // Assert
      expect(result).toBe(true);
    });

    it('should not call loginWithRedirect', () => {
      // Arrange
      const mockAuthService = createMockAuthService(true, false);

      TestBed.configureTestingModule({
        providers: [{ provide: AuthService, useValue: mockAuthService }],
      });

      // Act
      TestBed.runInInjectionContext(() => authGuard()).subscribe();

      // Assert
      expect(mockAuthService.loginWithRedirect).not.toHaveBeenCalled();
    });
  });

  describe('when loading transitions from true to false', () => {
    it('should emit after isLoading$ transitions to false while authenticated', () => {
      // Arrange
      const isLoading$ = new BehaviorSubject<boolean>(true);
      const mockAuthService = {
        isAuthenticated$: new BehaviorSubject<boolean>(true),
        isLoading$,
        loginWithRedirect: vi.fn(),
        logout: vi.fn(),
      };

      TestBed.configureTestingModule({
        providers: [{ provide: AuthService, useValue: mockAuthService }],
      });

      let result: boolean | undefined;
      TestBed.runInInjectionContext(() => authGuard()).subscribe((value) => {
        result = value;
      });

      expect(result).toBeUndefined();

      // Act — SDK finishes loading
      isLoading$.next(false);

      // Assert
      expect(result).toBe(true);
    });

    it('should emit after isLoading$ transitions to false while unauthenticated', () => {
      // Arrange
      const isLoading$ = new BehaviorSubject<boolean>(true);
      const mockAuthService = {
        isAuthenticated$: new BehaviorSubject<boolean>(false),
        isLoading$,
        loginWithRedirect: vi.fn(),
        logout: vi.fn(),
      };

      TestBed.configureTestingModule({
        providers: [{ provide: AuthService, useValue: mockAuthService }],
      });

      let result: boolean | undefined;
      TestBed.runInInjectionContext(() => authGuard()).subscribe((value) => {
        result = value;
      });

      expect(result).toBeUndefined();

      // Act — SDK finishes loading
      isLoading$.next(false);

      // Assert
      expect(result).toBe(false);
    });
  });
});
