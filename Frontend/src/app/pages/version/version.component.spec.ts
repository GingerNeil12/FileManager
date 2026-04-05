import { TestBed } from '@angular/core/testing';

import { of, throwError } from 'rxjs';
import { vi } from 'vitest';

import { environment } from '../../../environments/environment';
import { BackendVersionService } from '../../core/services/backend-version.service';
import { VersionComponent } from './version.component';

const BACKEND_SERVICE_INFO = {
  name: 'filemanager-backend',
  version: '1.0.0',
  healthStatus: 'Healthy',
};

describe('VersionComponent', () => {
  let mockBackendVersionService: { getServiceInfo: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    mockBackendVersionService = { getServiceInfo: vi.fn().mockReturnValue(of(BACKEND_SERVICE_INFO)) };

    await TestBed.configureTestingModule({
      imports: [VersionComponent],
      providers: [{ provide: BackendVersionService, useValue: mockBackendVersionService }],
    }).compileComponents();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('should create the component', () => {
    // Arrange / Act
    const fixture = TestBed.createComponent(VersionComponent);
    fixture.detectChanges();

    // Assert
    expect(fixture.componentInstance).toBeTruthy();
  });

  describe('frontend service card', () => {
    it('should display the frontend app name immediately', () => {
      // Arrange
      mockBackendVersionService.getServiceInfo.mockReturnValue(of());
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      const titles = fixture.nativeElement.querySelectorAll('h5.card-title') as NodeListOf<HTMLElement>;

      // Assert
      expect(titles[0].textContent?.trim()).toBe(environment.appName);
    });

    it('should display the frontend app version immediately', () => {
      // Arrange
      mockBackendVersionService.getServiceInfo.mockReturnValue(of());
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      const versionTexts = fixture.nativeElement.querySelectorAll('p.card-text') as NodeListOf<HTMLElement>;

      // Assert
      expect(versionTexts[0].textContent?.trim()).toBe(`v${environment.appVersion}`);
    });

    it('should not render a status paragraph for the frontend card', () => {
      // Arrange
      mockBackendVersionService.getServiceInfo.mockReturnValue(of());
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      const cards = fixture.nativeElement.querySelectorAll('.card') as NodeListOf<HTMLElement>;
      const frontendStatusParagraph = cards[0].querySelector('p.card-text:nth-of-type(2)');

      // Assert
      expect(frontendStatusParagraph).toBeNull();
    });
  });

  describe('backend service card', () => {
    it('should display both the frontend and backend cards after the backend loads', () => {
      // Arrange
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      fixture.detectChanges();
      const cards = fixture.nativeElement.querySelectorAll('.card') as NodeListOf<HTMLElement>;

      // Assert
      expect(cards.length).toBe(2);
    });

    it('should display the backend service name', () => {
      // Arrange
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      fixture.detectChanges();
      const titles = fixture.nativeElement.querySelectorAll('h5.card-title') as NodeListOf<HTMLElement>;

      // Assert
      expect(titles[1].textContent?.trim()).toBe(BACKEND_SERVICE_INFO.name);
    });

    it('should display the backend service version', () => {
      // Arrange
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      fixture.detectChanges();
      const versionTexts = fixture.nativeElement.querySelectorAll('p.card-text') as NodeListOf<HTMLElement>;

      // Assert
      expect(versionTexts[1].textContent?.trim()).toBe(`v${BACKEND_SERVICE_INFO.version}`);
    });

    it('should display the health status for the backend card', () => {
      // Arrange
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      fixture.detectChanges();
      const cards = fixture.nativeElement.querySelectorAll('.card') as NodeListOf<HTMLElement>;
      const statusBadge = cards[1].querySelector('span.badge') as HTMLElement;

      // Assert
      expect(statusBadge.textContent?.trim()).toBe(BACKEND_SERVICE_INFO.healthStatus);
    });

    it('should apply bg-success to the badge when healthStatus is "Healthy"', () => {
      // Arrange
      mockBackendVersionService.getServiceInfo.mockReturnValue(of({ ...BACKEND_SERVICE_INFO, healthStatus: 'Healthy' }));
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      fixture.detectChanges();
      const cards = fixture.nativeElement.querySelectorAll('.card') as NodeListOf<HTMLElement>;
      const statusBadge = cards[1].querySelector('span.badge') as HTMLElement;

      // Assert
      expect(statusBadge.classList).toContain('bg-success');
    });

    it('should apply bg-success to the badge when healthStatus is "HEALTHY" (case insensitive)', () => {
      // Arrange
      mockBackendVersionService.getServiceInfo.mockReturnValue(of({ ...BACKEND_SERVICE_INFO, healthStatus: 'HEALTHY' }));
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      fixture.detectChanges();
      const cards = fixture.nativeElement.querySelectorAll('.card') as NodeListOf<HTMLElement>;
      const statusBadge = cards[1].querySelector('span.badge') as HTMLElement;

      // Assert
      expect(statusBadge.classList).toContain('bg-success');
    });

    it('should apply bg-danger to the badge when healthStatus is "Degraded"', () => {
      // Arrange
      mockBackendVersionService.getServiceInfo.mockReturnValue(of({ ...BACKEND_SERVICE_INFO, healthStatus: 'Degraded' }));
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      fixture.detectChanges();
      const cards = fixture.nativeElement.querySelectorAll('.card') as NodeListOf<HTMLElement>;
      const statusBadge = cards[1].querySelector('span.badge') as HTMLElement;

      // Assert
      expect(statusBadge.classList).toContain('bg-danger');
    });

    it('should apply bg-danger to the badge when healthStatus is "Unhealthy"', () => {
      // Arrange
      mockBackendVersionService.getServiceInfo.mockReturnValue(of({ ...BACKEND_SERVICE_INFO, healthStatus: 'Unhealthy' }));
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      fixture.detectChanges();
      const cards = fixture.nativeElement.querySelectorAll('.card') as NodeListOf<HTMLElement>;
      const statusBadge = cards[1].querySelector('span.badge') as HTMLElement;

      // Assert
      expect(statusBadge.classList).toContain('bg-danger');
    });

    it('should call getServiceInfo with no arguments', () => {
      // Arrange
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();

      // Assert
      expect(mockBackendVersionService.getServiceInfo).toHaveBeenCalledWith();
    });

    it('should call getServiceInfo exactly once on init', () => {
      // Arrange
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();

      // Assert
      expect(mockBackendVersionService.getServiceInfo).toHaveBeenCalledTimes(1);
    });
  });

  describe('isCardHealthy', () => {
    it('should apply bg-success to the frontend card which has no healthStatus', () => {
      // Arrange
      mockBackendVersionService.getServiceInfo.mockReturnValue(of());
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      const cards = fixture.nativeElement.querySelectorAll('.card') as NodeListOf<HTMLElement>;

      // Assert
      expect(cards[0].classList).toContain('bg-success');
    });

    it('should apply bg-success to the card when healthStatus is "Healthy"', () => {
      // Arrange
      mockBackendVersionService.getServiceInfo.mockReturnValue(of({ ...BACKEND_SERVICE_INFO, healthStatus: 'Healthy' }));
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      fixture.detectChanges();
      const cards = fixture.nativeElement.querySelectorAll('.card') as NodeListOf<HTMLElement>;

      // Assert
      expect(cards[1].classList).toContain('bg-success');
    });

    it('should apply bg-success to the card when healthStatus is "HEALTHY" (case insensitive)', () => {
      // Arrange
      mockBackendVersionService.getServiceInfo.mockReturnValue(of({ ...BACKEND_SERVICE_INFO, healthStatus: 'HEALTHY' }));
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      fixture.detectChanges();
      const cards = fixture.nativeElement.querySelectorAll('.card') as NodeListOf<HTMLElement>;

      // Assert
      expect(cards[1].classList).toContain('bg-success');
    });

    it('should apply bg-danger to the card when healthStatus is "Unhealthy"', () => {
      // Arrange
      mockBackendVersionService.getServiceInfo.mockReturnValue(of({ ...BACKEND_SERVICE_INFO, healthStatus: 'Unhealthy' }));
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      fixture.detectChanges();
      const cards = fixture.nativeElement.querySelectorAll('.card') as NodeListOf<HTMLElement>;

      // Assert
      expect(cards[1].classList).toContain('bg-danger');
    });

    it('should apply bg-danger to the card when healthStatus is "Degraded"', () => {
      // Arrange
      mockBackendVersionService.getServiceInfo.mockReturnValue(of({ ...BACKEND_SERVICE_INFO, healthStatus: 'Degraded' }));
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      fixture.detectChanges();
      const cards = fixture.nativeElement.querySelectorAll('.card') as NodeListOf<HTMLElement>;

      // Assert
      expect(cards[1].classList).toContain('bg-danger');
    });
  });

  describe('error handling', () => {
    it('should not show the toast when there is no error', () => {
      // Arrange
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      const toast = fixture.nativeElement.querySelector('app-toast');

      // Assert
      expect(toast).toBeNull();
    });

    it('should show the toast when the backend service errors', () => {
      // Arrange
      mockBackendVersionService.getServiceInfo.mockReturnValue(
        throwError(() => new Error('Unable to reach backend service. (0)')),
      );
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      fixture.detectChanges();
      const toast = fixture.nativeElement.querySelector('app-toast');

      // Assert
      expect(toast).not.toBeNull();
    });

    it('should display the error message in the toast', () => {
      // Arrange
      const errorMessage = 'Unable to reach backend service. (503)';
      mockBackendVersionService.getServiceInfo.mockReturnValue(throwError(() => new Error(errorMessage)));
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      fixture.detectChanges();
      const toastBody = fixture.nativeElement.querySelector('.toast-body') as HTMLElement;

      // Assert
      expect(toastBody.textContent?.trim()).toBe(errorMessage);
    });

    it('should show the backend card in an error state when the service errors', () => {
      // Arrange
      mockBackendVersionService.getServiceInfo.mockReturnValue(
        throwError(() => new Error('Unable to reach backend service. (0)')),
      );
      const fixture = TestBed.createComponent(VersionComponent);

      // Act
      fixture.detectChanges();
      fixture.detectChanges();
      const cards = fixture.nativeElement.querySelectorAll('.card') as NodeListOf<HTMLElement>;

      // Assert
      expect(cards.length).toBe(2);
    });

    it('should hide the toast after it is dismissed', () => {
      // Arrange
      mockBackendVersionService.getServiceInfo.mockReturnValue(
        throwError(() => new Error('Unable to reach backend service. (0)')),
      );
      const fixture = TestBed.createComponent(VersionComponent);
      fixture.detectChanges();
      fixture.detectChanges();

      // Act
      const closeButton = fixture.nativeElement.querySelector('button.btn-close') as HTMLButtonElement;
      closeButton.click();
      fixture.detectChanges();
      const toast = fixture.nativeElement.querySelector('app-toast');

      // Assert
      expect(toast).toBeNull();
    });
  });
});
