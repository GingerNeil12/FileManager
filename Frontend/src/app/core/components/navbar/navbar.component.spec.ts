import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from '@auth0/auth0-angular';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { NavbarComponent } from './navbar.component';

const MOCK_USER = {
  picture: 'https://example.com/avatar.png',
  name: 'Test User',
};

function createMockAuthService(isAuthenticated: boolean) {
  return {
    isAuthenticated$: of(isAuthenticated),
    user$: of(isAuthenticated ? MOCK_USER : null),
    loginWithRedirect: vi.fn(),
    logout: vi.fn(),
  };
}

describe('NavbarComponent', () => {
  describe('when unauthenticated', () => {
    let fixture: ComponentFixture<NavbarComponent>;
    let mockAuthService: ReturnType<typeof createMockAuthService>;

    beforeEach(async () => {
      mockAuthService = createMockAuthService(false);

      await TestBed.configureTestingModule({
        imports: [NavbarComponent],
        providers: [
          provideRouter([]),
          { provide: AuthService, useValue: mockAuthService },
        ],
      }).compileComponents();

      fixture = TestBed.createComponent(NavbarComponent);
      fixture.detectChanges();
    });

    afterEach(() => vi.clearAllMocks());

    it('should create the component', () => {
      // Arrange / Act / Assert
      expect(fixture.componentInstance).toBeTruthy();
    });

    describe('navbar element', () => {
      it('should render a nav element', () => {
        // Arrange / Act
        const nav = fixture.nativeElement.querySelector('nav') as HTMLElement;

        // Assert
        expect(nav).toBeTruthy();
      });

      it('should apply navbar-light class', () => {
        // Arrange / Act
        const nav = fixture.nativeElement.querySelector('nav') as HTMLElement;

        // Assert
        expect(nav.classList).toContain('navbar-light');
      });

      it('should apply bg-light class', () => {
        // Arrange / Act
        const nav = fixture.nativeElement.querySelector('nav') as HTMLElement;

        // Assert
        expect(nav.classList).toContain('bg-light');
      });

      it('should apply fixed-top class', () => {
        // Arrange / Act
        const nav = fixture.nativeElement.querySelector('nav') as HTMLElement;

        // Assert
        expect(nav.classList).toContain('fixed-top');
      });
    });

    describe('brand', () => {
      it('should render the brand link', () => {
        // Arrange / Act
        const brand = fixture.nativeElement.querySelector('a.navbar-brand') as HTMLAnchorElement;

        // Assert
        expect(brand).toBeTruthy();
      });

      it('should display File Manager as the brand text', () => {
        // Arrange / Act
        const brand = fixture.nativeElement.querySelector('a.navbar-brand') as HTMLAnchorElement;

        // Assert
        expect(brand.textContent?.trim()).toBe('File Manager');
      });

      it('should render the folder icon as an inline SVG', () => {
        // Arrange / Act
        const icon = fixture.nativeElement.querySelector('a.navbar-brand svg') as SVGElement;

        // Assert
        expect(icon).toBeTruthy();
      });
    });

    it('should render the Login button', () => {
      // Arrange / Act
      const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

      // Assert
      expect(button).toBeTruthy();
    });

    it('should not render the Logout button', () => {
      // Arrange / Act
      const buttons = fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>;
      const logoutButton = Array.from(buttons).find((b) => b.textContent?.trim() === 'Logout');

      // Assert
      expect(logoutButton).toBeUndefined();
    });

    it('should display "Login" as the button label', () => {
      // Arrange / Act
      const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

      // Assert
      expect(button.textContent?.trim()).toBe('Login');
    });

    it('should apply btn-outline-dark class to the Login button', () => {
      // Arrange / Act
      const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

      // Assert
      expect(button.classList).toContain('btn-outline-dark');
    });

    it('should apply btn-sm class to the Login button', () => {
      // Arrange / Act
      const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

      // Assert
      expect(button.classList).toContain('btn-sm');
    });

    it('should set type="button" on the Login button', () => {
      // Arrange / Act
      const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

      // Assert
      expect(button.type).toBe('button');
    });

    it('should call loginWithRedirect when the Login button is clicked', () => {
      // Arrange
      const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

      // Act
      button.click();

      // Assert
      expect(mockAuthService.loginWithRedirect).toHaveBeenCalledWith();
    });
  });

  describe('when authenticated', () => {
    let fixture: ComponentFixture<NavbarComponent>;
    let mockAuthService: ReturnType<typeof createMockAuthService>;

    beforeEach(async () => {
      mockAuthService = createMockAuthService(true);

      await TestBed.configureTestingModule({
        imports: [NavbarComponent],
        providers: [
          provideRouter([]),
          { provide: AuthService, useValue: mockAuthService },
        ],
      }).compileComponents();

      fixture = TestBed.createComponent(NavbarComponent);
      fixture.detectChanges();
    });

    afterEach(() => vi.clearAllMocks());

    it('should not render the Login button', () => {
      // Arrange / Act
      const buttons = fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>;
      const loginButton = Array.from(buttons).find((b) => b.textContent?.trim() === 'Login');

      // Assert
      expect(loginButton).toBeUndefined();
    });

    describe('avatar', () => {
      it('should render the avatar image', () => {
        // Arrange / Act
        const img = fixture.nativeElement.querySelector('img') as HTMLImageElement;

        // Assert
        expect(img).toBeTruthy();
      });

      it('should set src to user.picture', () => {
        // Arrange / Act
        const img = fixture.nativeElement.querySelector('img') as HTMLImageElement;

        // Assert
        expect(img.src).toBe(MOCK_USER.picture);
      });

      it('should set alt to user.name', () => {
        // Arrange / Act
        const img = fixture.nativeElement.querySelector('img') as HTMLImageElement;

        // Assert
        expect(img.alt).toBe(MOCK_USER.name);
      });

      it('should apply rounded-circle class', () => {
        // Arrange / Act
        const img = fixture.nativeElement.querySelector('img') as HTMLImageElement;

        // Assert
        expect(img.classList).toContain('rounded-circle');
      });
    });

    describe('dropdown menu', () => {
      it('should not show the dropdown menu by default', () => {
        // Arrange / Act
        const menu = fixture.nativeElement.querySelector('.dropdown-menu') as HTMLElement;

        // Assert
        expect(menu.classList).not.toContain('show');
      });

      it('should show the dropdown menu when avatar is clicked', () => {
        // Arrange
        const img = fixture.nativeElement.querySelector('img') as HTMLImageElement;

        // Act
        img.click();
        fixture.detectChanges();

        // Assert
        const menu = fixture.nativeElement.querySelector('.dropdown-menu') as HTMLElement;
        expect(menu.classList).toContain('show');
      });

      it('should hide the dropdown menu when avatar is clicked again', () => {
        // Arrange
        const img = fixture.nativeElement.querySelector('img') as HTMLImageElement;
        img.click();
        fixture.detectChanges();

        // Act
        img.click();
        fixture.detectChanges();

        // Assert
        const menu = fixture.nativeElement.querySelector('.dropdown-menu') as HTMLElement;
        expect(menu.classList).not.toContain('show');
      });

      it('should render a Logout option in the dropdown', () => {
        // Arrange / Act
        const item = fixture.nativeElement.querySelector('.dropdown-item') as HTMLButtonElement;

        // Assert
        expect(item.textContent?.trim()).toBe('Logout');
      });

      it('should call logout when Logout option is clicked', () => {
        // Arrange
        const item = fixture.nativeElement.querySelector('.dropdown-item') as HTMLButtonElement;

        // Act
        item.click();

        // Assert
        expect(mockAuthService.logout).toHaveBeenCalledWith({
          logoutParams: { returnTo: window.location.origin },
        });
      });

      it('should close the dropdown when clicking outside the navbar', () => {
        // Arrange
        const img = fixture.nativeElement.querySelector('img') as HTMLImageElement;
        img.click();
        fixture.detectChanges();

        // Act
        document.body.dispatchEvent(new MouseEvent('click', { bubbles: true }));
        fixture.detectChanges();

        // Assert
        const menu = fixture.nativeElement.querySelector('.dropdown-menu') as HTMLElement;
        expect(menu.classList).not.toContain('show');
      });
    });
  });
});
