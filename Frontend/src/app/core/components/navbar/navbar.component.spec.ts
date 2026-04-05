import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { NavbarComponent } from './navbar.component';

describe('NavbarComponent', () => {
  let fixture: ComponentFixture<NavbarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NavbarComponent],
      providers: [provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    fixture.detectChanges();
  });

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

    it('should apply navbar-dark class', () => {
      // Arrange / Act
      const nav = fixture.nativeElement.querySelector('nav') as HTMLElement;

      // Assert
      expect(nav.classList).toContain('navbar-dark');
    });

    it('should apply bg-dark class', () => {
      // Arrange / Act
      const nav = fixture.nativeElement.querySelector('nav') as HTMLElement;

      // Assert
      expect(nav.classList).toContain('bg-dark');
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

    it('should display FileManager as the brand text', () => {
      // Arrange / Act
      const brand = fixture.nativeElement.querySelector('a.navbar-brand') as HTMLAnchorElement;

      // Assert
      expect(brand.textContent?.trim()).toBe('FileManager');
    });

    it('should render the folder icon as an inline SVG', () => {
      // Arrange / Act
      const icon = fixture.nativeElement.querySelector('a.navbar-brand svg') as SVGElement;

      // Assert
      expect(icon).toBeTruthy();
    });
  });

  describe('login button', () => {
    it('should render the login button', () => {
      // Arrange / Act
      const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

      // Assert
      expect(button).toBeTruthy();
    });

    it('should display Login as the button label', () => {
      // Arrange / Act
      const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

      // Assert
      expect(button.textContent?.trim()).toBe('Login');
    });

    it('should apply btn-outline-light class to the login button', () => {
      // Arrange / Act
      const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

      // Assert
      expect(button.classList).toContain('btn-outline-light');
    });

    it('should apply btn-sm class to the login button', () => {
      // Arrange / Act
      const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

      // Assert
      expect(button.classList).toContain('btn-sm');
    });

    it('should set type="button" on the login button', () => {
      // Arrange / Act
      const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

      // Assert
      expect(button.type).toBe('button');
    });
  });
});
