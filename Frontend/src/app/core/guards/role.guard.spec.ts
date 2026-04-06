import { TestBed } from '@angular/core/testing';
import { AuthService } from '@auth0/auth0-angular';
import { BehaviorSubject, Subject } from 'rxjs';
import { vi } from 'vitest';

import { roleGuard } from './role.guard';

const ROLES_CLAIM = 'http://localhost/roles';

function createMockAuthService(roles: string[] | null, isLoading: boolean) {
  const user = roles !== null ? { [ROLES_CLAIM]: roles } : null;
  return {
    user$: new BehaviorSubject(user),
    isLoading$: new BehaviorSubject<boolean>(isLoading),
  };
}

describe('roleGuard', () => {
  afterEach(() => vi.clearAllMocks());

  describe('when SDK is still loading', () => {
    it('should not emit while isLoading$ is true', () => {
      // Arrange
      const isLoading$ = new Subject<boolean>();
      const mockAuthService = {
        user$: new BehaviorSubject({ [ROLES_CLAIM]: ['InternalAdmin'] }),
        isLoading$,
      };

      TestBed.configureTestingModule({
        providers: [{ provide: AuthService, useValue: mockAuthService }],
      });

      let emitted = false;

      // Act
      TestBed.runInInjectionContext(() => roleGuard(['InternalAdmin'])()).subscribe(() => {
        emitted = true;
      });

      // Assert
      expect(emitted).toBe(false);
    });
  });

  describe('when user has an allowed role', () => {
    it('should return true', () => {
      // Arrange
      const mockAuthService = createMockAuthService(['InternalAdmin'], false);

      TestBed.configureTestingModule({
        providers: [{ provide: AuthService, useValue: mockAuthService }],
      });

      let result: boolean | undefined;

      // Act
      TestBed.runInInjectionContext(() => roleGuard(['InternalAdmin'])()).subscribe((value) => {
        result = value;
      });

      // Assert
      expect(result).toBe(true);
    });

    it('should return true when one of multiple allowed roles matches', () => {
      // Arrange
      const mockAuthService = createMockAuthService(['InternalUser'], false);

      TestBed.configureTestingModule({
        providers: [{ provide: AuthService, useValue: mockAuthService }],
      });

      let result: boolean | undefined;

      // Act
      TestBed.runInInjectionContext(() =>
        roleGuard(['InternalAdmin', 'InternalUser'])()
      ).subscribe((value) => {
        result = value;
      });

      // Assert
      expect(result).toBe(true);
    });

    it('should return true when user has multiple roles and one matches', () => {
      // Arrange
      const mockAuthService = createMockAuthService(['ExternalUser', 'InternalAdmin'], false);

      TestBed.configureTestingModule({
        providers: [{ provide: AuthService, useValue: mockAuthService }],
      });

      let result: boolean | undefined;

      // Act
      TestBed.runInInjectionContext(() => roleGuard(['InternalAdmin'])()).subscribe((value) => {
        result = value;
      });

      // Assert
      expect(result).toBe(true);
    });
  });

  describe('when user does not have an allowed role', () => {
    it('should return false', () => {
      // Arrange
      const mockAuthService = createMockAuthService(['ExternalUser'], false);

      TestBed.configureTestingModule({
        providers: [{ provide: AuthService, useValue: mockAuthService }],
      });

      let result: boolean | undefined;

      // Act
      TestBed.runInInjectionContext(() => roleGuard(['InternalAdmin'])()).subscribe((value) => {
        result = value;
      });

      // Assert
      expect(result).toBe(false);
    });
  });

  describe('when user has no roles claim', () => {
    it('should return false when roles claim is absent', () => {
      // Arrange
      const mockAuthService = createMockAuthService(null, false);

      TestBed.configureTestingModule({
        providers: [{ provide: AuthService, useValue: mockAuthService }],
      });

      let result: boolean | undefined;

      // Act
      TestBed.runInInjectionContext(() => roleGuard(['InternalAdmin'])()).subscribe((value) => {
        result = value;
      });

      // Assert
      expect(result).toBe(false);
    });

    it('should return false when roles claim is empty', () => {
      // Arrange
      const mockAuthService = createMockAuthService([], false);

      TestBed.configureTestingModule({
        providers: [{ provide: AuthService, useValue: mockAuthService }],
      });

      let result: boolean | undefined;

      // Act
      TestBed.runInInjectionContext(() => roleGuard(['InternalAdmin'])()).subscribe((value) => {
        result = value;
      });

      // Assert
      expect(result).toBe(false);
    });
  });

  describe('when loading transitions from true to false', () => {
    it('should emit true after isLoading$ transitions to false when user has an allowed role', () => {
      // Arrange
      const isLoading$ = new BehaviorSubject<boolean>(true);
      const mockAuthService = {
        user$: new BehaviorSubject({ [ROLES_CLAIM]: ['InternalAdmin'] }),
        isLoading$,
      };

      TestBed.configureTestingModule({
        providers: [{ provide: AuthService, useValue: mockAuthService }],
      });

      let result: boolean | undefined;
      TestBed.runInInjectionContext(() => roleGuard(['InternalAdmin'])()).subscribe((value) => {
        result = value;
      });

      expect(result).toBeUndefined();

      // Act
      isLoading$.next(false);

      // Assert
      expect(result).toBe(true);
    });

    it('should emit false after isLoading$ transitions to false when user lacks an allowed role', () => {
      // Arrange
      const isLoading$ = new BehaviorSubject<boolean>(true);
      const mockAuthService = {
        user$: new BehaviorSubject({ [ROLES_CLAIM]: ['ExternalUser'] }),
        isLoading$,
      };

      TestBed.configureTestingModule({
        providers: [{ provide: AuthService, useValue: mockAuthService }],
      });

      let result: boolean | undefined;
      TestBed.runInInjectionContext(() => roleGuard(['InternalAdmin'])()).subscribe((value) => {
        result = value;
      });

      expect(result).toBeUndefined();

      // Act
      isLoading$.next(false);

      // Assert
      expect(result).toBe(false);
    });
  });
});
