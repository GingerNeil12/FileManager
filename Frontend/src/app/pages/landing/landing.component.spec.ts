import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthService } from '@auth0/auth0-angular';
import { BehaviorSubject, Subject } from 'rxjs';
import { vi } from 'vitest';

import { LandingComponent } from './landing.component';

function createMockAuthService(isAuthenticated: boolean, isLoading: boolean) {
  return {
    isAuthenticated$: new BehaviorSubject<boolean>(isAuthenticated),
    isLoading$: new BehaviorSubject<boolean>(isLoading),
    loginWithRedirect: vi.fn(),
  };
}

function createMockRouter() {
  return { navigate: vi.fn() };
}

describe('LandingComponent', () => {
  let fixture: ComponentFixture<LandingComponent>;
  let mockAuthService: ReturnType<typeof createMockAuthService>;
  let mockRouter: ReturnType<typeof createMockRouter>;

  async function setup(isAuthenticated: boolean, isLoading: boolean) {
    mockAuthService = createMockAuthService(isAuthenticated, isLoading);
    mockRouter = createMockRouter();

    await TestBed.configureTestingModule({
      imports: [LandingComponent],
      providers: [
        { provide: AuthService, useValue: mockAuthService },
        { provide: Router, useValue: mockRouter },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LandingComponent);
    fixture.detectChanges();
  }

  afterEach(() => vi.clearAllMocks());

  describe('creation', () => {
    it('should create the component', async () => {
      // Arrange / Act
      await setup(false, false);

      // Assert
      expect(fixture.componentInstance).toBeTruthy();
    });
  });

  describe('template — nav', () => {
    it('should render a nav element', async () => {
      // Arrange
      await setup(false, false);

      // Act
      const nav = fixture.nativeElement.querySelector('nav') as HTMLElement;

      // Assert
      expect(nav).toBeTruthy();
    });

    it('should display "FileManager" as the brand name', async () => {
      // Arrange
      await setup(false, false);

      // Act
      const brand = fixture.nativeElement.querySelector('.landing-brand') as HTMLElement;

      // Assert
      expect(brand.textContent?.trim()).toBe('FileManager');
    });

    it('should render the folder icon svg in the nav', async () => {
      // Arrange
      await setup(false, false);

      // Act
      const icon = fixture.nativeElement.querySelector('nav svg') as SVGElement;

      // Assert
      expect(icon).toBeTruthy();
    });

    it('should render the Log In button', async () => {
      // Arrange
      await setup(false, false);

      // Act
      const buttons = fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>;
      const loginButton = Array.from(buttons).find((b) => b.textContent?.trim() === 'Log In');

      // Assert
      expect(loginButton).toBeTruthy();
    });

    it('should set type="button" on the Log In button', async () => {
      // Arrange
      await setup(false, false);

      // Act
      const buttons = fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>;
      const loginButton = Array.from(buttons).find((b) => b.textContent?.trim() === 'Log In') as HTMLButtonElement;

      // Assert
      expect(loginButton.type).toBe('button');
    });
  });

  describe('template — hero', () => {
    it('should render the hero section', async () => {
      // Arrange
      await setup(false, false);

      // Act
      const hero = fixture.nativeElement.querySelector('.hero') as HTMLElement;

      // Assert
      expect(hero).toBeTruthy();
    });

    it('should display the hero headline "Your files. Secured."', async () => {
      // Arrange
      await setup(false, false);

      // Act
      const headline = fixture.nativeElement.querySelector('.hero-headline') as HTMLElement;

      // Assert
      expect(headline.textContent?.trim()).toBe('Your files. Secured.');
    });

    it('should display the hero sub-headline', async () => {
      // Arrange
      await setup(false, false);

      // Act
      const sub = fixture.nativeElement.querySelector('.hero-sub') as HTMLElement;

      // Assert
      expect(sub.textContent?.trim()).toBeTruthy();
    });

    it('should render the "Get Started" button', async () => {
      // Arrange
      await setup(false, false);

      // Act
      const buttons = fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>;
      const cta = Array.from(buttons).find((b) => b.textContent?.trim() === 'Get Started');

      // Assert
      expect(cta).toBeTruthy();
    });

    it('should set type="button" on the "Get Started" button', async () => {
      // Arrange
      await setup(false, false);

      // Act
      const buttons = fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>;
      const cta = Array.from(buttons).find((b) => b.textContent?.trim() === 'Get Started') as HTMLButtonElement;

      // Assert
      expect(cta.type).toBe('button');
    });
  });

  describe('template — feature cards', () => {
    it('should render three feature cards', async () => {
      // Arrange
      await setup(false, false);

      // Act
      const cards = fixture.nativeElement.querySelectorAll('.feature-card') as NodeListOf<HTMLElement>;

      // Assert
      expect(cards.length).toBe(3);
    });

    it('should display the "Upload" feature title', async () => {
      // Arrange
      await setup(false, false);

      // Act
      const titles = fixture.nativeElement.querySelectorAll('.feature-title') as NodeListOf<HTMLElement>;

      // Assert
      expect(titles[0].textContent?.trim()).toBe('Upload');
    });

    it('should display the "Share" feature title', async () => {
      // Arrange
      await setup(false, false);

      // Act
      const titles = fixture.nativeElement.querySelectorAll('.feature-title') as NodeListOf<HTMLElement>;

      // Assert
      expect(titles[1].textContent?.trim()).toBe('Share');
    });

    it('should display the "Control" feature title', async () => {
      // Arrange
      await setup(false, false);

      // Act
      const titles = fixture.nativeElement.querySelectorAll('.feature-title') as NodeListOf<HTMLElement>;

      // Assert
      expect(titles[2].textContent?.trim()).toBe('Control');
    });

    it('should render an icon svg in each feature card', async () => {
      // Arrange
      await setup(false, false);

      // Act
      const icons = fixture.nativeElement.querySelectorAll('.feature-card svg') as NodeListOf<SVGElement>;

      // Assert
      expect(icons.length).toBe(3);
    });
  });

  describe('template — footer', () => {
    it('should render the footer element', async () => {
      // Arrange
      await setup(false, false);

      // Act
      const footer = fixture.nativeElement.querySelector('footer') as HTMLElement;

      // Assert
      expect(footer).toBeTruthy();
    });

    it('should display copyright text in the footer', async () => {
      // Arrange
      await setup(false, false);

      // Act
      const footer = fixture.nativeElement.querySelector('footer') as HTMLElement;

      // Assert
      expect(footer.textContent).toContain('FileManager');
    });
  });

  describe('ngOnInit — auth redirect', () => {
    it('should navigate to /dashboard when not loading and authenticated', async () => {
      // Arrange / Act
      await setup(true, false);

      // Assert
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
    });

    it('should navigate to /dashboard exactly once', async () => {
      // Arrange / Act
      await setup(true, false);

      // Assert
      expect(mockRouter.navigate).toHaveBeenCalledTimes(1);
    });

    it('should not navigate when not loading and not authenticated', async () => {
      // Arrange / Act
      await setup(false, false);

      // Assert
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should not navigate while still loading', async () => {
      // Arrange
      const isLoading$ = new Subject<boolean>();
      mockAuthService = {
        isAuthenticated$: new BehaviorSubject<boolean>(true),
        isLoading$: isLoading$ as unknown as BehaviorSubject<boolean>,
        loginWithRedirect: vi.fn(),
      };
      mockRouter = createMockRouter();

      await TestBed.configureTestingModule({
        imports: [LandingComponent],
        providers: [
          { provide: AuthService, useValue: mockAuthService },
          { provide: Router, useValue: mockRouter },
        ],
      }).compileComponents();

      fixture = TestBed.createComponent(LandingComponent);
      fixture.detectChanges();

      // Assert — isLoading$ has not emitted yet
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should navigate to /dashboard after loading transitions from true to false when authenticated', async () => {
      // Arrange
      const isLoading$ = new BehaviorSubject<boolean>(true);
      mockAuthService = {
        isAuthenticated$: new BehaviorSubject<boolean>(true),
        isLoading$,
        loginWithRedirect: vi.fn(),
      };
      mockRouter = createMockRouter();

      await TestBed.configureTestingModule({
        imports: [LandingComponent],
        providers: [
          { provide: AuthService, useValue: mockAuthService },
          { provide: Router, useValue: mockRouter },
        ],
      }).compileComponents();

      fixture = TestBed.createComponent(LandingComponent);
      fixture.detectChanges();
      expect(mockRouter.navigate).not.toHaveBeenCalled();

      // Act — SDK finishes loading
      isLoading$.next(false);

      // Assert
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/dashboard']);
    });

    it('should not navigate after loading transitions from true to false when not authenticated', async () => {
      // Arrange
      const isLoading$ = new BehaviorSubject<boolean>(true);
      mockAuthService = {
        isAuthenticated$: new BehaviorSubject<boolean>(false),
        isLoading$,
        loginWithRedirect: vi.fn(),
      };
      mockRouter = createMockRouter();

      await TestBed.configureTestingModule({
        imports: [LandingComponent],
        providers: [
          { provide: AuthService, useValue: mockAuthService },
          { provide: Router, useValue: mockRouter },
        ],
      }).compileComponents();

      fixture = TestBed.createComponent(LandingComponent);
      fixture.detectChanges();

      // Act — SDK finishes loading
      isLoading$.next(false);

      // Assert
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });

    it('should only react to the first emission after loading resolves', async () => {
      // Arrange
      const isLoading$ = new BehaviorSubject<boolean>(false);
      const isAuthenticated$ = new BehaviorSubject<boolean>(false);
      mockAuthService = {
        isAuthenticated$,
        isLoading$,
        loginWithRedirect: vi.fn(),
      };
      mockRouter = createMockRouter();

      await TestBed.configureTestingModule({
        imports: [LandingComponent],
        providers: [
          { provide: AuthService, useValue: mockAuthService },
          { provide: Router, useValue: mockRouter },
        ],
      }).compileComponents();

      fixture = TestBed.createComponent(LandingComponent);
      fixture.detectChanges();

      // Act — subsequent authenticated emission after take(1) has already fired
      isAuthenticated$.next(true);

      // Assert — navigate should still not have been called (was not authenticated on first emission)
      expect(mockRouter.navigate).not.toHaveBeenCalled();
    });
  });

  describe('handleLogin', () => {
    it('should call loginWithRedirect when "Get Started" button is clicked', async () => {
      // Arrange
      await setup(false, false);
      const buttons = fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>;
      const cta = Array.from(buttons).find((b) => b.textContent?.trim() === 'Get Started') as HTMLButtonElement;

      // Act
      cta.click();

      // Assert
      expect(mockAuthService.loginWithRedirect).toHaveBeenCalledWith();
    });

    it('should call loginWithRedirect when "Log In" button is clicked', async () => {
      // Arrange
      await setup(false, false);
      const buttons = fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>;
      const loginButton = Array.from(buttons).find((b) => b.textContent?.trim() === 'Log In') as HTMLButtonElement;

      // Act
      loginButton.click();

      // Assert
      expect(mockAuthService.loginWithRedirect).toHaveBeenCalledWith();
    });

    it('should call loginWithRedirect exactly once per "Get Started" click', async () => {
      // Arrange
      await setup(false, false);
      const buttons = fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>;
      const cta = Array.from(buttons).find((b) => b.textContent?.trim() === 'Get Started') as HTMLButtonElement;

      // Act
      cta.click();

      // Assert
      expect(mockAuthService.loginWithRedirect).toHaveBeenCalledTimes(1);
    });

    it('should call loginWithRedirect exactly once per "Log In" click', async () => {
      // Arrange
      await setup(false, false);
      const buttons = fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>;
      const loginButton = Array.from(buttons).find((b) => b.textContent?.trim() === 'Log In') as HTMLButtonElement;

      // Act
      loginButton.click();

      // Assert
      expect(mockAuthService.loginWithRedirect).toHaveBeenCalledTimes(1);
    });
  });
});
